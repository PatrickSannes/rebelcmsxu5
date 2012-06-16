using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// A mapper that performs mapping the properties of two objects taking into account any rules applied for each 
    /// member. If mapping cannot be directly applied, the process is sent back to the engine to attempt the mapping.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    public class ObjectMapper<TSource, TTarget>
    {
        private readonly AbstractTypeMapper<TSource, TTarget> _mapper;
        private readonly HashSet<int> _memberMapped = new HashSet<int>();

        public ObjectMapper(AbstractTypeMapper<TSource, TTarget> mapper)
        {
            _mapper = mapper;
        }

        public void Map(object source, object target, MappingExecutionScope scope)
        {
            var ci = new PropertyMapDefinition
            {
                Source =
                {
                    Type = source.GetType(),
                    Value = source
                },
                Target =
                {
                    Type = target.GetType(),
                    Value = target
                }
            };

            var sourceProps = source.GetProperties();
            var targetProps = target.GetProperties();

            //create MemberExpressionSignature for each property
            var targetPropDefs = targetProps.Cast<PropertyDescriptor>()
                .Select(x => new
                {
                    expression = new MemberExpressionSignature(
                        x.PropertyType,
                        typeof(TTarget),
                        x.Name),
                    property = x
                }).ToArray();

            //before we map the members by name, we need to ignore any members that have been referenced in the member expressions
            var filteredTargetProps = new PropertyDescriptorCollection(
                (from t in targetPropDefs
                 where !_mapper.MappingContext.MemberExpressions.Any(x => x.Equals(t.expression))
                 select t.property).ToArray());

            foreach (PropertyDescriptor s in sourceProps)
            {
                var t = filteredTargetProps.Find(s.Name, false);
                if (t == null) continue;
                var reflectedProp = ci.Target.Type.GetProperty(t.Name);
                if (reflectedProp == null) continue;
                if (!reflectedProp.CanWrite) continue;

                ci.SourceProp.Name = s.Name;
                ci.SourceProp.Value = s.GetValue(source);
                ci.SourceProp.Type = s.PropertyType;
                ci.TargetProp.Name = t.Name;
                ci.TargetProp.Type = t.PropertyType;
                ci.TargetProp.Value = t.GetValue(target);

                //if people have overridden this injector, we'll need to check that this hasn't already been mapped
                if (IsAlreadyMapped(ci))
                    continue;

                //check that we can proceed with setting this value
                if (IsTypeMappable(s.PropertyType, t.PropertyType))
                {
                    //set the output value to the source
                    ci.TargetProp.Value = ci.SourceProp.Value;
                }
                else
                {
                    var hasConverted = false;
                    // If no explicit mapping exists, but a TypeConverter exists between the source & destination types, use that
                    if (TypeFinder.IsImplicitValueType(ci.TargetProp.Type) && 
                        !IsMapperRegistered(ci.SourceProp.Type, ci.TargetProp.Type))
                    {
                        var converter = TypeDescriptor.GetConverter(ci.SourceProp.Type);
                        if (converter != null && converter.CanConvertTo(ci.TargetProp.Type))
                        {
                            ci.TargetProp.Value = converter.ConvertTo(ci.SourceProp.Value, ci.TargetProp.Type);
                            hasConverted = true;
                        }
                        else
                        {
                            converter = TypeDescriptor.GetConverter(ci.TargetProp.Type);
                            if (converter != null && converter.CanConvertFrom(ci.SourceProp.Type))
                            {
                                ci.TargetProp.Value = converter.ConvertFrom(ci.SourceProp.Value);
                                hasConverted = true;
                            }
                        }
                    }

                    if (!hasConverted)
                        // If we can't simply map it by setting the value, then we'll send the operation back through the mapper
                        // Also, if it's not a reference type, we can't map it in place and have to set the value of the property to a new value
                        if (TypeFinder.IsImplicitValueType(ci.TargetProp.Type) || ci.TargetProp.Value == null)
                        {
                            //map to new
                            var val = MapToNew(ci.SourceProp.Value, ci.SourceProp.Type, ci.TargetProp.Type, scope);                            
                            ci.TargetProp.Value = val;
                        }
                        else
                        {
                            //map to existing
                            MapToExisting(ci.SourceProp.Value, ci.TargetProp.Value, ci.SourceProp.Type, ci.TargetProp.Type);                            
                        }
                }

                SetProperty(reflectedProp, target, ci.TargetProp.Value, scope);
                //tag this as already mapped
                AddMemberMapped(ci);

            }

            //now that we've mapped by name, lets map by rule
            foreach (var signature in _mapper.MappingContext.MemberExpressions)
            {

                //get the target property definition with the expression mapped
                var targetProp = targetPropDefs.Single(x => x.expression.Equals(signature));

                var reflectedProp = ci.Target.Type.GetProperty(targetProp.property.Name);
                if (reflectedProp == null)
                    throw new MissingMemberException("Could not access property " + targetProp.property.Name + " on object Type " + targetProp.property.ComponentType.Name);

                //fill in the TypeMapDefinition object
                ci.TargetProp.Name = targetProp.property.Name;
                ci.TargetProp.Type = targetProp.property.PropertyType;

                //now, try to map from a rule, if successful set the value... the MapFromRule will automatically add the Info to the already mapped list.
                object val;
                if (!IsAlreadyMapped(ci) && _mapper.MappingContext.MapFromRule(ci, targetProp.expression, out val))
                {
                    if (!reflectedProp.CanWrite)
                        throw new MemberAccessException("A member expression has been declared for a writeable mapping operation for property " + targetProp.property.Name + " on object Type " + targetProp.property.ComponentType.Name + " but this property is readonly and cannot be written to");
                    SetProperty(reflectedProp, target, val, scope);
                    //tag this as already mapped
                    AddMemberMapped(ci);
                }
            }

        }

        /// <summary>
        /// Sets the target properties value
        /// </summary>
        /// <param name="targetReflectedProp"></param>
        /// <param name="targetObject"></param>
        /// <param name="valueToSet"></param>
        /// <param name="scope"></param>
        /// <remarks>
        /// This will check if the value can be set natively, if not it will check if a map is declared for the types and use it if one is found.
        /// </remarks>
        private void SetProperty(PropertyInfo targetReflectedProp, object targetObject, object valueToSet, MappingExecutionScope scope)
        {
            //Set the value using the expression compilation in DynamicMemberAccess, NOT the property descriptor since [ReadOnly] attributes will
            //prevent the property descriptor from setting the value.
            //Check that the value can be assigned, if not we'll ask the mapper to map it.
            if (valueToSet == null || targetReflectedProp.PropertyType.IsAssignableFrom(valueToSet.GetType()))
            {
                TypeFinder.DynamicMemberAccess.SetterDelegate(targetReflectedProp, true).Invoke(targetObject, valueToSet);
            }
            else
            {
                //if we can't set the value natively, then lets check if we have a mapping definition in the type mappers collection
                var hasMapper = IsMapperRegistered(valueToSet.GetType(), targetReflectedProp.PropertyType);
                if (hasMapper)
                {
                    //run the mapping operation and then set the value to the output
                    var mappedVal = MapToNew(valueToSet, valueToSet.GetType(), targetReflectedProp.PropertyType, scope);
                    TypeFinder.DynamicMemberAccess.SetterDelegate(targetReflectedProp, true).Invoke(targetObject, mappedVal);
                }
                else
                {
                    throw new NotSupportedException("Cannot map from Type " + targetReflectedProp.PropertyType.Name + " to Type " + valueToSet.GetType().Name + " because there is no mapping defined for this conversion");
                }
            }
        }

        /// <summary>
        /// Check that values can be set from -> to without any specific casting (i.e. Non-nullables to nullable)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        protected bool IsTypeMappable(Type from, Type to)
        {
            if (Nullable.GetUnderlyingType(from) == to)
                return true;
            if (from == Nullable.GetUnderlyingType(to))
                return true;
            if (from == typeof(int) && to.IsSubclassOf(typeof(Enum)))
                return true;
            if (from.IsSubclassOf(typeof(Enum)) && to == typeof(int))
                return true;
            if (from != to)
                return false;

            return true;
        }

        /// <summary>
        /// Maps to an existing value,this first checks for mappers in the local engine and if not found will then try to get 
        /// a mapper from the framework context's engine collection, if nothing is found there either a map is implicitly created.
        /// </summary>
        /// <param name="sourceVal"></param>
        /// <param name="targetVal"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        protected void MapToExisting(object sourceVal, object targetVal, Type from, Type to)
        {
            //first check registration with local engine
            var found = _mapper.MappingContext.Engine.TryGetMapper(from, to, false);
            if (found.Success)
            {
                var localDef = _mapper.MappingContext.Engine.GetMapperDefinition(from, to, false);
                localDef.Mapper.Map(sourceVal, targetVal);
                return;
            }

            //then check registration with framework context's engine collection
            var mapper = _mapper.MappingContext.FrameworkContext.TypeMappers.GetMapHandler(from, to);
            if (mapper != null)
            {
                mapper.Map(sourceVal, targetVal, from, to);
                return;
            }

            //implicitly map it
            _mapper.MappingContext.Engine.Map(sourceVal, targetVal, from, to);
        }

        /// <summary>
        /// Maps to a new value, this first checks for mappers in the local engine and if not found will then try to get 
        /// a mapper from the framework context's engine collection, if nothing is found there either a map is implicitly created.
        /// </summary>
        /// <param name="sourceVal"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        protected object MapToNew(object sourceVal, Type from, Type to, MappingExecutionScope scope)
        {
            Mandate.ParameterNotNull(from, "from");
            Mandate.ParameterNotNull(to, "to");

            if (sourceVal == (from.GetDefaultValue()))
            {
                return from.GetDefaultValue();
            }
                

            //first check registration with local engine
            var found = _mapper.MappingContext.Engine.TryGetMapper(from, to, false);
            if (found.Success)
            {
                var localDef = _mapper.MappingContext.Engine.GetMapperDefinition(from, to, false);
                return localDef.Mapper.Map(sourceVal, scope);
            }

            //then check registration with framework context's engine collection
            var mapper = _mapper.MappingContext.FrameworkContext.TypeMappers.GetMapHandler(from, to);
            if (mapper != null)
            {
                return mapper.Map(sourceVal, from, to);
            }

            //nothing found, this will create an implicit map
            return _mapper.MappingContext.Engine.Map(sourceVal, from, to, scope);
        }

        /// <summary>
        /// Checks if a mapper is registered for the from and to types, this first checks local registration with the 
        /// current Engine and if not found, then checks registration with the framework context's type mapper collection
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        protected bool IsMapperRegistered(Type from, Type to)
        {
            //first check locally registered in this engine
            var found = _mapper.MappingContext.Engine.TryGetMapper(from, to, false).Success;
            if (found) return true;

            //now check if its registered with the framework context mapping collection
            var mapper = _mapper.MappingContext.FrameworkContext.TypeMappers.GetMapHandler(from, to);
            if (mapper != null) return true;

            return false;
        }


        /// <summary>
        /// Adds the hash code of the member that has been mapped
        /// </summary>
        /// <param name="member">The member to get the hashcode from</param>
        protected void AddMemberMapped(PropertyMapDefinition member)
        {
            _memberMapped.Add(member.GetTargetObjectHashCode());
        }

        /// <summary>
        /// Determines whether the member is already mapped
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>
        ///   <c>true</c> if [is already mapped] [the specified member]; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsAlreadyMapped(PropertyMapDefinition member)
        {
            return _memberMapped.Contains(member.GetTargetObjectHashCode());
        }

    }
}
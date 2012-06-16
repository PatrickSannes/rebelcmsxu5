using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Umbraco.Framework.TypeMapping
{

    /// <summary>
    /// Maps an IEnumerable type to another
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <remarks>
    /// When mapping to an existing destination, this will only work if the underlying IEnumerable type is IEnumerable{T} when mapping to a new object,
    /// otherwise if mapping to an existing object, the collection needs to be anything that has an Add and Clear method such as an IList or IDictionary otherwise
    /// you will have to inherit from this class.
    /// </remarks>
    [DebuggerDisplay("Mapper: {SourceType} -> {TargetType}")]
    public class EnumerableTypeMapper<TSource, TTarget> : AbstractTypeMapper<TSource, TTarget>
    {
        public EnumerableTypeMapper(AbstractFluentMappingEngine engine) : base(engine)
        {
            Mandate.That(TypeFinder.IsTypeAssignableFrom<IEnumerable>(typeof (TSource)), x => new InvalidOperationException("EnumerableTypeMapper is only used for source and destination types of IEnumerable"));
            Mandate.That(TypeFinder.IsTypeAssignableFrom<IEnumerable>(typeof (TTarget)), x => new InvalidOperationException("EnumerableTypeMapper is only used for source and destination types of IEnumerable"));
        }

        private MethodInfo _addMethod;
        private MethodInfo _clearMethod;
        private Type _enumerableTargetItemType;
        private Type _enumerableSourceItemType;

        public override TTarget GetTargetFromScope(TSource source, MappingExecutionScope scope)
        {
            return scope.CreateOnce(
                source,
                () => MappingContext.CreateUsingAction == null
                      //create a new list
                          ? CreateTarget()
                      //create the collection based on the CreateUsing specified
                          : MappingContext.CreateUsingAction(source));
        }

        public override TTarget Map(TSource source, MappingExecutionScope scope)
        {
            if ((object)source == typeof(TSource).GetDefaultValue())
                return default(TTarget);

            var target = GetTargetFromScope(source, scope);

            PerformMapping(source, target, scope);

            ExecuteAfterMap(source, target);

            return target;
        }


        /// <summary>
        /// Maps the specified source to the existing destination
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="scope">The scope.</param>
        protected override void PerformMap(TSource source, TTarget target, MappingExecutionScope scope)
        {
            if ((object)source == typeof(TSource).GetDefaultValue())
                return;

            PerformMapping(source, target, scope);
        }

        /// <summary>
        /// This will search each base type of this object until it finds IEnumerable{T}, then will return the type of {T}.
        /// If TTarget doesn't implement IEnumerable{T} an exception is thrown
        /// </summary>
        /// <returns></returns>
        private static Type GetEnumerableItemType<T>()
        {
            Type[] collectionItemType;
            if (!typeof(T).TryGetGenericArguments(typeof(IEnumerable<>), out collectionItemType))
            {
                throw new NotSupportedException("EnumerableTypeMapper will only work for types derived from IEnumerable<T>, otherwise the class will need to be overridden to allow custom collections");
            }

            //now that listType is of type IEnumerable<T>, return its generic argument type
            return collectionItemType[0];
        }

        /// <summary>
        /// Trys to create the target type if no CreateUsing is specified, if it can't create the native type, the it will create a generic list and attempt to cast it back to TTarget.
        /// </summary>
        /// <returns></returns>
        private TTarget CreateTarget()
        {
            object t;
            try
            {
                t = Creator.Create(typeof(TTarget));
            }
            catch (ArgumentException)
            {
                //throw the argument exceptions
                throw;
            }
            catch(Exception)
            {
                //there was another issue trying to create thsi type, so lets revert to creating a list to fill in hopes we can assign it back to the enumerable collection
                t = Creator.Create(typeof(List<>).MakeGenericType(GetEnumerableTargetItemType()));
            }
            try
            {
                return (TTarget)t;
            }
            catch (InvalidCastException ex)
            {                
                throw new InvalidOperationException("Cannot implicitly create collection type of " + typeof(TTarget).Name + ". A CreateUsing expression must be declared.", ex);
            }
        }

        /// <summary>
        /// Override this method to specify the explicit item type of the Enumerable collection
        /// </summary>
        /// <returns></returns>
        protected virtual Type GetEnumerableSourceItemType()
        {
            if (_enumerableSourceItemType == null)
            {
                _enumerableSourceItemType = GetEnumerableItemType<TSource>();
            }
            return _enumerableSourceItemType;
        }

        /// <summary>
        /// Override this method to specify the explicit item type of the Enumerable collection
        /// </summary>
        /// <returns></returns>
        protected virtual Type GetEnumerableTargetItemType()
        {
            if (_enumerableTargetItemType == null)
            {
                _enumerableTargetItemType = GetEnumerableItemType<TTarget>();    
            }
            return _enumerableTargetItemType;            
        }

        /// <summary>
        /// Override this method to 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="enumerableTarget">pass in  the target object as it may be required to create a child item object</param>
        /// <returns></returns>
        protected virtual object CreateEnumerableItem(TSource source, TTarget enumerableTarget)
        {
            var t = Creator.Create(GetEnumerableTargetItemType());
            return t;
        }

        /// <summary>
        /// Does the adding of an item to the collection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        protected virtual void PerformAddItem(object collection, object item)
        {
            if (_addMethod == null)
            {
                _addMethod = collection.GetType().GetMethod("Add");
                Mandate.That(_addMethod != null, x => new NotSupportedException("Cannot modify a non mutable IEnumerable type"));    
            }

            _addMethod.Invoke(collection, new[] { item });
        }

        /// <summary>
        /// Does the clearing of the collection
        /// </summary>
        /// <param name="collection"></param>
        protected virtual void PerformClear(object collection)
        {
            if (_clearMethod == null)
            {
                _clearMethod = collection.GetType().GetMethod("Clear");
                Mandate.That(_clearMethod != null, x => new NotSupportedException("Cannot modify a non mutable IEnumerable type"));
            }

            _clearMethod.Invoke(collection, null);
        }

        /// <summary>
        /// Helper method to do the add/clear of the IEnumerable
        /// </summary>
        /// <param name="sourceSequence"></param>
        /// <param name="targetSequence"></param>
        /// <param name="mappingExecutionScope"></param>
        protected virtual void PerformMapping(TSource sourceSequence, TTarget targetSequence, MappingExecutionScope mappingExecutionScope)
        {                        
            //first, clear the items
            PerformClear(targetSequence);
            var targetItemType = GetEnumerableTargetItemType();
            var sourceItemType = GetEnumerableSourceItemType();
            foreach (var source in (IEnumerable)sourceSequence)
            {
                var targetToAdd = MappingContext.Engine.Map(source, sourceItemType, targetItemType, mappingExecutionScope);

                PerformAddItem(targetSequence, targetToAdd);                
            }
        }

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Umbraco.Framework.Dynamics
{
    /// <summary>
    /// A pliable object for representing a key-value store at runtime, which supports a deep object graph and member access by both indexer and member name.
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("Bendy: {BendyObjectName}, {Value}")]
    public class BendyObject : DynamicNullableValueObject, INotifyPropertyChanged, IEnumerable<BendyObject>
    {
        internal protected bool IsLeaf;
        internal protected bool IsEmptyLeaf;

        public Func<BendyObject, string, object> WhenItemNotFound { get; set; }

        /// <summary>
        /// Enables derived types to initialize a new instance of the <see cref="T:System.Dynamic.DynamicObject"/> type.
        /// </summary>
        public BendyObject()
        {
            BendyObjectName = Guid.NewGuid().ToString("N");
            __KeyedChildren = new BendyObjectCollection();
        }

        /// <summary>
        /// Enables derived types to initialize a new instance of the <see cref="T:System.Dynamic.DynamicObject"/> type.
        /// </summary>
        protected BendyObject(string synthNodeName, object value)
            : this()
        {
            BendyObjectName = synthNodeName;
            BendyObjectValue = value;
            IsLeaf = true;
            IsEmptyLeaf = value == null;
        }

        /// <summary>
        /// Creates a new BendyObject instance. If <paramref name="value"/> is of type <see cref="BendyObject"/>, it is passed to <see cref="AddOrUpdateChild"/>.
        /// Otherwise, a delegate for accessing each readable property of the object is added to this instance by calling <see cref="AddLazy"/>.
        /// </summary>
        public BendyObject(dynamic value)
            : this()
        {
            if (value is BendyObject)
            {
                AddOrUpdateChild(value as BendyObject);
            }
            else if (value is IDictionary)
            {
                var dictionary = (IDictionary)value;
                foreach (var key in dictionary.Keys)
                {
                    var localKey = key;
                    AddLazy(key.ToString(), () => dictionary[localKey]);
                }
            }
            else
            {
                //Value = value;
                Type incomingType = value.GetType();
                foreach (var propertyInfo in incomingType.GetProperties())
                {
                    if (!propertyInfo.CanRead) continue;

                    var localInfo = propertyInfo;
                    AddLazy(propertyInfo.Name, () => localInfo.GetValue(value, null));
                }
            }

            IsLeaf = true;
        }

        /// <summary>
        /// Gets or sets the member on this dynamic object with the specified name.
        /// </summary>
        /// <remarks></remarks>
        public dynamic this[string member]
        {
            get { return GetMember(member); }
            set { SetMember(member, value); }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks></remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when a property changes, and raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <remarks></remarks>
        void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Gets or sets the name of the bendy object.
        /// </summary>
        /// <value>The name of the bendy object.</value>
        /// <remarks></remarks>
        public string BendyObjectName { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks></remarks>
        public dynamic BendyObjectValue { get; set; }

        /// <summary>
        /// Gets or sets the keyed children.
        /// </summary>
        /// <value>The keyed children.</value>
        /// <remarks></remarks>
        public BendyObjectCollection __KeyedChildren { get; set; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <remarks></remarks>
        public IEnumerable<BendyObject> __BendyChildren { get { return __KeyedChildren.Values; } }

        /// <summary>
        /// Adds or updates a child where the name matches.
        /// </summary>
        /// <param name="bendyObject">The bendy object.</param>
        /// <remarks></remarks>
        public void AddOrUpdateChild(BendyObject bendyObject)
        {
            __KeyedChildren.AddOrUpdate(bendyObject.BendyObjectName, bendyObject, (key, oldVal) => bendyObject);
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, the <paramref name="value"/> is "Test".</param>
        /// <returns>true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)</returns>
        /// <remarks></remarks>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var memberName = binder.Name;

            SetMember(memberName, value);

            return true;
        }

        /// <summary>
        /// Sets the member.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="value">The value.</param>
        /// <remarks></remarks>
        private void SetMember(string memberName, object value)
        {
            if (value is BendyObject)
            {
                AddOrUpdateChild(value as BendyObject);
            }

            // Try enumerables
            var asDynamic = value as IEnumerable<dynamic>;
            if (asDynamic != null)
            {
                //var addCompoundChild = new BendyObject(memberName, null);
                //foreach (var toAdd in asDynamic.Select(item => item as BendyObject ?? new BendyObject(memberName, item)))
                //{
                //    addCompoundChild.AddOrUpdateChild(toAdd);
                //}
                var addCompoundChild = new BendyObject(memberName, asDynamic);
                AddOrUpdateChild(addCompoundChild);
            }
            else
            {
                var synth = new BendyObject(memberName, value);
                AddOrUpdateChild(synth);
            }

            OnPropertyChanged(memberName);
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
        /// <returns>true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)</returns>
        /// <remarks></remarks>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var membername = binder.Name;

            result = GetMember(membername);

            return true;
        }

        private readonly HashSet<DynamicMethod> __bendyMethods = new HashSet<DynamicMethod>();
        public void AddMethod(DynamicMethod method)
        {
            __bendyMethods.Add(method);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var name = binder.Name;

            var matchingByName = __bendyMethods
                .Where(x => x.Signature.Name == name)
                .ToArray();

            var withSameParamCount = matchingByName
                .Where(method => ExcludeFirstBendyParameter(method).Count() == args.Length)
                .ToArray();

            if (withSameParamCount.Any())
            {
                if (withSameParamCount.Count() == 1)
                {
                    var onlyMethod = withSameParamCount.First();
                    if (InvokeDynamicMethodBody(args, out result, onlyMethod)) return true;
                }

                // Figure out the best match based on the supplied parameter types
                Expression<Func<DynamicMethod, bool>> buildPredicate =
                    method => ExcludeFirstBendyParameter(method).Count() == args.Length;
                for (int i = 0; i < args.Length; i++)
                {
                    int index = i;
                    buildPredicate = buildPredicate.And(method => CompareParamTypesStrict(args, method, index));
                }

                var methodToInvoke = matchingByName.Where(buildPredicate.Compile()).FirstOrDefault();

                if (methodToInvoke != null)
                {
                    if (InvokeDynamicMethodBody(args, out result, methodToInvoke)) return true;
                }
            }

            var allMethods = GetMethodsAsStringList(matchingByName);
            throw new InvalidOperationException(
                "Could not finding matching Signature for '{0}' with {1} parameters. Did you mean to call one of these?\n{2}".InvariantFormat(
                    name, args.Length, allMethods));
        }

        private static string GetMethodsAsStringList(IEnumerable<DynamicMethod> dynamicMethods)
        {
            var sb = new StringBuilder();
            foreach (var method in dynamicMethods)
            {
                sb.Append(method.Signature.ToString() + "\n");
            }
            var allMethods = sb.ToString();
            return allMethods;
        }

        private static IEnumerable<Parameter> ExcludeFirstBendyParameter(DynamicMethod x)
        {
            var excludeFirstBendyParameter = x.Signature.Parameters.ToArray();
            if (!excludeFirstBendyParameter.Any()) return Enumerable.Empty<Parameter>();
            return excludeFirstBendyParameter.First().Type == typeof(BendyObject) ? excludeFirstBendyParameter.Skip(1) : excludeFirstBendyParameter;
        }

        private bool InvokeDynamicMethodBody(object[] args, out object result, DynamicMethod methodToInvoke)
        {
            var count = ExcludeFirstBendyParameter(methodToInvoke).Count();
            if (methodToInvoke.Signature.ReturnsValue)
            {
                // Need to compile Funcs etc.
                switch (count)
                {
                    case 1:
                        var local = methodToInvoke.Body as LambdaExpression;
                        result = local.Compile().DynamicInvoke(Enumerable.Repeat(this, 1).Concat(args).ToArray());
                        return true;
                }
            }
            else
            {
                switch (count)
                {
                    case 0:
                        result = ((LambdaExpression) methodToInvoke.Body).Compile().DynamicInvoke(this);
                        return true;
                    case 1:
                        var local = methodToInvoke.Body as LambdaExpression;
                        result = local.Compile().DynamicInvoke(Enumerable.Repeat(this, 1).Concat(args).ToArray());
                        return true;
                }
            }
            result = null;
            return false;
        }

        private static bool CompareParamTypesLoose(IList<object> args, DynamicMethod x, int index)
        {
            var parameters = x.Signature.Parameters.ToArray();
            if (index > parameters.Length || index > args.Count) return false;
            return TypeFinder.IsTypeAssignableFrom(parameters[index].Type, args[index].GetType());
        }

        private static bool CompareParamTypesStrict(IList<object> args, DynamicMethod x, int index)
        {
            var parameters = ExcludeFirstBendyParameter(x).ToArray();
            if (index > parameters.Length || index > args.Count) return false;
            return parameters[index].Type == args[index].GetType();
        }

        /// <summary>
        /// Gets the member.
        /// </summary>
        /// <param name="membername">The membername.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private dynamic GetMember(string membername)
        {
            dynamic result = null;
            var foundChild = __KeyedChildren
                .Where(x => x.Key == membername)
                .Select(x => new { MemberName = x.Key, Bendy = x.Value })
                .FirstOrDefault();

            // If the member doesn't exist yet, we need to auto-create a child Bendy
            // so that implicit creation of children using just dot-notation works (neat)
            if (foundChild == null)
            {
                // Now execute WhenItemNotFound to see if it should be used
                if (WhenItemNotFound != null)
                {
                    var obj = WhenItemNotFound.Invoke(this, membername);
                    if (obj != null)
                    {
                        SetMember(membername, obj);
                        result = obj;
                    }
                }
                else
                {
                    var synth = new BendyObject(membername, null);
                    AddOrUpdateChild(synth);

                    result = synth;
                }
            }
            else
            {
                // If the found child has children, return the full Bendy object, otherwise just the value
                var hasChildren = foundChild.Bendy.__BendyChildren.Any();

                if (!hasChildren && foundChild.Bendy.IsLeaf && foundChild.Bendy.IsEmptyLeaf)
                    result = foundChild.Bendy;
                else
                {
                    result = hasChildren
                        ? foundChild.Bendy
                        : foundChild.Bendy.GetOrCreateValue();
                }
            }
            return result;
        }

        protected override object GetOrCreateValue()
        {
            var value = BendyObjectValue;

            // If the value is deferred, execute it
            // Use ReferenceEquals for the null check in case a type we're storing doesn't support != operator for nulls
            if (!ReferenceEquals(value, null))
            {
                var actionCallback = value as Func<dynamic>;

                if (actionCallback != null)
                {
                    // Execute the callback and also replace the value so we don't execute it again next time
                    dynamic delegateValue = actionCallback.Invoke();

                    BendyObjectValue = delegateValue;
                }
                else
                {
                    var funcCallback = value as Func<dynamic, dynamic>;

                    if (funcCallback != null)
                    {
                        dynamic delegateValue = funcCallback.Invoke(this);

                        BendyObjectValue = delegateValue;
                    }
                }
            }

            return BendyObjectValue;
        }

        public static implicit operator string(BendyObject other)
        {
            return other.ToString();
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(BendyObject))
            {
                if (__BendyChildren.Count() == 0)
                {
                    result = new DynamicNullableValueObject();
                    return true;
                }
            }
            return base.TryConvert(binder, out result);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The x.</param>
        /// <param name="right">The y.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks></remarks>
        public static bool operator ==(BendyObject left, BendyObject right)
        {
            // If both are null, or both are same instance, or both have null Values, or both Values are the same reference, return true.
            if (ReferenceEquals(left, right))
                return true;

            // If one is null, and the other has no children, return true in order to "fake" that it is a null reference
            if (FakeNullEquality(left, right)) return true;
            if (FakeNullEquality(right, left)) return true;

            // Else if one is null, but not both, return false.
            if (((object)left == null) || ((object)right == null))
                return false;

            if (left.IsLeaf)
                return ReferenceEquals(left.BendyObjectName, right.BendyObjectValue);

            // If we get here just compare the Values
            return left.__KeyedChildren == right.__KeyedChildren;
        }

        private static bool FakeNullEquality(BendyObject x, BendyObject y)
        {
            return ReferenceEquals(x, null) && !y.__BendyChildren.Any();
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks></remarks>
        public static bool operator !=(BendyObject x, BendyObject y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<BendyObject> GetEnumerator()
        {
            var thisValueAsEnum = BendyObjectValue as IEnumerable<BendyObject>;
            return thisValueAsEnum != null ? thisValueAsEnum.GetEnumerator() : Enumerable.Empty<BendyObject>().GetEnumerator();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public override bool Equals(object obj)
        {
            // If we're comparing to null, return true if our Value is also null
            // This is because we have to return a new BendyObject when a new member is accessed
            // in order to do a deep object graph, but we still need to check for nulls
            if (obj == null)
            {
                return ReferenceEquals(BendyObjectValue, null);
            }

            // If param cannot be cast to BendyObject return false
            var bendyObject = obj as BendyObject;
            if (ReferenceEquals(bendyObject, null)) return false;

            // Check field equality
            return (BendyObjectValue == bendyObject.BendyObjectValue) && (__BendyChildren.SequenceEqual(bendyObject.__BendyChildren));
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return BendyObjectValue == null ? __BendyChildren.GetHashCode() : BendyObjectValue.GetHashCode();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds a lazy-loaded property
        /// </summary>
        /// <param name="memberName">Name of the property</param>
        /// <param name="callback">Callback to populate the property values</param>
        public void AddLazy(string memberName, Func<dynamic> callback)
        {
            this[memberName] = callback;
        }

        /// <summary>
        /// Adds a lazy-loaded property
        /// </summary>
        /// <param name="memberName">Name of the property</param>
        /// <param name="callback">Callback to populate the property values, with a reference to this <see cref="BendyObject"/></param>
        public void AddLazy(string memberName, Func<dynamic, dynamic> callback)
        {
            this[memberName] = callback;
        }

        /// <summary>
        /// Creates a dynamic version of the object
        /// </summary>
        /// <returns></returns>
        public dynamic AsDynamic()
        {
            return this;
        }
    }
}

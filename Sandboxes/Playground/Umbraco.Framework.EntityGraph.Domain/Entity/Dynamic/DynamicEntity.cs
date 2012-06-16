using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Diagnostics.Contracts;
using Umbraco.Framework.EntityGraph.Domain.Versioning;
using Umbraco.Framework.Data.Common;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Dynamic
{
    /// <summary>
    /// A dynamic implementation of the <see cref="IEntity"/> object
    /// </summary>
    public class DynamicEntity : DynamicObject, IEntity
    {
        /// <summary>
        /// Information for a binding event
        /// </summary>
        internal class BinderInfo
        {
            public BinderInfo(bool ignoreCase, string name, Type returnType)
            {
                IgnoreCase = ignoreCase;
                Name = name;
                ReturnType = returnType;
            }

            public bool IgnoreCase { get; private set; }
            public string Name { get; private set; }
            public Type ReturnType { get; private set; }
        }

        private readonly IEntity _entity;

        internal DynamicEntity(IEntity entity)
        {
            Contract.Requires(entity != null);
            _entity = entity;
        }

        [ContractInvariantMethod]
        private void SpecificObjectInvariant()
        {
            Contract.Invariant(_entity != null);
        }

        #region IEntity Members

        /// <summary>
        /// Gets or sets the status of the entity.
        /// </summary>
        /// <value>The status.</value>
        public IEntityStatus Status
        {
            get
            {
                return _entity.Status;
            }
            set
            {
                _entity.Status = value;
            }
        }

        /// <summary>
        /// Gets or sets the creation date of the entity (UTC).
        /// </summary>
        /// <value>The UTC created.</value>
        public DateTime UtcCreated
        {
            get
            {
                return _entity.UtcCreated;
            }
            set
            {
                _entity.UtcCreated = value;
            }
        }

        /// <summary>
        /// Gets or sets the modification date of the entity (UTC).
        /// </summary>
        /// <value>The UTC modified.</value>
        public DateTime UtcModified
        {
            get
            {
                return _entity.UtcModified;
            }
            set
            {
                _entity.UtcModified = value;
            }
        }

        /// <summary>
        /// Gets or sets the timestamp when the status of this entity was last changed (in UTC).
        /// </summary>
        /// <value>The UTC status changed.</value>
        public DateTime UtcStatusChanged
        {
            get
            {
                return _entity.UtcStatusChanged;
            }
            set
            {
                _entity.UtcStatusChanged = value;
            }
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public IMappedIdentifier Id
        {
            get
            {
                return _entity.Id;
            }
            set
            {
                _entity.Id = value;
            }
        }

        /// <summary>
        /// Gets or sets the revision data.
        /// </summary>
        /// <value>The revision.</value>
        public IRevisionData Revision
        {
            get
            {
                return _entity.Revision;
            }
            set
            {
                _entity.Revision = value;
            }
        }

        #endregion

        #region ITracksConcurrency Members

        /// <summary>
        /// Gets the concurrency token.
        /// </summary>
        /// <value>The concurrency token.</value>
        public IConcurrencyToken ConcurrencyToken
        {
            get
            {
                return _entity.ConcurrencyToken;
            }
            set
            {
                _entity.ConcurrencyToken = value;
            }
        }

        #endregion

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, the <paramref name="value"/> is "Test".</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            //TODO: Work out what show be required on the dynamic IEntity
            //Some thoughts: drop UTC from Date
            return base.TrySetMember(binder, value);
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            //TODO: Work out what show be required on the dynamic IEntity
            //Some thoughts: drop UTC from Date
            return base.TryGetMember(binder, out result);
        }
    }
}

using System;

namespace Umbraco.Cms.Web.DependencyManagement
{
    /// <summary>
    /// Specifies that the target class is capable of binding a particular model
    /// </summary>
    public class ModelBinderForAttribute : Attribute
    {
        private readonly Type _targetType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBinderForAttribute"/> class.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        public ModelBinderForAttribute(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            _targetType = targetType;
        }

        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>The type of the target.</value>
        public Type TargetType
        {
            get { return _targetType; }
        }
    }
}
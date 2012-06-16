using System;
using System.Linq;

namespace Umbraco.Framework.Security
{
    public abstract class Permission
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Permission"/> class.
        /// </summary>
        protected Permission()
        {
            //Locate the metadata attribute
            var permissionAttributes = GetType()
                .GetCustomAttributes(typeof(PermissionAttribute), true)
                .OfType<PermissionAttribute>();

            if (!permissionAttributes.Any())
                throw new InvalidOperationException(
                    string.Format("The Permission of type {0} is missing the {1} attribute", GetType().FullName,
                                  typeof(PermissionAttribute).FullName));

            //assign the permissionAttributes of this object to those of the metadata attribute
            var attr = permissionAttributes.First();
            Id = attr.Id;
            Name = attr.Name;
            Type = attr.Type;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public virtual Guid Id { get; protected set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public virtual string Type { get; protected set; }
    }
}

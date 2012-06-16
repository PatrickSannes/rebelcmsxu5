using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
namespace Umbraco.Framework.Persistence.Model
{
    public abstract class AbstractSchemaPart : AbstractEntity
    {

    }

    [DebuggerDisplay("{DebugTypeName} with Id: {Id} and Alias: {Alias}")]
    public abstract class AbstractSchemaPart<T> : AbstractSchemaPart, IReferenceByName
        where T : AbstractSchemaPart<T>
    {
        private string _alias;
        private LocalizedString _name;
        private static readonly PropertyInfo _aliasSelector = ExpressionHelper.GetPropertyInfo<AbstractSchemaPart<T>, string>(x => x.Alias);
        private static readonly PropertyInfo _nameSelector = ExpressionHelper.GetPropertyInfo<AbstractSchemaPart<T>, string>(x => x.Name);

        /// <summary>
        /// Gets or sets the alias of the object. The alias is a string to which this object
        /// can be referred programmatically, and is often a normalised version of the <see cref="IReferenceByName.Name"/> property.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias
        {
            get { return _alias; }
            set
            {
                _alias = value;
                OnPropertyChanged(_aliasSelector);
            }
        }

        /// <summary>
        /// Gets or sets the name of the object. The name is a string intended to be human-readable, and
        /// is often a more descriptive version of the <see cref="IReferenceByName.Alias"/> property.
        /// </summary>
        /// <value>The name.</value>
        public LocalizedString Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(_nameSelector);
            }
        }
    }
}
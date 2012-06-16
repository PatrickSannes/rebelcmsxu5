namespace Umbraco.Framework.Persistence.Model.Versioning
{
    public class Branch : AbstractEntity
    {
        public Branch()
            : this("default")
        {
        }

        #region Implementation of IBranch

        public Branch(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name { get; set; }

        #endregion
    }
}
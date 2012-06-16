namespace Umbraco.Framework.Persistence.Model.Versioning
{
    public class Changeset : AbstractEntity
    {
        public Changeset()
            : this(new Branch())
        {
        }

        #region Implementation of IChangeset

        public Changeset(Branch branch)
        {
            Branch = branch;
        }

        /// <summary>
        /// Gets or sets the branch in which this changeset resides.
        /// </summary>
        /// <value>The branch.</value>
        public virtual Branch Branch { get; set; }

        #endregion
    }
}
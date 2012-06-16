namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// A model representing copying a node
    /// </summary>
    public class CopyModel : MoveModel
    {

        public bool RelateToOriginal { get; set; }

    }
}
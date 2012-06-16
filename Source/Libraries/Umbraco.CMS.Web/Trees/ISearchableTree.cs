namespace Umbraco.Cms.Web.Trees
{
    public interface ISearchableTree
    {

        TreeSearchJsonResult Search(string searchText);

    }
}
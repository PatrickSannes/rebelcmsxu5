using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Cms.Web
{

    /// <summary>
    /// <![CDATA[
    /// Extension methods for SelectList and IEnumerable<SelectListItem>
    /// ]]>
    /// </summary>
    public static class SelectListExtensions
    {

        /// <summary>
        /// Un-selects all items in list
        /// </summary>
        /// <param name="selectList"></param>
        public static void UnSelectItems(this IEnumerable<SelectListItem> selectList)
        {
            foreach(var i in selectList)
            {
                i.Selected = false;
            }
        }

        /// <summary>
        /// Select all items that have values contained in selectedVals
        /// </summary>
        /// <param name="selectList"></param>
        /// <param name="selectedVals"></param>
        public static void SelectItems(this IEnumerable<SelectListItem> selectList, IEnumerable<string> selectedVals)
        {
            foreach (var i in selectList)
            {
                i.Selected = selectedVals.Contains(i.Value);
            }
        }

        public static void SelectItems(this IEnumerable<SelectListItem> selectList, params string[] selectedVals)
        {
            foreach (var i in selectList)
            {
                i.Selected = selectedVals.Contains(i.Value);
            }
        }

    }
}

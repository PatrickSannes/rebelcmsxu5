using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web.Model.BackOffice.Trees;

namespace Umbraco.Cms.Web
{
    public static class MenuItemExtensions
    {
        /// <summary>
        /// Returns a menu item and it's meta data based on a menu item type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="menuItems"></param>
        /// <returns></returns>
        public static Lazy<MenuItem, MenuItemMetadata> GetItem<T>(this IEnumerable<Lazy<MenuItem, MenuItemMetadata>> menuItems)
            where T: MenuItem
        {
            return menuItems.Where(x => x.Metadata.ComponentType == typeof (T)).FirstOrDefault();
        }

    }
}

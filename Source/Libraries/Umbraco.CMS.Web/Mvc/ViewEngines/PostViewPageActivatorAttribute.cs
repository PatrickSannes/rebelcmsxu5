using System;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    /// <summary>
    /// Allows ordering of post view page activators, if the attribute is not declared then all post view page activators
    /// without it will be executed as how IoC loads them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PostViewPageActivatorAttribute : Attribute
    {
        public int Order { get; private set; }

        public PostViewPageActivatorAttribute(int order)
        {
            Order = order;
        }
    }
}
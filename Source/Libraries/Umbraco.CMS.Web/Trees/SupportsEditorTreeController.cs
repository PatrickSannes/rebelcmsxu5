using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Trees
{
    /// <summary>
    /// An abstract tree that supports menu actions with an editor
    /// </summary>
    public abstract class SupportsEditorTreeController : TreeController
    {
        protected SupportsEditorTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// The ID of the editor controller
        /// </summary>
        public abstract Guid EditorControllerId { get; }       

    }

}
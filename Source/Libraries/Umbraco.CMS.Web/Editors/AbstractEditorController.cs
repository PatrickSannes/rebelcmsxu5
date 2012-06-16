using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Editors
{
    /// <summary>
    /// Represents the base class for all editor controllers
    /// </summary>
    public abstract class AbstractEditorController  : SecuredBackOfficeController, IGeneratePath
    {
        protected AbstractEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            //Locate the editor attribute
            var editorAttributes = GetType()
                .GetCustomAttributes(typeof(EditorAttribute), false)
                .OfType<EditorAttribute>();

            if (!editorAttributes.Any())
            {
                throw new InvalidOperationException("The Editor controller is missing the " + typeof(EditorAttribute).FullName + " attribute");
            }

            //assign the properties of this object to those of the metadata attribute
            var attr = editorAttributes.First();
            EditorId = attr.Id;           
        }

        /// <summary>
        /// Returns the editor id assigned to the controller via attributes
        /// </summary>
        public Guid EditorId { get; private set; }

        private EntityPathCollection _currentPaths;
         
        /// <summary>
        /// Returns an Id path for the current entity being edited by the controller instance
        /// </summary>
        /// <returns></returns>
        public virtual EntityPathCollection GetPaths()
        {
            return _currentPaths;
        }

        /// <summary>
        /// Sets the Id path for the current entity being edited by the controller instance
        /// </summary>
        /// <param name="path"></param>
        public void GeneratePathsForCurrentEntity(EntityPathCollection path)
        {
            _currentPaths = path;
        }
    }
}

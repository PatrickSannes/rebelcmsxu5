using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ViewEngines;
using Umbraco.Cms.Web.Templates;
using Umbraco.Framework;

using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.TemplateEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    [AlternateViewEnginePath("ScriptEditor")]
    public class TemplateEditorController : AbstractFileEditorController
    {
        private readonly GroupUnitFactory<IFileStore> _hive;

        public TemplateEditorController(IBackOfficeRequestContext requestContext) : base(requestContext)
        {
            _hive = BackOfficeRequestContext
                .Application
                .Hive
                .GetWriter<IFileStore>(new Uri("storage://templates"));
        }

        public override GroupUnitFactory<IFileStore> Hive
        {
            get { return _hive; }
        }

        protected override string SaveSuccessfulTitle
        {
            get { return "Template.Save.Title".Localize(this); }
        }

        protected override string SaveSuccessfulMessage
        {
            get { return "Template.Save.Message".Localize(this); }
        }

        protected override string CreateNewTitle
        {
            get { return "Create a template"; }
        }

        protected override string[] AllowedFileExtensions
        {
            get { return new[] { ".cshtml" }; }
        }

        //public override ActionResult Create(string fileName, HiveId parentId, HiveId stubFileId = default(HiveId))
        //{
        //    var model = CreateNewEditorModel(fileName, parentId, stubFileId);

        //    return View("Edit", model);
        //}

        protected override EntityPathCollection CreatePaths(File file)
        {
            //return base.CreatePath(file);

            if (file.IsContainer)
                return base.CreatePaths(file);

            var parser = new TemplateParser();
            var result = parser.Parse(file);

            if (string.IsNullOrWhiteSpace(result.Layout))
                return base.CreatePaths(file);

            var path = new List<HiveId>
            {
                file.Id
            };

            using (var uow = Hive.Create())
            {
                // Get parent paths from layout
                //TODO: Need to fetch from the current folder if we want to support folders
                var files = uow.Repositories.GetAll<File>().Where(x => !x.IsContainer && x.Id != file.Id);

                var currentLayout = TemplateHelper.ResolveLayoutPath(result.Layout, file);
                while(!string.IsNullOrWhiteSpace(currentLayout))
                {
                    var parent = files.SingleOrDefault(x => TemplateHelper.ResolveLayoutPath(x.RootRelativePath, x) == currentLayout);
                    if(parent != null)
                    {
                        path.Add(parent.Id);
                        currentLayout = parser.Parse(parent).Layout;
                    }
                    else
                    {
                        currentLayout = "";
                    }
                }

                path.Reverse();

                // Insert the actual parents path at the begining
                var actualParent = uow.Repositories.GetLazyParentRelations(file.Id, FixedRelationTypes.DefaultRelationType)
                    .Select(x => x.Source as File)
                    .SingleOrDefault();

                if (actualParent != null)
                    path.InsertRange(0, base.CreatePaths(actualParent)[0]); // Assumes a template will only ever be available at one location
            }

            return new EntityPathCollection(file.Id, new[]{ new EntityPath(path) });
        }

        protected override void EnsureViewData(FileEditorModel model, File file)
        {
            // Setup UIElements
            model.UIElements.Add(new SeperatorUIElement());
            model.UIElements.Add(new ButtonUIElement
            {
                Alias = "InsertField",
                Title = "Insert an umbraco page field",
                CssClass = "insert-field-button toolbar-button",
                AdditionalData = new Dictionary<string, string>
                {
                    { "id", "submit_InsertField" },
                    { "name", "submit.InsertField" }
                }
            });
            model.UIElements.Add(new ButtonUIElement
            {
                Alias = "InsertPartial",
                Title = "Insert a partial view",
                CssClass = "insert-partial-button toolbar-button",
                AdditionalData = new Dictionary<string, string>
                {
                    { "id", "submit_InsertPartial" },
                    { "name", "submit.InsertPartial" }
                }
            });
            model.UIElements.Add(new ButtonUIElement
            {
                Alias = "InsertMacro",
                Title = "Insert a macro",
                CssClass = "insert-macro-button toolbar-button",
                AdditionalData = new Dictionary<string, string>
                {
                    { "id", "submit_InsertMacro" },
                    { "name", "submit.InsertMacro" }
                }
            });
            model.UIElements.Add(new SeperatorUIElement());
            model.UIElements.Add(new ButtonUIElement
            {
                Alias = "DefineSection",
                Title = "Define a Section",
                CssClass = "define-section-button toolbar-button",
                AdditionalData = new Dictionary<string, string>
                {
                    { "id", "submit_DefineSection" },
                    { "name", "submit.DefineSection" }
                }
            });
            model.UIElements.Add(new ButtonUIElement
            {
                Alias = "ImplementSection",
                Title = "Implement a Section",
                CssClass = "implement-section-button toolbar-button",
                AdditionalData = new Dictionary<string, string>
                {
                    { "id", "submit_ImplementSection" },
                    { "name", "submit.ImplementSection" }
                }
            });
            model.UIElements.Add(new SeperatorUIElement());
            model.UIElements.Add(new SelectListUIElement
            {
                Alias = "Layout",
                Title = "Layout",
                CssClass = "layout-select-list",
                AdditionalData = new Dictionary<string, string>
                {
                    { "id", "select_Layout" },
                    { "name", "select.Layout" }
                }
            });

            // Setup data
            var parser = new TemplateParser();
            ViewBag.CurrentLayout = file != null ? parser.Parse(file).Layout : "";
            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
            {
                //create the allowed templates check box list
                ViewBag.AvailableTemplates = new List<SelectListItem>(
                    uow.Repositories.GetAllNonContainerFiles()
                    .OrderBy(x => x.Name)
                    .Select(x =>
                        new SelectListItem
                        {
                            Text = x.GetFileNameForDisplay(),
                            Value = x.Name
                        })).ToArray();
            }
        }

        protected override void PopulateFileContentFromStub(FileEditorModel model, HiveId stubFileId, IDictionary<string, string> replacements)
        {
            var parentId = model.ParentId;

            model.ParentId = FixedHiveIds.SystemRoot;

            replacements = new Dictionary<string, string> { { "$layout$", "" }, { "$sections$", "" } };

            if (!stubFileId.IsNullValueOrEmpty())
            {
                if (!parentId.IsNullValueOrEmpty() && parentId != FixedHiveIds.SystemRoot)
                {
                    using (var uow = Hive.Create())
                    {
                        var parentFile = uow.Repositories.Get<File>(parentId);
                        if (parentFile == null)
                            throw new ArgumentException("No file could be found for the parent id specified");

                        replacements["$layout$"] = parentFile.Name;
                        replacements["$sections$"] = new TemplateParser().Parse(parentFile).Sections.Where(x => x != "Body")
                            .Aggregate("", (current, section) => current + ("\n@section " + section + "\n{\n\n}\n"));
                    }
                }
            }
            
            base.PopulateFileContentFromStub(model, stubFileId, replacements);
        }

        public FileEditorModel CreateNewEditorModel(string fileName, HiveId parentId, HiveId stubFileId = default(HiveId))
        {
            var model = FileEditorModel.CreateNew();
            model.Name = fileName;
            model.ParentId = parentId;

            PopulateFileContentFromStub(model, stubFileId, null);

            EnsureViewData(model, null);

            return model;
        }

        protected override void OnBeforeCreate(CreateFileModel createModel, FileEditorModel editorModel)
        {
            createModel.ParentId = editorModel.ParentId = FixedHiveIds.SystemRoot;
        }

        //protected override void OnAfterSave(File oldFile, File file)
        //{
        //    if (oldFile != null)
        //    {
        //        // Update all templates using this as layout
        //        using (var uow = Hive.Create())
        //        {
        //            var repo = uow.Repositories;
        //            var files = repo.GetAll<File>().Where(x => !x.IsContainer);

        //            // Create map of files based upon Layout properties
        //            var fileMap = TemplateHelper.CreateLayoutFileMap(files);

        //            // Choose the tree depth to render
        //            var currentKey = Server.MapPath(oldFile.RootedPath);

        //            foreach (var childLayout in fileMap[currentKey])
        //            {
        //                var content = Encoding.UTF8.GetString(childLayout.ContentBytes);
        //                content = content.Replace(oldFile.Name, file.Name);
        //                childLayout.ContentBytes = Encoding.UTF8.GetBytes(content);
        //                uow.Repositories.AddOrUpdate(childLayout);
        //            }

        //            uow.Complete();
        //        }
        //    }
        //}
    }
}

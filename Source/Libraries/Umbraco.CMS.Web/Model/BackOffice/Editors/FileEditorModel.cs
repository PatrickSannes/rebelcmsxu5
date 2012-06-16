using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Represents a model for a script editor
    /// </summary>
    public class FileEditorModel : EditorModel
    {
        public static FileEditorModel CreateNew()
        {
            return new FileEditorModel();
        }

        private FileEditorModel()
        {
            PopulateUIElements();
        }

        public FileEditorModel(HiveId id, string name, DateTimeOffset created, DateTimeOffset updated, Func<string> content)
        {
            Mandate.ParameterNotEmpty(id, "id");
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Mandate.ParameterNotNull(content, "content");
            

            Id = id;
            UtcCreated = created;
            UtcModified = updated;
            Name = name;

            FileContent = content();

            PopulateUIElements();
        }

        private string _fileContent;

        [DataType(global::System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string FileContent
        {
            get { return _fileContent ?? string.Empty; }
            set { _fileContent = value; }
        }

        protected void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());
        }
    }
}
﻿using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Tests.Extensions.Stubs.PropertyEditors
{
    public class TestEditorModel : EditorModel
    {
        [Required(AllowEmptyStrings = false)]
        public string Value { get; set; }

    }
}
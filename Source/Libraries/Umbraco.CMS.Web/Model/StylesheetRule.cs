using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model
{    
    public class StylesheetRule
    {
        public StylesheetRule()
        { }

        [HiddenInput(DisplayValue = false)]
        public HiveId StylesheetId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public HiveId RuleId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Selector { get; set; }

        public string Styles { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("/*" + Environment.NewLine);
            sb.AppendFormat("   Name: {0}" + Environment.NewLine, Name);
            sb.Append("*/" + Environment.NewLine);
            sb.AppendFormat("{0} {{" + Environment.NewLine, Selector);
            sb.Append(string.Join(Environment.NewLine, !string.IsNullOrWhiteSpace(Styles) ? string.Join(Environment.NewLine, Styles.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Select(x => "\t" + x)) + Environment.NewLine : ""));
            sb.Append("}");

            return sb.ToString();
        }
    }
}

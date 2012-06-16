using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.WebPages;
using ClientDependency.Core;
using ClientDependency.Core.Mvc;
using Microsoft.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework;
using ExpressionHelper = System.Web.Mvc.ExpressionHelper;

namespace Umbraco.Cms.Web
{

    public static class HtmlHelperEditorExtensions
    {
        /// <summary>
        /// Renders a form for the specified controller id
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="editorId"></param>
        /// <param name="routeVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginForm(this HtmlHelper html, string action, Guid editorId, object routeVals = null, object htmlAttributes = null)
        {
            return html.BeginForm(action, editorId, routeVals, htmlAttributes.ToDictionary<object>());
        }

        /// <summary>
        /// Renders a form for the specified controller id
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="editorId"></param>
        /// <param name="routeVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginForm(this HtmlHelper html, string action, Guid editorId, object routeVals, IDictionary<string, object> htmlAttributes)
        {
            var components = DependencyResolver.Current.GetService<ComponentRegistrations>();
            var settings = DependencyResolver.Current.GetService<UmbracoSettings>();
            var editorMetaData = components
                .EditorControllers
                .Where(x => x.Metadata.Id == editorId)
                .SingleOrDefault();

            if (editorMetaData == null)
                throw new InvalidOperationException("Could not find the editor controller with id " + editorId);

            var routeValDictionary = new RouteValueDictionary(routeVals);
            routeValDictionary["editorId"] = editorId.ToString("N");

            var area = settings.UmbracoPaths.BackOfficePath;

            //now, need to figure out what area this editor belongs too...
            var pluginDefinition = editorMetaData.Metadata.PluginDefinition;
            if (pluginDefinition.HasRoutablePackageArea())
            {
                area = pluginDefinition.PackageName;
            }

            //add the plugin area to our collection
            routeValDictionary["area"] = area;

            return html.BeginForm(action, editorMetaData.Metadata.ControllerName, routeValDictionary, FormMethod.Post, htmlAttributes);
        }

        public static IHtmlString UmbNextButton(this HtmlHelper html, object htmlAttributes = null)
        {
            var builder = new TagBuilder("button");
            builder.Attributes.Add("id", "submit_Save");
            builder.Attributes.Add("name", "submit.Save");
            builder.Attributes.Add("value", "submit.Save");
            builder.Attributes.Add("title", "Next");
            builder.Attributes.Add("type", "submit");
            builder.AddCssClass("next-button");

            if (htmlAttributes != null)
            {
                var d = htmlAttributes.ToDictionary();
                builder.MergeAttributes(d);    
            }

            builder.SetInnerText("next >");

            return new HtmlString(builder.ToString());
        }

        /// <summary>
        /// Renders a Tree Picker.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="treePickerRenderModel">The tree picker render model.</param>
        /// <returns></returns>
        public static IHtmlString TreePicker(this HtmlHelper html, TreePickerRenderModel treePickerRenderModel)
        {
            return html.Action("Index", "TreePicker", new { area = "umbraco", model = treePickerRenderModel });
        }

        /// <summary>
        /// Renders a Tree Picker for the given expression.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="treePickerRenderModel">The tree picker render model.</param>
        /// <returns></returns>
        public static IHtmlString TreePickerFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, TreePickerRenderModel treePickerRenderModel)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            treePickerRenderModel.Id = html.IdFor(expression);
            treePickerRenderModel.Name = html.NameFor(expression);
            treePickerRenderModel.SelectedValue = (HiveId)modelMetadata.Model;

            return html.Action("Index", "TreePicker", new { area = "umbraco", model = treePickerRenderModel });
        }

        /// <summary>
        /// A call to this is required when we need unobtrusive validation in partial views when the Form element isn't being generated
        /// on this particular view.
        /// See: http://stackoverflow.com/questions/6401033/editorfor-not-rendering-data-validation-attributes-without-beginform
        /// </summary>
        /// <param name="helper"></param>
        public static void EnablePartialViewValidation(this HtmlHelper helper)
        {
            if (helper.ViewContext.FormContext == null)
            {
                helper.ViewContext.FormContext = new FormContext();
            }
        }

        /// <summary>
        /// Renders a dashboard item
        /// </summary>
        /// <param name="html"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static IHtmlString RenderDashboard(this HtmlHelper html, DashboardItemModel model)
        {
            return html.Action("RenderDashboard", "DashboardEditor", new { model });
        }

        /// <summary>
        /// Ensures that the output of the IHtmlString is HtmlDecoded
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        /// <remarks>
        /// Though the Raw() method normally works, this method was created because the Raw() method seems to ignore encoded attributes
        /// </remarks>
        public static IHtmlString EnsureRaw(this IHtmlString output)
        {
            return new MvcHtmlString(HttpUtility.HtmlDecode(output.ToString()));
        }

        public static IHtmlString UmbLabelFor<TModel, TValue>(
            this HtmlHelper<TModel> html, 
            Expression<Func<TModel, TValue>> expression)
        {
            return UmbLabelFor(html, expression, (object)null);
        }

        public static IHtmlString UmbLabelFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            object htmlAttributes)
        {
            return UmbLabelFor(html, expression, null);
        }

        public static IHtmlString UmbLabelFor<TModel, TValue>(
            this HtmlHelper<TModel> html, 
            Expression<Func<TModel, TValue>> expression, 
            string labelText)
        {
            return UmbLabelFor(html, expression, labelText, (object)null);
        }

        public static IHtmlString UmbLabelFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            string labelText,
            object htmlAttributes)
        {
            return UmbLabelFor(html, expression, labelText, null, (object)null);
        }

        public static IHtmlString UmbLabelFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            string labelText,
            string descriptionText)
        {
            return UmbLabelFor(html, expression, labelText, descriptionText, (object)null);
        }

        public static IHtmlString UmbLabelFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            string labelText,
            string descriptionText,
            object htmlAttributes)
        {
            return LabelHelper(
                html,
                ModelMetadata.FromLambdaExpression(expression, html.ViewData),
                global::System.Web.Mvc.ExpressionHelper.GetExpressionText(expression),
                labelText,
                descriptionText,
                htmlAttributes
                );
        }

        private static IHtmlString LabelHelper(
            HtmlHelper html,
            ModelMetadata metadata,
            string htmlFieldName,
            string labelText,
            string descriptionText,
            object htmlAttributes
        )
        {
            var resolvedLabelText = labelText ?? metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (string.IsNullOrEmpty(resolvedLabelText))
            {
                return MvcHtmlString.Empty;
            }

            var labelTag = new TagBuilder("label");
            labelTag.Attributes.Add("for", TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
            labelTag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            labelTag.SetInnerText(resolvedLabelText);

            var resolvedDescriptionText = descriptionText ?? metadata.Description;

            if (!string.IsNullOrEmpty(resolvedDescriptionText))
            {
                var descriptionTag = new TagBuilder("small");
                descriptionTag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
                descriptionTag.SetInnerText(resolvedDescriptionText);

                return MvcHtmlString.Create(labelTag.ToString(TagRenderMode.Normal) + descriptionTag.ToString(TagRenderMode.Normal));
            }

            return MvcHtmlString.Create(labelTag.ToString(TagRenderMode.Normal));
        }


        #region Table

        /// <summary>
        /// Creates an Html table based on the collection 
        /// which has a maximum numer of rows (expands horizontally)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <param name="collection"></param>
        /// <param name="maxRows"></param>
        /// <param name="template"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static HelperResult TableHorizontal<T>(this HtmlHelper html,
            IEnumerable<T> collection,
            int maxRows,
            Func<T, HelperResult> template,
            object htmlAttributes = null) where T : class
        {
            return new HelperResult(writer =>
            {
                var items = collection.ToArray();
                var itemCount = items.Count();
                var maxCols = Convert.ToInt32(Math.Ceiling(items.Count() / Convert.ToDecimal(maxRows)));
                //construct a grid first
                var grid = new T[maxCols, maxRows];
                var current = 0;
                for (var x = 0; x < maxCols; x++)
                    for (var y = 0; y < maxRows; y++)
                        if (current < itemCount)
                            grid[x, y] = items[current++];
                WriteTable(grid, writer, maxRows, maxCols, template, htmlAttributes);
            });
        }

        /// <summary>
        /// Creates an Html table based on the collection 
        /// which has a maximum number of cols (expands vertically)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <param name="collection"></param>
        /// <param name="maxCols"></param>
        /// <param name="template"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static HelperResult TableVertical<T>(this HtmlHelper html,
            IEnumerable<T> collection,
            int maxCols,
            Func<T, HelperResult> template,
            object htmlAttributes = null) where T : class
        {
            return new HelperResult(writer =>
            {
                var items = collection.ToArray();
                var itemCount = items.Count();
                var maxRows = Convert.ToInt32(Math.Ceiling(items.Count() / Convert.ToDecimal(maxCols)));
                //construct a grid first
                var grid = new T[maxCols, maxRows];
                var current = 0;
                for (var y = 0; y < maxRows; y++)
                    for (var x = 0; x < maxCols; x++)
                        if (current < itemCount)
                            grid[x, y] = items[current++];
                WriteTable(grid, writer, maxRows, maxCols, template, htmlAttributes);
            });
        }

        /// <summary>
        /// Writes the table markup to the writer based on the item
        /// input and the pre-determined grid of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="grid"></param>
        /// <param name="writer"></param>
        /// <param name="maxRows"></param>
        /// <param name="maxCols"></param>
        /// <param name="template"></param>
        /// <param name="htmlAttributes"></param>
        private static void WriteTable<T>(T[,] grid,
            TextWriter writer,
            int maxRows,
            int maxCols,
            Func<T, HelperResult> template,
            object htmlAttributes = null) where T : class
        {

            var attributes = htmlAttributes.ToDictionary<string>();


            //create a table based on the grid            
            writer.Write("<table ");
            foreach (var a in attributes)
            {
                writer.Write(a.Key + "=\"" + a.Value + "\" ");
            }
            writer.Write(">");

            for (var y = 0; y < maxRows; y++)
            {

                writer.Write("<tr>");
                for (var x = 0; x < maxCols; x++)
                {
                    writer.Write("<td>");
                    var item = grid[x, y];
                    if (item != null)
                    {
                        //if there's an item at that grid location, call its template
                        template(item).WriteTo(writer);
                    }
                    writer.Write("</td>");
                }
                writer.Write("</tr>");
            }
            writer.Write("</table>");


        }
        #endregion

        /// <summary>
        /// The Umbraco validation summary
        /// </summary>
        /// <param name="html"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static IHtmlString UmbValidationSummary(this HtmlHelper html, string msg)
        {
            //ensure we register the validation module with CD:
            html.ViewContext.GetLoader()
                .RegisterDependency("Umbraco.System/Validation.js", "Scripts", ClientDependencyType.Javascript);

            var isValidClass = html.ViewData.ModelState.IsValid ? "valid" : "invalid";
            var sb = new StringBuilder();
            sb.AppendLine("<div class='validation-summary " + isValidClass + "'>");
            sb.AppendLine("<div class='collapse-button toggle-button'></div>");
            sb.Append(html.ValidationSummary(false, msg, new { @class = "error" }));
            sb.AppendLine("</div>");
            return new HtmlString(sb.ToString());
        }

        #region Join
        /// <summary>
        /// This joins together any IHtmlString
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static IHtmlString Join(this HtmlHelper helper, params IHtmlString[] arguments)
        {
            var builder = new StringBuilder();
            foreach (var a in arguments)
            {
                builder.Append(a);
            }
            return new HtmlString(builder.ToString());
        }

        /// <summary>
        /// This joins together any Razor template delegates
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static IHtmlString Join(this HtmlHelper helper, params Func<object, HelperResult>[] arguments)
        {
            var builder = new StringBuilder();
            foreach (var a in arguments)
            {
                builder.Append(a(null));
            }
            return new HtmlString(builder.ToString());
        }
        #endregion

        #region Collapse Panel

        /// <summary>
        /// Creates an collapsable panel and renders a custom editor
        /// </summary>
        /// <param name="html"></param>
        /// <param name="collapsedTitle"></param>
        /// <param name="expandedTitle"></param>
        /// <param name="htmlId">The id to add for the div tag</param>
        /// <param name="showDeleteButton"></param>
        /// <param name="editor"></param>
        /// <returns></returns>
        public static IHtmlString UmbCollapsePanel(this HtmlHelper html, string collapsedTitle, string expandedTitle, string htmlId, bool showDeleteButton, IHtmlString editor)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div " + (string.IsNullOrEmpty(htmlId) ? "" : "id='" + htmlId + "'") + " class='collapse-panel' data-expanded='" + html.Encode(expandedTitle) + "' data-collapsed='" + html.Encode(collapsedTitle) + "'>");
            sb.Append("<a><span>");
            sb.Append(collapsedTitle);
            sb.AppendLine("</span></a>");
            if (showDeleteButton)
            {
                sb.AppendLine("<button name=\"submit.DeletePanel\" value=\"" + htmlId + "\" title=\"Delete\" class=\"delete-button\"></button>");
            }
            sb.AppendLine("<div class='toggle-button expand-button'></div>");
            sb.AppendLine("<div class='box clearfix'>");

            sb.Append(editor);

            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// Creates an collapsable panel and renders a custom editor
        /// </summary>
        /// <param name="html"></param>
        /// <param name="collapsedTitle"></param>
        /// <param name="expandedTitle"></param>
        /// <param name="htmlId"></param>
        /// <param name="showDeleteButton"></param>
        /// <param name="editor">any @Razor-Syntax</param>
        /// <returns></returns>
        public static IHtmlString UmbCollapsePanel(this HtmlHelper html, string collapsedTitle, string expandedTitle, string htmlId, bool showDeleteButton, Func<object, HelperResult> editor)
        {
            return html.UmbCollapsePanel(collapsedTitle, expandedTitle, htmlId, showDeleteButton, editor(null));
        }

        /// <summary>
        /// Creates an collapsable panel and renders a custom editor
        /// </summary>
        /// <param name="html"></param>
        /// <param name="expandedTitle"></param>
        /// <param name="showDeleteButton"></param>
        /// <param name="editor">
        /// Any IHtmlString
        /// </param>
        /// <param name="collapsedTitle"></param>
        /// <returns></returns>
        public static IHtmlString UmbCollapsePanel(this HtmlHelper html, string collapsedTitle, string expandedTitle, bool showDeleteButton, IHtmlString editor)
        {
            return html.UmbCollapsePanel(collapsedTitle, expandedTitle, string.Empty, showDeleteButton, editor);
        }

        /// <summary>
        /// Creates an collapsable panel and renders a custom editor
        /// </summary>
        /// <param name="html"></param>
        /// <param name="collapsedTitle"></param>
        /// <param name="expandedTitle"></param>
        /// <param name="showDeleteButton"></param>
        /// <param name="editor"></param>
        /// <returns></returns>
        public static IHtmlString UmbCollapsePanel(this HtmlHelper html, string collapsedTitle, string expandedTitle, bool showDeleteButton, Func<object, HelperResult> editor)
        {
            return html.UmbCollapsePanel(collapsedTitle, expandedTitle, string.Empty, showDeleteButton, editor);
        }

        #endregion

        #region Umbraco Property Editor

        /// <summary>
        /// Umbs the display id.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbDisplayId<TModel>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, HiveId>> expression, 
            string description = "",
            string tooltip = "")
        {
            var meta = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            return UmbEditorMarkup(html.UmbLabelFor(expression), 
                new HtmlString(meta.Model != null ? "<span class='hive-id'>" + ((HiveId)meta.Model).ToString(HiveIdFormatStyle.AsUri) + "</span>" : string.Empty), // Used to be HiveId.ToFriendlyString()
                null, 
                description,
                tooltip);
        }

        /// <summary>
        /// Creates a display for the field passed in and ensures it is styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbDisplayFor<TModel, TValue>(
             this HtmlHelper<TModel> html,
             Expression<Func<TModel, TValue>> expression,
            string description = "",
            string tooltip = "")
        {
            return UmbEditorMarkup(html.UmbLabelFor(expression), 
                html.DisplayFor(expression), 
                null, 
                description,
                tooltip);
        }

        #region UmbEditor

        /// <summary>
        /// Creates a label and custom editor for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="label">String expression for a label</param>
        /// <param name="editor">The editor.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditor(
            this HtmlHelper html,
            string label,
            IHtmlString editor,
            string description = "",
            string tooltip = "")
        {
            return UmbEditorMarkup(html.Label(label), 
                editor, 
                null, 
                description,
                tooltip);
        }

        /// <summary>
        /// Creates a label, validation message and custom editor for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="label">The label.</param>
        /// <param name="validationMessage">The validation message.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditor(
            this HtmlHelper html,
            string label,
            IHtmlString validationMessage,
            IHtmlString editor,
            string description = "",
            string tooltip = "")
        {
            return UmbEditorMarkup(html.Label(label), 
                editor, 
                validationMessage,
                description,
                tooltip);
        }

        /// <summary>
        /// Creates a label, validation message and custom editor for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditor(
           this HtmlHelper html,
           string expression,
            string description = "",
            string tooltip = "")
        {
            return UmbEditorMarkup(html.Label(expression), 
                html.Editor(expression), 
                html.ValidationMessage(expression), 
                description,
                tooltip);
        }

        /// <summary>
        /// Creates a label, validation message and custom editor for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="viewPath">The view path.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditor(
           this HtmlHelper html,
           string expression,
           string viewPath,
           string prefix,
            string description = "",
            string tooltip = "")
        {
            return UmbEditorMarkup(html.Label(expression), 
                html.Editor(expression, viewPath, prefix), 
                html.ValidationMessage(expression), 
                description,
                tooltip);
        }



        #endregion

        #region UmbEditorFor

        /// <summary>
        /// Creates a label and custom editor for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="labelFor">The label for.</param>
        /// <param name="editor">Any IHtmlString</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditorFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> labelFor,
            IHtmlString editor,
            string description = "",
            string tooltip = "")
        {
            return UmbEditorMarkup(html.UmbLabelFor(labelFor), 
                editor, 
                null, 
                description,
                tooltip);
        }

        /// <summary>
        /// Creates a label, validation message and custom editor for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="labelFor">The label for.</param>
        /// <param name="validationFor">The validation for.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditorFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> labelFor,
            Expression<Func<TModel, TValue>> validationFor,
            IHtmlString editor,
            string description = "",
            string tooltip = "")
        {
            return UmbEditorMarkup(html.UmbLabelFor(labelFor), 
                editor, 
                html.ValidationMessageFor(validationFor), 
                description,
                tooltip);
        }

        /// <summary>
        /// Creates a label and custom editor for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="labelFor">The label for.</param>
        /// <param name="editor">Any number of razor template delegates</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditorFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> labelFor,
            Func<object, HelperResult> editor,
            string description = "",
            string tooltip = "")
        {
            return html.UmbEditorFor(labelFor, editor(null), description, tooltip);
        }

        /// <summary>
        /// Creates a label, validation message and custom editor for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="labelFor">The label for.</param>
        /// <param name="validationFor">The validation for.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditorFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> labelFor,
            Expression<Func<TModel, TValue>> validationFor,
            Func<object, HelperResult> editor,
            string description = "",
            string tooltip = "")
        {
            return html.UmbEditorFor(labelFor, validationFor, editor(null), description, tooltip);
        }

        /// <summary>
        /// Creates a label and field for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="labelFor">the label to use for the property</param>
        /// <param name="editorFor">the editor to use for the property</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditorFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> labelFor,
            Expression<Func<TModel, TValue>> editorFor,
            string description = "",
            string tooltip = "")
        {
            return html.UmbEditorFor(labelFor, html.EditorFor(editorFor), description, tooltip);
        }

        /// <summary>
        /// Creates a label, validation message and field for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="labelFor">The label for.</param>
        /// <param name="validationFor">The validation for.</param>
        /// <param name="editorFor">The editor for.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditorFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> labelFor,
            Expression<Func<TModel, TValue>> validationFor,
            Expression<Func<TModel, TValue>> editorFor,
            string description = "",
            string tooltip = "")
        {
            return html.UmbEditorFor(labelFor, validationFor, html.EditorFor(editorFor), description, tooltip);
        }

        /// <summary>
        /// Creates a label and field for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="labelFor">the label to use for the property</param>
        /// <param name="editorFor">the editor to use for the property</param>
        /// <param name="viewPath">The view path.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <param name="showUmbracoLabel">Whether or not the editor will render the standard Umbraco field label or this editor will occupy all of the editor space</param>
        /// <param name="viewData">additional view data, by default this is null</param>
        /// <returns></returns>
        public static IHtmlString UmbEditorFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> labelFor,
            Expression<Func<TModel, TValue>> editorFor,
            string viewPath,
            string prefix,
            string description = "",
            string tooltip = "",
            bool showUmbracoLabel = true,
            object viewData = null)
        {
            if (!showUmbracoLabel)
            {
                return UmbEditorMarkup(html.EditorFor(editorFor, viewPath, prefix, viewData), null);
            }

            return html.UmbEditorFor(labelFor, 
                html.EditorFor(editorFor, viewPath, prefix, viewData),
                description,
                tooltip);
        }

        /// <summary>
        /// Creates a label, validation message and field for an Umbraco property and ensures it is styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="labelFor">The label for.</param>
        /// <param name="validationFor">The validation for.</param>
        /// <param name="editorFor">The editor for.</param>
        /// <param name="viewPath">The view path.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <param name="showUmbracoLabel">if set to <c>true</c> [show umbraco label].</param>
        /// <param name="viewData">The view data.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditorFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> labelFor,
            Expression<Func<TModel, TValue>> validationFor,
            Expression<Func<TModel, TValue>> editorFor,
            string viewPath,
            string prefix,
            string description = "",
            string tooltip = "",
            bool showUmbracoLabel = true,
            object viewData = null)
        {
            if (!showUmbracoLabel)
            {
                return UmbEditorMarkup(html.EditorFor(editorFor, viewPath, prefix, viewData), html.ValidationMessageFor(validationFor));
            }

            return html.UmbEditorFor(labelFor, 
                validationFor, 
                html.EditorFor(editorFor, viewPath, prefix, viewData),
                description,
                tooltip);
        }

        /// <summary>
        /// Creates a label, validation message and field for an Umbraco property and ensures it styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditorFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            string description = "",
            string tooltip = "")
        {
            return html.UmbEditorFor(expression, 
                expression, 
                html.EditorFor(expression),
                description,
                tooltip);
        }

        /// <summary>
        /// Creates a label and field for an umbraco property and ensures it is styled correctly
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="viewPath">The editor template to render for the editor</param>
        /// <param name="prefix">The html prefix for the editor</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        public static IHtmlString UmbEditorFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            string viewPath,
            string prefix,
            string description = "",
            string tooltip = "")
        {
            return html.UmbEditorFor(expression, 
                html.EditorFor(expression, viewPath, prefix),
                description,
                tooltip);
        }
        #endregion

        /// <summary>
        /// Renders the output for the umbraco editor markup
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="validationMessage">The validation message.</param>
        /// <param name="description">The description.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns></returns>
        private static IHtmlString UmbEditorMarkup(IHtmlString label, IHtmlString editor, IHtmlString validationMessage = null, string description = "", string tooltip = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class='property-editor clearfix'>");
            if (validationMessage != null)
            {
                sb.Append(validationMessage);
            }
            sb.AppendLine("<div class='property-editor-label' title='"+ tooltip +"'>");
            sb.Append(label);
            if (!description.IsNullOrWhiteSpace())
            {
                sb.AppendLine("<small>");
                sb.Append(description);
                sb.AppendLine("</small>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='property-editor-control'>");
            sb.Append(editor);
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// Renders the output for a full width editor without the standard Umbraco label
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="validationMessage"></param>
        /// <returns></returns>
        private static IHtmlString UmbEditorMarkup(IHtmlString editor, IHtmlString validationMessage = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class='property-editor'>");
            if (validationMessage != null)
            {
                sb.Append(validationMessage);
            }
            sb.AppendLine("<div class='property-editor-control-full-width'>");
            sb.Append(editor);
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return new HtmlString(sb.ToString());
        }


        #endregion

        /// <summary>
        /// Renders the property editor UI elements.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="customProperties">The custom properties.</param>
        /// <returns></returns>
        public static IHtmlString RenderPropertyEditorUIElements(this HtmlHelper html, IEnumerable<ContentProperty> customProperties)
        {
            var defs = new Dictionary<string, IEnumerable<UIElement>>();

            foreach (var p in customProperties.Where(x => x.PropertyEditorModel != null && x.PropertyEditorModel is IHasUIElements))
            {
                var uiElements = ((IHasUIElements)p.PropertyEditorModel).UIElements;
                var filteredUiElements = html.FilterUIElements(uiElements);
                if (filteredUiElements.Any())
                {
                    defs.Add(p.Id.GetHtmlId(), uiElements);
                }
            }

            return new MvcHtmlString(defs.ToJsonString());
        }

        /// <summary>
        /// Convert an object to a JSON string with camelCase formatting
        /// </summary>
        /// <param name="html"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IHtmlString ToJsonString(this HtmlHelper html, object obj)
        {
            return new HtmlString(obj.ToJsonString());
        }

        /// <summary>
        /// Convert an object to a JSON string with the specified formatting
        /// </summary>
        /// <param name="html"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IHtmlString ToJsonString(this HtmlHelper html, object obj, PropertyNamesCaseType propertyNamesCaseType)
        {
            return new HtmlString(obj.ToJsonString(propertyNamesCaseType));
        }

        #region UmbEditorBar

        public static IHtmlString UmbEditorBar(this HtmlHelper html)
        {
            return html.UmbEditorBar(new { });
        }

        public static IHtmlString UmbEditorBar(this HtmlHelper html, object htmlAttributes)
        {
            if (htmlAttributes is IDictionary)
            {
                var d = ((IDictionary)htmlAttributes).ConvertTo<string, object>();
                return html.UmbEditorBar(d);
            }

            IDictionary<string, object> attributes = htmlAttributes.ToDictionary<object>();
            return html.UmbEditorBar(attributes);
        }

        public static IHtmlString UmbEditorBar(this HtmlHelper html, IDictionary<string, object> htmlAttributes)
        {
            var builder = new StringBuilder();
            builder.Append("<div id=\"editorBar\" ");
            if (htmlAttributes != null)
            {
                foreach (var attribute in htmlAttributes)
                {
                    builder.Append(attribute.Key + "='" + attribute.Value + "' ");
                }
            }
            builder.Append(">");
            builder.Append("</div>");

            var defaultUIElementDefs = new List<UIElement>();
            if(html.ViewData.Model is IHasUIElements)
            {
                var uiElements = ((IHasUIElements)html.ViewData.Model).UIElements;
                defaultUIElementDefs.AddRange(html.FilterUIElements(uiElements));
            }

            builder.Append("<script type=\"text/javascript\"> $('#editorBar').EditorBar(" + defaultUIElementDefs.ToJsonString() + "); </script>");
            return new MvcHtmlString(builder.ToString());
        }
        #endregion

        private static IEnumerable<UIElement> FilterUIElements(this HtmlHelper html, IEnumerable<UIElement> elements)
        {
            var filteredElements = new List<UIElement>();

            foreach (var uiElement in elements)
            {
                var attributes = uiElement.GetType().GetCustomAttributes(typeof(UmbracoAuthorizeAttribute), true);
                if (attributes.Length == 0 || attributes.Aggregate(false, (current, attribute) => current || ((UmbracoAuthorizeAttribute)attribute).IsAuthorized(html.ViewContext.HttpContext)))
                {
                    // Authorized or no permission attribute
                    filteredElements.Add(uiElement);
                }
            }

            return filteredElements;
        }
    }
}

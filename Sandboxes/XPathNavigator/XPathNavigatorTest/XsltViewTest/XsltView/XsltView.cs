using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Hosting;
using XsltViewEngine;
using System.Xml.Xsl;
using System.Web.WebPages;
using System.IO;

namespace XsltViewTest
{
    public class XsltView : ActionResult
    {
        public string XsltPath { get; set; }

        public object Model { get; set; }

        public XsltView(object model, string xsltPath = null)
        {
            Model = model;

            if (xsltPath != null)
            {
                XsltPath = xsltPath.StartsWith("~/") ? HostingEnvironment.MapPath(xsltPath) : xsltPath;
            }
        }

        protected virtual XsltArgumentList GetArguments()
        {
            return new XsltArgumentList();
        }



        private static Dictionary<string, XslCompiledTransform> _stylesheets = new Dictionary<string, XslCompiledTransform>();

        public override void ExecuteResult(ControllerContext context)
        {
            string xsltPath = XsltPath;
            if (string.IsNullOrEmpty(xsltPath))
            {
                xsltPath = HostingEnvironment.MapPath(string.Format("~/Views/{0}/{1}.xslt",
                    context.RouteData.Values["controller"],
                    context.RouteData.Values["action"]));
            }            

            context.HttpContext.Response.ContentType = "text/html";
            Transform(xsltPath, context.HttpContext.Response.Output);            
        }

        void Transform(string xsltPath, TextWriter output)
        {
            
            XslCompiledTransform xslt;
            if (!_stylesheets.TryGetValue(xsltPath, out xslt))
            {
                _stylesheets[xsltPath] = xslt = new XslCompiledTransform();
                xslt.Load(xsltPath);
            }

            xslt.Transform(new ObjectNavigator(ElementDescriptor.CreateFor(Model)), GetArguments(), output);
        }


        public HelperResult Transform()
        {
            if( string.IsNullOrEmpty(XsltPath) ) {
                throw new ArgumentNullException("XsltPath");
            }

            return new HelperResult((writer)=>{
                Transform(XsltPath, writer);            
            });
        }
    }
}
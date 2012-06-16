using System;
using Umbraco.Framework;


namespace Umbraco.Cms.Web.EmbeddedViewEngine
{
    /// <summary>
    /// When added to a model property, determines which resource to look up the label string for
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EmbeddedViewAttribute : Attribute
    {

        /// <summary>
        /// Creates an EmbeddedViewAttribute
        /// </summary>
        /// <param name="compiledViewName">Needs to be the fully qualified path of the embedded resource</param>
        /// <param name="assemblyName">The Assembly name that the embedded resouce exists in (without the .dll)</param>
        public EmbeddedViewAttribute(string compiledViewName, string assemblyName)
        {
            Mandate.ParameterNotNullOrEmpty(compiledViewName, "compiledViewName");
            Mandate.ParameterNotNullOrEmpty(assemblyName, "assemblyName");

            CompiledViewName = compiledViewName;
            AssemblyName = assemblyName;
        }

        public string CompiledViewName{ get; private set; }
        public string AssemblyName { get; private set; }

    }
}

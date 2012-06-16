using System;

namespace Umbraco.Framework.Localization.Web.Mvc
{
    [Serializable]
    public class AssemblyNotFoundException : Exception
    {
        public Localization.ExceptionHelper Localization { get; private set; }

        public AssemblyNotFoundException(string key = "Exceptions.AssemblyNotFoundException", object parameters = null, Exception innerException = null)            
        {
            Localization = new Localization.ExceptionHelper(
                this, key, "The assembly for the resource specifier could not be found.", parameters);
        }

        public override string Message { get { return Localization.GetMessage(base.Message); } }
    }
}

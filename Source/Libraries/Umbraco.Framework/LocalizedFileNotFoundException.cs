using System;
using System.IO;
using Umbraco.Framework.Localization;

namespace Umbraco.Framework
{
    [Serializable]
    public class LocalizedFileNotFoundException : FileNotFoundException
    {
        public ExceptionHelper Localization { get; private set; }

        public LocalizedFileNotFoundException(string filename, string key = "Exceptions.FileNotFoundException", string defaultMessage = null, object parameters = null, Exception innerException = null)
            : base(defaultMessage, innerException)
        {
            Localization = new ExceptionHelper(this, key, defaultMessage, parameters);
            Localization.AddParameter("FileName", filename);
        }

        public override string Message { get { return Localization.GetMessage(base.Message); } }
    }    
}

using System;
using System.Collections.Generic;
using Umbraco.Framework.Localization;

namespace Umbraco.Framework
{
    [Serializable]
    public class LocalizedKeyNotFoundException : KeyNotFoundException
    {
        public ExceptionHelper Localization { get; private set; }

        public LocalizedKeyNotFoundException(string key = "Exceptions.KeyNotFoundException", string defaultMessage = null, object parameters = null, Exception innerException = null)
            : base(defaultMessage, innerException)
        {
            Localization = new ExceptionHelper(this, key, defaultMessage, parameters);
        }

        public override string Message { get { return Localization.GetMessage(base.Message); } }
    }
}
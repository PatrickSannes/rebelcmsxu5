using System;

namespace Umbraco.Framework.Persistence.ProviderSupport._Revised
{
    public class ProviderMetadata : DisposableObject
    {
        public ProviderMetadata(string @alias, Uri mappingRoot, bool responsibleForIdGeneration, bool isPassthroughProvider)
        {
            Alias = alias;
            MappingRoot = mappingRoot;
            ResponsibleForIdGeneration = responsibleForIdGeneration;
            IsPassthroughProvider = isPassthroughProvider;
        }

        public string Alias { get; private set; }
        public Uri MappingRoot { get; private set; }
        public bool ResponsibleForIdGeneration { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is passthrough provider, i.e. is not responsible for id generation.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is passthrough provider; otherwise, <c>false</c>.
        /// </value>
        public bool IsPassthroughProvider { get; protected set; }

        protected override void DisposeResources()
        {
            // Note: this class does not technically need to be IDisposable, 
            // but ParallelProviderMetadata accepts IEnumerable<ProviderServicePair<ProviderMetadata> and
            // ProviderServicePair stipulates IDisposable, so it just avoids making another type
            return;
        }
    }
}
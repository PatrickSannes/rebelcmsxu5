using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Hive.Configuration
{
    public class ProviderMappingGroup : IRequiresFrameworkContext
    {
        public string Key { get; protected set; }

        public ProviderMappingGroup(string key,
            WildcardUriMatch singleMatch, 
            ReadonlyProviderSetup singleReader, 
            ProviderSetup singleWriter,
            IFrameworkContext frameworkContext)
            : this(key, Enumerable.Repeat(singleMatch, 1), Enumerable.Repeat(singleReader, 1), Enumerable.Repeat(singleWriter, 1), frameworkContext)
        {
            Mandate.ParameterNotNull(singleReader, "singleReader");
            Mandate.ParameterNotNull(singleWriter, "singleWriter");
        }

        public ProviderMappingGroup(string key,
            IEnumerable<WildcardUriMatch> matches, 
            IEnumerable<ReadonlyProviderSetup> readers,
            IEnumerable<ProviderSetup> writers,
            IFrameworkContext frameworkContext)
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");
            Mandate.ParameterNotNull(readers, "readers");
            Mandate.ParameterNotNull(writers, "writers");
            var readerCount = readers.Count();
            var writerCount = writers.Count();
            Mandate.That(readerCount > 0 || writerCount > 0, 
                x => new ArgumentOutOfRangeException("readers / writers", 
                    "ProviderMappingGroup '{0}' must be supplied with at least one reader or writer. Given Readers: {1}, Writers: {2}"
                    .InvariantFormat(key, readerCount, writerCount)));

            Mandate.ParameterNotNull(frameworkContext, "frameworkContext");

            Key = key;
            FrameworkContext = frameworkContext;
            WildcardUriMatches = matches;
            Readers = readers;
            Writers = writers;
        }

        public IEnumerable<WildcardUriMatch> WildcardUriMatches { get; protected set; }


        private IEnumerable<UriMatchRegex> _generatedUriMatchRegexes = null;
        private IEnumerable<UriMatchRegex> GetAndCacheUriMatchRegexes()
        {
            return _generatedUriMatchRegexes ?? (_generatedUriMatchRegexes = WildcardUriMatches
                .Select(uriMatch =>
                    new UriMatchRegex(uriMatch, new WildcardRegex(FixUriBugs(uriMatch.Root.ToString().TrimEnd('*') + '*'), RegexOptions.IgnoreCase | RegexOptions.Compiled))));
        }

        private readonly ConcurrentDictionary<string, ProviderUriMatchResult> _matchCache = new ConcurrentDictionary<string, ProviderUriMatchResult>();

        /// <summary>
        /// Determines whether this group matches the specified route URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns><c>true</c> if this group is a match for the given URI; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public ProviderUriMatchResult IsMatchForUri(Uri uri)
        {
            Mandate.ParameterNotNull(uri, "uri");
            return IsMatchForUri(FixUriBugs(uri.ToString()));
        }

        private static string FixUriBugs(string uri)
        {
            uri = uri.Replace(":///", "://");
            return uri.EndsWith(":/") ? uri + "/" : uri;
        }

        /// <summary>
        /// Determines whether this group matches the specified route URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns><c>true</c> if this group is a match for the given URI; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public ProviderUriMatchResult IsMatchForUri(string uri)
        {
            Mandate.ParameterNotNullOrEmpty(uri, "uri");

            //TODO: Do more validation on the incoming uri, for now just check the incoming scheme and convert http(s) to content
            var incomingUriBuilder = new UriBuilder(uri);
            if (string.Compare(incomingUriBuilder.Scheme, "http", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(incomingUriBuilder.Scheme, "https", StringComparison.InvariantCultureIgnoreCase) == 0)
                incomingUriBuilder.Scheme = "content";

            incomingUriBuilder.Port = -1;
            // We don't care about the port and setting it to -1 causes UriBuilder not to include it in ToString()

            if (WildcardUriMatches == null) return new ProviderUriMatchResult(false, null);

            LogHelper.TraceIfEnabled<ProviderMappingGroup>("Checking '{0}' against {1} UriMatches", () => uri, () => WildcardUriMatches.Count());
            return _matchCache.GetOrAdd(uri, x =>
                                                   {
                                                       var uriRegexes = GetAndCacheUriMatchRegexes();
                                                       var matches = uriRegexes.Where(uriMatch => uriMatch.Regex.IsMatch(FixUriBugs(incomingUriBuilder.ToString())))
                                                           .Select(match => new ProviderUriMatchResult(true, match.UriMatch))
                                                           .FirstOrDefault();
                                                       return matches ?? new ProviderUriMatchResult(false, null);
                                                   });
        }

        public IEnumerable<ReadonlyProviderSetup> Readers { get; protected set; }

        public IEnumerable<ProviderSetup> Writers { get; protected set; }

        public IFrameworkContext FrameworkContext { get; protected set; }
    }
}
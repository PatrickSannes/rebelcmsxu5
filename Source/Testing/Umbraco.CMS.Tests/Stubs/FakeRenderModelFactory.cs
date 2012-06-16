using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using Rhino.Mocks;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Cms.Stubs
{
    public class FakeRenderModelFactory : IRenderModelFactory
    {
        private readonly IUmbracoApplicationContext _application;

        public static FakeRenderModelFactory CreateWithApp()
        {
            return new FakeRenderModelFactory(new FakeUmbracoApplicationContext());
        }

        public FakeRenderModelFactory(IUmbracoApplicationContext application)
        {
            _application = application;
        }

        public IUmbracoRenderModel Create(HttpContextBase httpContext, string rawUrl)
        {
            var mock = MockRepository.GenerateMock<IUmbracoRenderModel>();

            //if there's a url to match then return the content for it.
            if (_urlsToMatch.Keys.Any(x => Regex.IsMatch(rawUrl, x)))
            {
                var match = _urlsToMatch.First(x => Regex.IsMatch(rawUrl, x.Key));
                mock.Stub(x => x.CurrentNode).Return(match.Value);
            }
            
            return mock;
        }

        private readonly Dictionary<string, Content> _urlsToMatch = new Dictionary<string, Content>();

        /// <summary>
        /// Adds a regex url for the Create method to match against to determine if it should mock a CurrentNode
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content">If the url matches, this is the content objec that will be returned for the CurrentNode mock</param>
        public void AddMatchingUrlForContent(string url, Content content)
        {
            if (!_urlsToMatch.ContainsKey(url))
            {
                _urlsToMatch.Add(url, content);
            }
        }
    }
}
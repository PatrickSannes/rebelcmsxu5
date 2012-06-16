using System;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;

namespace Umbraco.Cms.Web.Model
{

    /// <summary>
    /// The model supplied to the controller to render the content to the front-end.
    /// This model wraps the 'Content' model which is returned by the controller action to the view
    /// and returns the 'ContentNode' as Lazy loaded data.
    /// </summary>
    public class UmbracoRenderModel : DisposableObject, IUmbracoRenderModel
    {
        private readonly Func<Content> _renderItemDeferred;

        public UmbracoRenderModel(IUmbracoApplicationContext baseContext, Content renderItem)
        {
            _baseContext = baseContext;
            _renderItem = renderItem;
        }

        public UmbracoRenderModel(IUmbracoApplicationContext baseContext, Func<Content> renderItemDeferred)
        {
            _baseContext = baseContext;
            _renderItemDeferred = renderItemDeferred;
        }

        private readonly IUmbracoApplicationContext _baseContext;
        private Content _renderItem;

        /// <summary>
        /// Gets the current item associated with this request.
        /// </summary>
        /// <value>The current item.</value>
        public Content CurrentNode
        {
            get
            {
                if (_renderItem == null && _renderItemDeferred != null)
                    _renderItem = _renderItemDeferred.Invoke();
                return _renderItem;
            }
            protected set { _renderItem = value; }
        }        

        protected override void DisposeResources()
        {
            LogHelper.TraceIfEnabled<UmbracoRenderModel>("Disposing");
            _baseContext.FrameworkContext.ScopedFinalizer.FinalizeScope();
        }
    }
}

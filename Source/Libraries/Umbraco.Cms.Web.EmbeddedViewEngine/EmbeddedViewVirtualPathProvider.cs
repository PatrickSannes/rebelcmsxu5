using System;
using System.Web.Hosting;
using System.Web.Caching;
using System.Collections;

namespace Umbraco.Cms.Web.EmbeddedViewEngine
{

    /// <summary>
    /// Virtual path provider for returning embedded Views
    /// </summary>
    /// <remarks>
    /// the virtual path format is:
    /// ~/EV.axd/_EV_MD5HASHOFURLLOCATION or 
    /// ~/EV.axd/FQN.OF.VIEW,SPECIFIEDASSEMBLY
    /// </remarks>
    public class EmbeddedViewVirtualPathProvider : VirtualPathProvider
    {


        public override bool FileExists(string virtualPath)
        {
            return EmbeddedViewPath.IsEmbeddedView(virtualPath) || (Previous != null && Previous.FileExists(virtualPath));
        }

        /// <summary>
        /// Returns the view resource stream 
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public override VirtualFile GetFile(string virtualPath)
        {
            if (EmbeddedViewPath.IsEmbeddedView(virtualPath))
            {
                //to get the file we need to get 
                var hash = EmbeddedViewPath.GetHashFromPath(virtualPath);
                if (!string.IsNullOrEmpty(hash))
                {
                    return new EmbeddedViewVirtualFile(EmbeddedViewPath.GetViewResourceType(hash), virtualPath);
                }
            }            

            //let the base class handle this
            return Previous.GetFile(virtualPath);
        }


        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (EmbeddedViewPath.IsEmbeddedView(virtualPath))
            {
                //TODO: We should actually create a cache dependency file here!

                return null;
            }
            return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }
    }

}
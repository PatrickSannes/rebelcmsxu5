namespace Umbraco.Cms.Web.Routing
{
    /// <summary>
    /// The status returned when a URL is resolved for an entity
    /// </summary>
    public enum UrlResolutionStatus
    {
        /// <summary>
        /// The URL was generated without a hostname
        /// </summary>
        /// <remarks>
        /// This occurs in 2 situations: When a call to generate the URL is made from the context of a node in the non-hostname branch, or
        /// when a call is made to generate a URL from the context of a node in a hostname branch for another node node in the same hostname
        /// umbrella and the current hostname in the HttpContext is equal to the hostname assigned.
        /// </remarks>
        SuccessWithoutHostname = 0,

        /// <summary>
        /// THe URL was generated with a hostname
        /// </summary>
        /// <remarks>
        /// This occurs when a call is made to GetFullUrlsForEntity() or when a call is made to generate a URL for an entity that exists in a hostname branch
        /// but the call is made when the hostname in the HttpContext is not equal to the hostname assigned to the branch.
        /// </remarks>
        SuccessWithHostname = 1,

        /// <summary>
        /// Url generation failed because the URL requested was for a node that exists on a hostname branch but no hostname is assigned
        /// </summary>
        /// <remarks>
        /// This occurs when there are 2 root branches without hostnames and a call is made to generate a URL for an entity on the 2nd root branch which 
        /// requires that a hostname is assigned.
        /// </remarks>
        FailedRequiresHostname = 10,

        //TODO: Implement check for the following:

        /// <summary>
        /// Occurs when a URL is requested from the context of a branch with an assigned hostname 
        /// for an entity that exists in a branch without hostnames assigned and the generated URL already 
        /// matches a relative URL in the current hostname branch.
        /// </summary>
        /// <example>
        /// Here's a tree structure, the root 'hello' node has the domain 'hello.com' assigned to it:
        /// - welcome				/
        /// -- my (id = 1)			/my
        /// --- name				/my/name
        /// ---- is					/my/name/is
        /// ----- shannon			/my/name/is/shannon
        /// - hello (hello.com)		hello.com						
        /// -- my (id = 22)			hello.com/my					
        /// --- name				hello.com/my/name
        /// ---- is					hello.com/my/name/is
        /// ----- shannon			hello.com/my/name/is/shannon
        /// 
        /// When rendering the template for: http://hello.com/my we make a call to:
        /// NiceUrl(22)
        /// this is going to return = "/my"; 
        /// without the domain prefix because we are curring rendering in the domain: hello.com
        /// (another reason we cannot prefix the domain is because there may be many domains assigned to this node/branch)
        /// 
        /// In the same template we make a call to:
        /// NiceUrl(1)
        /// This is going to throw a YSOD because normally in the first root branch this would generate a "/my" url but that is already a relative URL to the current hello.com domain. 
        /// This means that we cannot link to entity with id=1 from the branch hello.com
        /// </example>
        FailedNotRoutableFromCurrentHost = 11,

        /// <summary>
        /// Cannot generate url for non-published node
        /// </summary>
        FailedNotPublished = 12,
    }
}
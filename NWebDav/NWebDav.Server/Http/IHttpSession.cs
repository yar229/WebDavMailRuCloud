using System.Security.Principal;

namespace NWebDav.Server.Http
{
    /// <summary>
    /// HTTP session interface.
    /// </summary>
    public interface IHttpSession
    {
        /// <summary>
        /// Gets the principal of the current request.
        /// </summary>
        /// <value>Principal of the current request.</value>
        IPrincipal Principal { get; }
    }
}
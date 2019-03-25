using System.Security.Principal;

using NWebDav.Server.Http;

namespace NWebDav.Server.HttpListener
{
    public class HttpSession : IHttpSession
    {
        internal HttpSession(IPrincipal principal)
        {
            Principal = principal;
        }

        public IPrincipal Principal { get; }
    }
}

using System.Net;

using NWebDav.Server.Http;

namespace NWebDav.Server.HttpListener
{
    public class HttpContext : HttpBaseContext
    {
        private static readonly IHttpSession s_nullSession = new HttpSession(null);

        public HttpContext(HttpListenerContext httpListenerContext) : base(httpListenerContext.Request, httpListenerContext.Response)
        {
        }

        public override IHttpSession Session => s_nullSession;
    }
}

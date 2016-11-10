using System;
using System.Security.Claims;
using System.Security.Principal;

using NWebDav.Server.Http;

namespace NWebDav.Server.AspNetCore
{
    public partial class AspNetCoreContext
    {
        private class AspNetCoreSession : IHttpSession
        {
            internal AspNetCoreSession(ClaimsPrincipal principal)
            {
                Principal = principal;
            }

            public IPrincipal Principal { get; }
        }
    }
}
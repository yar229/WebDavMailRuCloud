using System;
using System.Threading.Tasks;

using NWebDav.Server.Http;

namespace NWebDav.Server
{
    public interface IWebDavDispatcher
    {
        Task DispatchRequestAsync(IHttpContext httpContext);
    }
}
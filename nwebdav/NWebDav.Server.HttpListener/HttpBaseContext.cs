using System;
using System.Net;

using NWebDav.Server.Http;

namespace NWebDav.Server.HttpListener
{
    public abstract class HttpBaseContext : IHttpContext
    {
        private readonly HttpListenerResponse _response;

        protected HttpBaseContext(HttpListenerRequest request, HttpListenerResponse response)
        {
            // Assign properties
            Request = new HttpRequest(request);
            Response = new HttpResponse(response);

            // Save response
            _response = response;
        }

        public IHttpRequest Request { get; }
        public IHttpResponse Response { get; }
        public abstract IHttpSession Session { get; }

        public void Close()
        {
            // Close the response
            _response.Close();
        }
    }
}

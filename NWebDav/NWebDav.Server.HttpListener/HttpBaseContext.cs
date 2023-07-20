using System.Net;
using System.Threading.Tasks;
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
            response.SendChunked = true;
            Response = new HttpResponse(response);

            // Save response
            _response = response;
        }

        public IHttpRequest Request { get; }
        public IHttpResponse Response { get; }
        public abstract IHttpSession Session { get; }

        public Task CloseAsync()
        {
            try
            {
                // Prevent error of closing stream before all bytes are rent
                _response?.OutputStream?.Flush();
            } catch { }

            // Close the response
            _response?.Close();

            // Command completed synchronous
            return Task.FromResult(true);
        }
    }
}

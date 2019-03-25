using System.IO;
using System.Net;

using NWebDav.Server.Http;

namespace NWebDav.Server.HttpListener
{
    public class HttpResponse : IHttpResponse
    {
        private readonly HttpListenerResponse _response;

        internal HttpResponse(HttpListenerResponse response)
        {
            _response = response;
        }

        public int Status
        {
            get => _response.StatusCode;
            set => _response.StatusCode = value;
        }

        public string StatusDescription
        {
            get => _response.StatusDescription;
            set => _response.StatusDescription = value;
        }

        public void SetHeaderValue(string header, string value)
        {
            switch (header)
            {
                case "Content-Length":
                    _response.ContentLength64 = long.Parse(value);
                    break;

                case "Content-Type":
                    _response.ContentType = value;
                    break;

                default:
                    _response.Headers[header] = value;
                    break;
            }
        }

        public void Abort()
        {
            _response.Abort();
        }

        public Stream Stream => _response.OutputStream;
    }
}
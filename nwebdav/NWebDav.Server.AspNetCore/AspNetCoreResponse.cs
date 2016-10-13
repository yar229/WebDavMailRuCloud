using System;
using System.IO;

using Microsoft.AspNetCore.Http;


using NWebDav.Server.Http;

namespace NWebDav.Server.AspNetCore
{
    public partial class AspNetCoreContext
    {
        private class AspNetCoreResponse : IHttpResponse
        {
            private readonly HttpResponse _response;

            internal AspNetCoreResponse(HttpResponse response)
            {
                _response = response;
            }

            public int Status 
            { 
                get { return _response.StatusCode; }
                set { _response.StatusCode = value; }
            }

            // Status Description isn't send to the requester
            public string StatusDescription
            { 
                get;
                set;
            }

            public void SetHeaderValue(string header, string value)
            {
                switch (header)
                {
                    case "Content-Length":
                        _response.ContentLength = long.Parse(value);
                        break;

                    case "Content-Type":
                        _response.ContentType = value;
                        break;

                    default:
                        _response.Headers[header] = value;
                        break;
                }
            }

            public Stream Stream => _response.Body;
        }
    }
}

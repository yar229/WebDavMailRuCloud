﻿using System.Collections.Generic;
using System.IO;
using System.Net;

using NWebDav.Server.Http;

namespace NWebDav.Server.HttpListener
{
    public class HttpRequest : IHttpRequest
    {
        private readonly HttpListenerRequest _request;

        internal HttpRequest(HttpListenerRequest request)
        {
            _request = request;
        }

        public string HttpMethod => _request.HttpMethod;
        public WebDavUri Url => new($"{_request.Url.Scheme}://{_request.Url.Authority}", _request.RawUrl);
        public string RemoteEndPoint => _request.UserHostName;
        public IEnumerable<string> Headers => _request.Headers.AllKeys;
        public string GetHeaderValue(string header) => _request.Headers[header];
        public Stream Stream => _request.InputStream;
    }
}
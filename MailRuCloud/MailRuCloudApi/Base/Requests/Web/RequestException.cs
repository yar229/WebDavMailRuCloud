using System;
using System.Net;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    [Serializable]
    public class RequestException : Exception
    {
        public RequestException()
        { }

        public RequestException(string message) : base(message) { }

        public RequestException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// HTTP Status Code retuned by server
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Response body text
        /// </summary>
        public string ResponseBody { get; set; }

        /// <summary>
        /// Optional. Human-readable description of the result (by Telegram)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Contents are subject to change in the future (by Telegram)
        /// </summary>
        public long? ErrorCode { get; set; }
    }

}

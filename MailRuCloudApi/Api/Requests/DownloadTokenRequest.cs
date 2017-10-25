using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MailRuCloudApi.Api.Requests.Types;

namespace MailRuCloudApi.Api.Requests
{
    class DownloadTokenRequest : BaseRequest<DownloadTokenResult>
    {
        public DownloadTokenRequest(CloudApi cloudApi) : base(cloudApi)
        {
        }

        public override string RelationalUri
        {
            get
            {
                var uri = $"/api/v2/tokens/download?token={CloudApi.Account.AuthToken}";
                return uri;
            }
        }
    }



    class DownloadTokenHtmlRequest : BaseRequest<DownloadTokenResult>
    {
        private readonly string _url;

        public DownloadTokenHtmlRequest(CloudApi cloudApi, string url) : base(cloudApi)
        {
            _url = url;
        }

        public override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest(ConstSettings.AuthDomain);
            request.Accept = ConstSettings.DefaultAcceptType;
            return request;
        }

        public override string RelationalUri => _url;

        //protected override byte[] CreateHttpContent()
        //{
        //    string data = $"Login={Uri.EscapeUriString(_login)}&Domain={ConstSettings.Domain}&Password={Uri.EscapeUriString(_password)}";

        //    return Encoding.UTF8.GetBytes(data);
        //}

        protected override RequestResponse<DownloadTokenResult> DeserializeMessage(string responseText)
        {
            var m = Regex.Match(responseText,
                @"""tokens"":{""download""\s*:\s*""(?<token>.*?)""");

            var msg = new RequestResponse<DownloadTokenResult>
            {
                Ok = m.Success,
                Result = new DownloadTokenResult
                {
                    body = new DownloadTokenBody{token = m.Groups["token"].Value } 
                }
            };
            return msg;
        }
    }


}
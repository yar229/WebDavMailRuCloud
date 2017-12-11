using System.Net;
using System.Text.RegularExpressions;
using YaR.MailRuCloud.Api.Base.Requests.Repo;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class DownloadTokenRequest : BaseRequestJson<DownloadTokenResult>
    {
        public DownloadTokenRequest(IWebProxy proxy, IAuth auth) : base(proxy, auth)
        {
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = $"/api/v2/tokens/download?token={Auth.AccessToken}";
                return uri;
            }
        }
    }



    class DownloadTokenHtmlRequest : BaseRequestString<DownloadTokenResult>
    {
        private readonly string _url;

        public DownloadTokenHtmlRequest(IWebProxy proxy, IAuth auth, string url) : base(proxy, auth)
        {
            _url = url;
        }

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest(ConstSettings.AuthDomain);
            request.Accept = ConstSettings.DefaultAcceptType;
            return request;
        }

        protected override string RelationalUri => _url;

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
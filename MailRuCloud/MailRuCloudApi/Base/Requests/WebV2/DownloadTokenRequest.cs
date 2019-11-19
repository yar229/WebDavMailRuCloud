using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class DownloadTokenRequest : BaseRequestJson<DownloadTokenResult>
    {
        public DownloadTokenRequest(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
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



    //class DownloadTokenHtmlRequest : BaseRequestString<DownloadTokenResult>
    //{
    //    public DownloadTokenHtmlRequest(HttpCommonSettings settings, IAuth auth, string url) : base(settings, auth)
    //    {
    //        RelationalUri = url;
    //    }

    //    protected override HttpWebRequest CreateRequest(string baseDomain = null)
    //    {
    //        var request = base.CreateRequest(CommonSettings.AuthDomain);
    //        request.Accept = CommonSettings.DefaultAcceptType;
    //        return request;
    //    }

    //    protected override string RelationalUri { get; }

    //    protected override RequestResponse<DownloadTokenResult> DeserializeMessage(string responseText)
    //    {
    //        var m = Regex.Match(responseText,
    //            @"""tokens"":{""download""\s*:\s*""(?<token>.*?)""");

    //        var msg = new RequestResponse<DownloadTokenResult>
    //        {
    //            Ok = m.Success,
    //            Result = new DownloadTokenResult
    //            {
    //                Body = new DownloadTokenBody{Token = m.Groups["token"].Value } 
    //            }
    //        };
    //        return msg;
    //    }
    //}


}
using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Auth;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{

   class RemoveRequest : BaseRequestJson<WebV2.RemoveRequest.Result>
    {
        private readonly string _fullPath;

        public RemoveRequest(HttpCommonSettings settings, IAuth auth, string fullPath) 
            : base(settings, auth)
        {
            _fullPath = fullPath;
        }

        protected override string RelationalUri => $"/api/m1/file/remove?access_token={Auth.AccessToken}&home={Uri.EscapeDataString(_fullPath)}";

        //protected override byte[] CreateHttpContent()
        //{
        //    //var data = string.Format("home={0}&api={1}&access_token={2}&email={3}&x-email={3}", Uri.EscapeDataString(_fullPath), 2, Auth.AccessToken, Auth.Login);
        //    var data = string.Format("home={0}&api={1}&email={2}&x-email={2}", Uri.EscapeDataString(_fullPath), 2, Auth.Login);
        //    return Encoding.UTF8.GetBytes(data);
        //}
    }
}

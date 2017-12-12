using System.IO;
using System.Net;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    internal abstract class BaseRequestString<T> : BaseRequest<string, T> where T : class
    {
        protected BaseRequestString(IWebProxy proxy, IAuth auth) : base(proxy, auth)
        {
        }

        protected override string Transport(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}

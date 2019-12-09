using System.IO;
using YaR.MailRuCloud.Api.Base.Repos;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    internal abstract class BaseRequestString<T> : BaseRequest<string, T> where T : class
    {
        protected BaseRequestString(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
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

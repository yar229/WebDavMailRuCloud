using System.IO;
using YaR.Clouds.Base.Repos;

namespace YaR.Clouds.Base.Requests
{
    internal abstract class BaseRequestString<T> : BaseRequest<string, T> where T : class
    {
        protected BaseRequestString(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
        {
        }

        protected override string Transport(Stream stream)
        {
            using var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }
    }
}

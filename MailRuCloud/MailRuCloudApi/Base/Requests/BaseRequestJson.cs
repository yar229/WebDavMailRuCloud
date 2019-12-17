using System.IO;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Repos;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    internal abstract class BaseRequestJson<T> : BaseRequest<Stream, T> where T : class
    {
        protected BaseRequestJson(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
        {
        }

        protected override Stream Transport(Stream stream)
        {
            return stream;
        }

        protected override RequestResponse<T> DeserializeMessage(Stream stream)
        {
        //    var serializer = new JsonSerializer();

        //    using (var sr = new StreamReader(stream))
        //    using (var jsonTextReader = new JsonTextReader(sr))
        //    {
                

        //        var msg = new RequestResponse<T>
        //        {
        //            Ok = true,
        //            Result = serializer.Deserialize<T>(jsonTextReader)
        //        };
        //        return msg;
        //    }

            using (var sr = new StreamReader(stream))
            {
                string text = sr.ReadToEnd();

                var msg = new RequestResponse<T>
                {
                    Ok = true,
                    Result = JsonConvert.DeserializeObject<T>(text)
                };
                return msg;

            }

        }


    }
}
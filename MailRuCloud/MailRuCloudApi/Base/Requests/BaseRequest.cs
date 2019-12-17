using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Repos;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    internal abstract class BaseRequest<TConvert, T> where T : class
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(BaseRequest<TConvert, T>));

        protected readonly HttpCommonSettings Settings;
        protected readonly IAuth Auth;

        protected BaseRequest(HttpCommonSettings settings, IAuth auth)
        {
            Settings = settings;
            Auth = auth;
        }

        protected abstract string RelationalUri { get; }

        protected virtual HttpWebRequest CreateRequest(string baseDomain = null)
        {
            string domain = string.IsNullOrEmpty(baseDomain) ? ConstSettings.CloudDomain : baseDomain;
            var uriz = new Uri(new Uri(domain), RelationalUri);
            
            // supressing escaping is obsolete and breaks, for example, chinese names
            // url generated for %E2%80%8E and %E2%80%8F seems ok, but mail.ru replies error
            // https://stackoverflow.com/questions/20211496/uri-ignore-special-characters
            //var udriz = new Uri(new Uri(domain), RelationalUri, true);

            var request = (HttpWebRequest)WebRequest.Create(uriz);
            request.Proxy = Settings.Proxy;
            request.CookieContainer = Auth?.Cookies;
            request.Method = "GET";
            request.ContentType = ConstSettings.DefaultRequestType;
            request.Accept = "application/json";
            request.UserAgent = Settings.UserAgent;
            
            return request;
        }

        protected virtual byte[] CreateHttpContent()
        {
            return null;
        }


        public virtual async Task<T> MakeRequestAsync()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            var httprequest = CreateRequest();

            var content = CreateHttpContent();
            if (content != null)
            {
                httprequest.Method = "POST";
                var stream = httprequest.GetRequestStream();
                stream.Write(content, 0, content.Length);
            }
            try
            {
                using (var response = (HttpWebResponse) await Task.Factory.FromAsync(httprequest.BeginGetResponse,
                    asyncResult => httprequest.EndGetResponse(asyncResult), null))
                {
                    if ((int) response.StatusCode >= 500)
                        throw new RequestException("Server fault")
                        {
                            StatusCode = response.StatusCode
                        }; // Let's throw exception. It's server fault

                    RequestResponse<T> result;
                    using (var responseStream = response.GetResponseStream())
                    {
                        result = DeserializeMessage(Transport(responseStream));
                    }

                    if (!result.Ok || response.StatusCode != HttpStatusCode.OK)
                    {
                        var exceptionMessage =
                            $"Request failed (status code {(int) response.StatusCode}): {result.Description}";
                        throw new RequestException(exceptionMessage)
                        {
                            StatusCode = response.StatusCode,
                            ResponseBody = string.Empty,
                            Description = result.Description,
                            ErrorCode = result.ErrorCode
                        };
                    }
                    var retVal = result.Result;

                    return retVal;
                }
            }
            // ReSharper disable once RedundantCatchClause
            #pragma warning disable 168
            catch (Exception ex)
            #pragma warning restore 168
            {
                throw;
            }
            finally
            {
                watch.Stop();
                Logger.Debug($"HTTP:{httprequest.Method}:{httprequest.RequestUri.AbsoluteUri} ({watch.Elapsed.Milliseconds} ms)");
            }


        }

        protected abstract TConvert Transport(Stream stream);

        protected abstract RequestResponse<T> DeserializeMessage(TConvert data);
    }
}

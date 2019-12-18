using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Streams;
using YaR.MailRuCloud.Api.Common;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Repos.MailRuCloud
{
    abstract class MailRuBaseRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(MailRuBaseRepo));

        public static readonly string[] AvailDomains = {"mail", "inbox", "bk", "list"};

        protected MailRuBaseRepo(IBasicCredentials creds)
        {
            Credentials = creds;

            if (!AvailDomains.Any(d => creds.Login.Contains($"@{d}.")))
            {
                string domains = AvailDomains.Aggregate((c, n) => c + ", @" + n);
                Logger.Warn($"Missing domain part ({domains}) in login, file and folder deleting will be denied");
            }
        }

        protected readonly IBasicCredentials Credentials;

        public IAuth Authent { get; protected set; }
        public abstract HttpCommonSettings HttpSettings { get; }

        public abstract Task<ShardInfo> GetShardInfo(ShardType shardType);

        public string ConvertToVideoLink(string publicLink, SharedVideoResolution videoResolution)
        {
            return GetShardInfo(ShardType.WeblinkVideo).Result.Url +
                   videoResolution.ToEnumMemberValue() + "/" + //"0p/" +
                   Common.Base64Encode(publicLink.TrimStart('/')) +
                   ".m3u8?double_encode=1";
        }

        public ICloudHasher GetHasher()
        {
            return new MailRuSha1Hash();
        }

        public bool SupportsAddSmallFileByHash => true;



        private HttpRequestMessage UploadClientRequest(PushStreamContent content, File file)
        {
            var shard = GetShardInfo(ShardType.Upload).Result;
            var url = new Uri($"{shard.Url}?token={Authent.AccessToken}");

            var request = new HttpRequestMessage
            {
                RequestUri = url,
                Method = HttpMethod.Put
            };

            request.Headers.Add("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("User-Agent", HttpSettings.UserAgent);

            request.Content = content;
            request.Content.Headers.ContentLength = file.OriginalSize;

            return request;
        }

        public async Task<UploadFileResult> DoUpload(HttpClient client, PushStreamContent content, File file)
        {
            var request = UploadClientRequest(content, file);
            var responseMessage = await client.SendAsync(request);
            var ures = responseMessage.ToUploadPathResult();

            return ures;
        }
    }
}

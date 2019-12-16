using System;
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

        public HttpRequestMessage UploadClientRequest(PushStreamContent content, File file)
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Streams;
using YaR.MailRuCloud.Api.Common;
using YaR.MailRuCloud.Api.Links;
using Stream = System.IO.Stream;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb
{
    class YadWebRequestRepo : IRequestRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(YadWebRequestRepo));





        public YadWebRequestRepo(IWebProxy proxy, IBasicCredentials creds)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            HttpSettings.Proxy = proxy;
            Authent = new YadWebAuth(HttpSettings, creds);
        }

        public IAuth Authent { get; }

        public HttpCommonSettings HttpSettings { get; } = new HttpCommonSettings
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36"
        };

        public Stream GetDownloadStream(File afile, long? start = null, long? end = null)
        {
            CustomDisposable<HttpWebResponse> ResponseGenerator(long instart, long inend, File file)
            {
                var urldata = new YadGetResourceUrlRequest(HttpSettings, (YadWebAuth)Authent, file.FullPath)
                    .MakeRequestAsync()
                    .Result;

                var url = "https:" + urldata.Models[0].Data.File; //var url = urldata.Models[0].Data.Digest;
                HttpWebRequest request = new YadDownloadRequest(HttpSettings, (YadWebAuth)Authent, url, instart, inend);
                var response = (HttpWebResponse)request.GetResponse();

                return new CustomDisposable<HttpWebResponse>
                {
                    Value = response,
                    OnDispose = () => {}
                };
            }

            var stream = new DownloadStream(ResponseGenerator, afile, start, end);
            return stream;
        }

        //public HttpWebRequest UploadRequest(File file, UploadMultipartBoundary boundary)
        //{
        //    var urldata = 
        //        new YadGetResourceUploadUrlRequest(HttpSettings, (YadWebAuth)Authent, file.FullPath, file.OriginalSize)
        //        .MakeRequestAsync()
        //        .Result;
        //    var url = urldata.Models[0].Data.UploadUrl;

        //    var result = new YadUploadRequest(HttpSettings, (YadWebAuth)Authent, url, file.OriginalSize);
        //    return result;
        //}

        public ICloudHasher GetHasher()
        {
            return null;
        }

        public bool SupportsAddSmallFileByHash => false;

        private HttpRequestMessage UploadClientRequest(PushStreamContent content, File file)
        {
            var urldata = 
                new YadGetResourceUploadUrlRequest(HttpSettings, (YadWebAuth)Authent, file.FullPath, file.OriginalSize)
                    .MakeRequestAsync()
                    .Result;
            var url = urldata.Models[0].Data.UploadUrl;

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
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



        public async Task<IEntry> FolderInfo(string path, Link ulink, int offset = 0, int limit = Int32.MaxValue, int depth = 1)
        {
            var itemInfo = await new YadItemInfoRequest(HttpSettings, (YadWebAuth)Authent, path)
                .MakeRequestAsync();
            var itdata = itemInfo.Models.First().Data;

            if (itdata?.Type == null)
                return null;

            if (itdata.Type == "file")
                return itdata.ToFile();

            YadRequestResult<FolderInfoDataResources, FolderInfoParamsResources> datares;
            try
            {
                datares = await new YadFolderInfoRequest(HttpSettings, (YadWebAuth)Authent, path, offset, limit)
                    .MakeRequestAsync();
            }
            catch (WebException e) when ((e.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var entry = datares.ToFolder(path);

            return entry;
        }

        public Task<FolderInfoResult> ItemInfo(string path, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        {
            throw new NotImplementedException();
        }

        public async Task<AccountInfoResult> AccountInfo()
        {
            var req = await new YadAccountInfoRequest(HttpSettings, (YadWebAuth)Authent).MakeRequestAsync();
            var res = req.ToAccountInfo();
            return res;
        }

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            var req = await new YadCreateFolderRequest(HttpSettings, (YadWebAuth)Authent, path)
                .MakeRequestAsync();
            var res = req.ToCreateFolderResult();
            return res;
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime,
            ConflictResolver? conflictResolver)
        {
            var res = new AddFileResult
            {
                Path = fileFullPath,
                Success = true
            };

            return await Task.FromResult(res);
        }

        public Task<CloneItemResult> CloneItem(string fromUrl, string toPath)
        {
            throw new NotImplementedException();
        }

        public async Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            string destFullPath = WebDavPath.Combine(destinationPath, WebDavPath.Name(sourceFullPath));

            var req = await new YadCopyRequest(HttpSettings, (YadWebAuth)Authent, sourceFullPath, destFullPath)
                .MakeRequestAsync();
            var res = req.ToCopyResult();
            return res;
        }

        public async Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            string destFullPath = WebDavPath.Combine(destinationPath, WebDavPath.Name(sourceFullPath));

            var req = await new YadMoveRequest(HttpSettings, (YadWebAuth)Authent, sourceFullPath, destFullPath)
                .MakeRequestAsync();

            var res = req.ToMoveResult();
            return res;
        }

        public Task<PublishResult> Publish(string fullPath)
        {
            throw new NotImplementedException();
        }

        public Task<UnpublishResult> Unpublish(string publicLink)
        {
            throw new NotImplementedException();
        }

        public async Task<RemoveResult> Remove(string fullPath)
        {
            var req = await new YadDeleteRequest(HttpSettings, (YadWebAuth)Authent, fullPath)
                .MakeRequestAsync();
            var res = req.ToRemoveResult();
            return res;
        }

        public async Task<RenameResult> Rename(string fullPath, string newName)
        {
            string destPath = WebDavPath.Parent(fullPath);
            destPath = WebDavPath.Combine(destPath, newName);

            var req = await new YadMoveRequest(HttpSettings, (YadWebAuth)Authent, fullPath, destPath).MakeRequestAsync();
            var res = req.ToRenameResult();
            return res;
        }

        public Dictionary<ShardType, ShardInfo> GetShardInfo1()
        {
            throw new NotImplementedException();
        }

        public string GetShareLink(string path)
        {
            throw new NotImplementedException();
        }













        public string ConvertToVideoLink(string publicLink, SharedVideoResolution videoResolution)
        {
            throw new NotImplementedException("Yad not implemented ConvertToVideoLink");
        }
    }
}

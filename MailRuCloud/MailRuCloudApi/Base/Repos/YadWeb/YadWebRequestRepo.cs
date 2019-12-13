﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Streams;
using YaR.MailRuCloud.Api.Links;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb
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

        public Stream GetDownloadStream(File file, long? start = null, long? end = null)
        {
            throw new NotImplementedException();
        }

        public HttpWebRequest UploadRequest(ShardInfo shard, File file, UploadMultipartBoundary boundary)
        {
            throw new NotImplementedException();
        }

        public Task<ShardInfo> GetShardInfo(ShardType shardType)
        {
            throw new NotImplementedException();
        }

        public Task<IEntry> FolderInfo(string path, Link ulink, int offset = 0, int limit = Int32.MaxValue, int depth = 1)
        {
            throw new NotImplementedException();
        }

        public Task<FolderInfoResult> ItemInfo(string path, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        {
            throw new NotImplementedException();
        }

        public async Task<AccountInfoResult> AccountInfo()
        {
            var req = await new YadAccountInfoRequest(HttpSettings, Authent).MakeRequestAsync();
            var res = req.ToAccountInfo();
            return res;
        }

        public Task<CreateFolderResult> CreateFolder(string path)
        {
            throw new NotImplementedException();
        }

        public Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime,
            ConflictResolver? conflictResolver)
        {
            throw new NotImplementedException();
        }

        public Task<CloneItemResult> CloneItem(string fromUrl, string toPath)
        {
            throw new NotImplementedException();
        }

        public Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            throw new NotImplementedException();
        }

        public Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            throw new NotImplementedException();
        }

        public Task<PublishResult> Publish(string fullPath)
        {
            throw new NotImplementedException();
        }

        public Task<UnpublishResult> Unpublish(string publicLink)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveResult> Remove(string fullPath)
        {
            throw new NotImplementedException();
        }

        public Task<RenameResult> Rename(string fullPath, string newName)
        {
            throw new NotImplementedException();
        }

        public Dictionary<ShardType, ShardInfo> GetShardInfo1()
        {
            throw new NotImplementedException();
        }

        public string GetShareLink(string path)
        {
            throw new NotImplementedException();
        }
    }
}

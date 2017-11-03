using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Extensions
{
    public static class DtoImport
    {
        public static AccountInfo ToAccountInfo(this AccountInfoResult data)
        {
            var res = new AccountInfo
            {
                FileSizeLimit = data.body.cloud.file_size_limit
            };
            return res;
        }


        public static DiskUsage ToDiskUsage(this AccountInfoResult data)
        {
            var res = new DiskUsage
            {
                Total = data.body.cloud.space.total * 1024 * 1024,
                Used = data.body.cloud.space.used * 1024 * 1024,
                OverQuota = data.body.cloud.space.overquota
            };
            return res;
        }

        public static string ToToken(this AuthTokenResult data)
        {
            var res = data.body.token;
            return res;
        }


        public static string ToToken(this DownloadTokenResult data)
        {
            var res = data.body.token;
            return res;
        }


        public static Dictionary<ShardType, ShardInfo> ToShardInfo(this ShardInfoResult webdata)
        {
            var dict = new Dictionary<ShardType, ShardInfo>
            {
                {ShardType.Video,             new ShardInfo{Type = ShardType.Video,       Url = webdata.body.video[0].url} },    
                {ShardType.ViewDirect,        new ShardInfo{Type = ShardType.ViewDirect,  Url = webdata.body.view_direct[0].url} },
                {ShardType.WeblinkView,       new ShardInfo{Type = ShardType.WeblinkView, Url = webdata.body.weblink_view[0].url} },
                {ShardType.WeblinkVideo,      new ShardInfo{Type = ShardType.WeblinkVideo, Url = webdata.body.weblink_video[0].url} },
                {ShardType.WeblinkGet,        new ShardInfo{Type = ShardType.WeblinkGet, Url = webdata.body.weblink_get[0].url} },
                {ShardType.WeblinkThumbnails, new ShardInfo{Type = ShardType.WeblinkThumbnails, Url = webdata.body.weblink_thumbnails[0].url} },
                {ShardType.Auth,              new ShardInfo{Type = ShardType.Auth, Url = webdata.body.auth[0].url} },
                {ShardType.View,              new ShardInfo{Type = ShardType.View, Url = webdata.body.view[0].url} },
                {ShardType.Get,               new ShardInfo{Type = ShardType.Get, Url = webdata.body.get[0].url} },
                {ShardType.Upload,            new ShardInfo{Type = ShardType.Upload, Url = webdata.body.upload[0].url} },
                {ShardType.Thumbnails,        new ShardInfo{Type = ShardType.Thumbnails, Url = webdata.body.thumbnails[0].url} }
            };

            return dict;
        }

        private static readonly string[] FolderKinds = { "folder", "camera-upload", "mounted", "shared" };

        public static IEntry ToEntry(this FolderInfoResult data)
        {
            if (data.body.kind == "file")
            {
                var file = data.ToFile();
                return file;
            }

            var folder = new Folder(data.body.size, WebDavPath.Combine(data.body.home ?? WebDavPath.Root, data.body.name))
            {
                Folders = data.body.list?
                    .Where(it => FolderKinds.Contains(it.kind))
                    .Select(item => item.ToFolder())
                    .ToList(),
                Files = data.body.list?
                    .Where(it => it.kind == "file")
                    .Select(item => item.ToFile())
                    .ToGroupedFiles()
                    .ToList()
            };


            return folder;
        }


        public static Folder ToFolder(this FolderInfoResult data)
        {
            var folder = new Folder(data.body.size, data.body.home)
            {
                Folders = data.body.list?
                    .Where(it => FolderKinds.Contains(it.kind))
                    .Select(item => item.ToFolder())
                    .ToList(),
                Files = data.body.list?
                    .Where(it => it.kind == "file")
                    .Select(item => item.ToFile())
                    .ToGroupedFiles()
                    .ToList()
            };

            return folder;
        }

        public static File ToFile(this FolderInfoResult data, string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return new File(WebDavPath.Combine(data.body.home ?? "", data.body.name), data.body.size);
            }

            var groupedFile = data.body.list?
                .Where(it => it.kind == "file")
                .Select(it => it.ToFile())
                .ToGroupedFiles()
                .First(it => it.Name == filename);

            return groupedFile;
        }

        private static Folder ToFolder(this FolderInfoProps item)
        {
            var folder = new Folder(item.size, item.home, string.IsNullOrEmpty(item.weblink) ? "" : ConstSettings.PublishFileLink + item.weblink);
            return folder;
        }

        private static File ToFile(this FolderInfoProps item)
        {
            var file = new File(item.home, item.size, item.hash)
            {
                PublicLink =
                    string.IsNullOrEmpty(item.weblink) ? "" : ConstSettings.PublishFileLink + item.weblink,
                CreationTimeUtc = UnixTimeStampToDateTime(item.mtime),
                LastAccessTimeUtc = UnixTimeStampToDateTime(item.mtime),
                LastWriteTimeUtc = UnixTimeStampToDateTime(item.mtime),
            };
            return file;
        }

        private static IEnumerable<File> ToGroupedFiles(this IEnumerable<File> list)
        {
            var groupedFiles = list
                .GroupBy(f => Regex.Match(f.Name, @"(?<name>.*?)(\.wdmrc\.(crc|\d\d\d))?\Z").Groups["name"].Value,
                    file => file)
                .Select(group => group.Count() == 1
                    ? group.First()
                    : new SplittedFile(group.ToList()));
            return groupedFiles;
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}

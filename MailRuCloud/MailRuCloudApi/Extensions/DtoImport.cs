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




        public static ShardInfo ToShardInfo(this ShardInfoResult webdata, ShardType shardType)
        {
            List<ShardSection> shard;

            switch (shardType)
            {
                case ShardType.Video:
                    shard = webdata.body.video;
                    break;
                case ShardType.ViewDirect:
                    shard = webdata.body.view_direct;
                    break;
                case ShardType.WeblinkView:
                    shard = webdata.body.weblink_view;
                    break;
                case ShardType.WeblinkVideo:
                    shard = webdata.body.weblink_video;
                    break;
                case ShardType.WeblinkGet:
                    shard = webdata.body.weblink_get;
                    break;
                case ShardType.WeblinkThumbnails:
                    shard = webdata.body.weblink_thumbnails;
                    break;
                case ShardType.Auth:
                    shard = webdata.body.auth;
                    break;
                case ShardType.View:
                    shard = webdata.body.view;
                    break;
                case ShardType.Get:
                    shard = webdata.body.get;
                    break;
                case ShardType.Upload:
                    shard = webdata.body.upload;
                    break;
                case ShardType.Thumbnails:
                    shard = webdata.body.thumbnails;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shardType), shardType, null);
            }

            if (null == shard || shard.Count == 0)
                throw new Exception("Cannot get shard info");

            var res = new ShardInfo
            {
                Type = shardType,
                Count = int.Parse(shard[0].count),
                Url = shard[0].url

            };

            return res;
        }

        private static readonly string[] FolderKinds = { "folder", "camera-upload", "mounted", "shared" };

        //public static Entry ToEntry(this FolderInfoResult data)
        //{
        //    var entry = new Entry(
        //            data.body.list?
        //                .Where(it => FolderKinds.Contains(it.kind))
        //                .Select(it => new Folder(it.count.folders, it.count.files, it.size, it.home, string.IsNullOrEmpty(it.weblink) ? "" : ConstSettings.PublishFileLink + it.weblink))
        //                .ToList(),
        //            data.body.list?
        //                .Where(it => it.kind == "file")
        //                .Select(it => new File(it.home, it.size, it.hash)
        //                {
        //                    PublicLink =
        //                        string.IsNullOrEmpty(it.weblink) ? "" : ConstSettings.PublishFileLink + it.weblink,
        //                    CreationTimeUtc = UnixTimeStampToDateTime(it.mtime),
        //                    LastAccessTimeUtc = UnixTimeStampToDateTime(it.mtime),
        //                    LastWriteTimeUtc = UnixTimeStampToDateTime(it.mtime),
        //                }).ToList(),
        //            data.body.home)
        //    {
        //        CreationDate = DateTime.Now,
        //        Name = data.body.name,
        //        IsFile = data.body.kind == "file",
        //        Size = data.body.size,
        //        //WebLink = data.body.
        //        //WebLink = (data.body?.list != null && data.body.list.Count > 0)
        //        //    ? data.body.list[0].weblink
        //        //    : string.Empty
        //    };

        //    return entry;
        //}

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

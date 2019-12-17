using System;
using System.Linq;
using YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb
{
    static class DtoImportYadWeb
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(DtoImportYadWeb));

        public static AccountInfoResult ToAccountInfo(this YadRequestResult<DataAccountInfo, ParamsAccountInfo> data)
        {
            var info = data.Models[0].Data;
            var res = new AccountInfoResult
            {
                
                FileSizeLimit = info.FilesizeLimit,

                DiskUsage = new DiskUsage
                {
                    Total = info.Limit,
                    Used = info.Used,
                    OverQuota = info.Used > info.Limit
                }
            };

            return res;
        }


        public static IEntry ToFolder(this YadRequestResult<FolderInfoDataResources, FolderInfoParamsResources> data, string path)
        {
            var fi = data.Models
                .First(m => m.ModelName == "resources")
                .Data
                .Resources;

            var res = new Folder(path) { IsChildsLoaded = true };

            res.Files.AddRange(fi
                .Where(it => it.Type == "file")
                .Select(f => f.ToFile()));

            foreach (var it in fi.Where(it => it.Type == "dir"))
            {
                res.Folders.Add(it.ToFolder());
            }

            return res;
        }

        public static File ToFile(this FolderInfoDataResource data)
        {
            var path = data.Path.Remove(0, 5); // remove "/disk"

            var res = new File(path, data.Meta.Size ?? throw new Exception("File size is null"))
            {
                CreationTimeUtc = UnixTimeStampToDateTime(data.Ctime, DateTime.MinValue),
                LastAccessTimeUtc = UnixTimeStampToDateTime(data.Utime, DateTime.MinValue),
                LastWriteTimeUtc = UnixTimeStampToDateTime(data.Mtime, DateTime.MinValue)
            };
            return res;
        }

        public static Folder ToFolder(this FolderInfoDataResource resource)
        {
            var path = resource.Path.Remove(0, 5); // remove "/disk"

            var res = new Folder(path) { IsChildsLoaded = false };

            return res;
        }

        public static RenameResult ToRenameResult(this YadRequestResult<MoveData, MoveParams> data)
        {
            var res = new RenameResult
            {
                IsSuccess = true
            };
            return res;
        }

        public static RemoveResult ToRemoveResult(this YadRequestResult<YadDeleteRequestData, YadDeleteRequestParams> data)
        {
            var res = new RemoveResult
            {
                IsSuccess = data.Models?.FirstOrDefault() != null
            };
            return res;
        }

        public static CreateFolderResult ToCreateFolderResult(this YadRequestResult<YadCreateFolderRequestData, YadCreateFolderRequestParams> data)
        {
            var res = new CreateFolderResult
            {
                IsSuccess = true,
                Path = data.Models[0].Params.Id.Remove(0, "/disk".Length)
            };
            return res;
        }






        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp, DateTime defaultvalue)
        {
            try
            {
                // Unix timestamp is seconds past epoch
                var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp); //.ToLocalTime(); - doesn't need, clients usially convert to localtime by itself
                return dtDateTime;
            }
            catch (Exception e)
            {
                Logger.Error($"Error converting unixTimeStamp {unixTimeStamp} to DateTime, {e.Message}");
                return defaultvalue;
            }
        }
    }
}

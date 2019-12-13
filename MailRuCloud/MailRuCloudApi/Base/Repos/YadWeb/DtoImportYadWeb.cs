using System;
using YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb
{
    static class DtoImportYadWeb
    {
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


        public static IEntry ToFolder(this YadRequestResult<DataResources, ParamsResources> data, string path)
        {
            throw new NotImplementedException("Yad ToFolder");
        }
    }
}

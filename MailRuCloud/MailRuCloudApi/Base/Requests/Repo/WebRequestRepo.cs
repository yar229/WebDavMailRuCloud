using System;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    class WebRequestRepo: IRequestRepo
    {
        private readonly CloudApi _cloudApi;

        public WebRequestRepo(CloudApi cloudApi)
        {
            _cloudApi = cloudApi;
        }

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            return (await new Web.CreateFolderRequest(_cloudApi, path).MakeRequestAsync())
                .ToCreateFolderResult();
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            var res = await new Web.CreateFileRequest(_cloudApi, fileFullPath, fileHash, fileSize, conflictResolver)
                .MakeRequestAsync();

            return res.ToAddFileResult();
        }
    }
}
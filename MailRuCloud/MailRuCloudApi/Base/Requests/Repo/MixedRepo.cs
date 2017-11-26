using System;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    class MixedRepo : IRequestRepo
    {
        private readonly IRequestRepo _webRepo;
        private readonly IRequestRepo _mobileRepo;

        public MixedRepo(CloudApi cloudApi)
        {
            _webRepo = new WebRequestRepo(cloudApi);
            _mobileRepo = new MobileRequestRepo(cloudApi);
        }

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            return await _webRepo.CreateFolder(path);
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            return await _mobileRepo.AddFile(fileFullPath, fileHash, fileSize, dateTime, conflictResolver);
        }
    }
}
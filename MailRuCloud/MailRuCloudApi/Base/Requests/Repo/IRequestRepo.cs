using System;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    interface IRequestRepo
    {
        Task<CreateFolderResult> CreateFolder(string path);
        Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver);
    }
}

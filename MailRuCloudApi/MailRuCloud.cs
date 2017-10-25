//-----------------------------------------------------------------------
// <created file="MailRuCloudApi.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MailRuCloudApi.Api;
using MailRuCloudApi.Api.Requests;
using MailRuCloudApi.Extensions;
using MailRuCloudApi.PathResolve;


namespace MailRuCloudApi
{


    /// <summary>
    /// Cloud client.
    /// </summary>
    public class MailRuCloud : IDisposable
    {
        private readonly PathResolver _pathResolver;



        public CloudApi CloudApi { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="MailRuCloud" /> class.
        /// </summary>
        /// <param name="login">Login name as the email.</param>
        /// <param name="password">Password, associated with this email.</param>
        /// <param name="twoFaHandler"></param>
        public MailRuCloud(string login, string password, ITwoFaHandler twoFaHandler)
        {
            CloudApi = new CloudApi(login, password, twoFaHandler);
            _pathResolver = new PathResolver(CloudApi);
        }



        /// <summary>
        /// Get list of files and folders from account.
        /// </summary>
        /// <param name="path">Path in the cloud to return the list of the items.</param>
        /// <returns>List of the items.</returns>
        public virtual async Task<Entry> GetItems(string path)
        {
            string ulink = _pathResolver.AsRelationalWebLink(path);

            var data = await new FolderInfoRequest(CloudApi, string.IsNullOrEmpty(ulink) ? path : ulink, !string.IsNullOrEmpty(ulink)).MakeRequestAsync();

            if (!string.IsNullOrEmpty(ulink))
            {
                bool isFile = data.body.list.Any(it => it.weblink.TrimStart('/') == ulink.TrimStart('/'));

                string trimpath = path;
                if (isFile) trimpath = WebDavPath.Parent(path);

                foreach (var propse in data.body.list)
                {
                    propse.home = WebDavPath.Combine(trimpath, propse.name);
                }
                data.body.home = trimpath;
            }

            var entry = data.ToEntry();


            var flinks = _pathResolver.GetItems(entry.FullPath);
            if (flinks.Any())
            {
                foreach (var flink in flinks)
                {
                    string linkpath = WebDavPath.Combine(entry.FullPath, flink.Name);

                    if (!flink.IsFile)
                        entry.Folders.Add(new Folder(0, 0, 0, linkpath) { CreationTimeUtc = flink.CreationDate ?? DateTime.MinValue });
                    else
                    {
                        if (entry.Files.All(inf => inf.FullPath != linkpath))
                            entry.Files.Add(new File(linkpath, flink.Size, string.Empty));
                    }
                }
            }

            return entry;
        }


        /// <summary>
        /// Get disk usage for account.
        /// </summary>
        /// <returns>Returns Total/Free/Used size.</returns>
        public async Task<DiskUsage> GetDiskUsage()
        {
            var data = await new AccountInfoRequest(CloudApi).MakeRequestAsync();
            return data.ToDiskUsage();
        }

        /// <summary>
        /// Get list of files and folders from account.
        /// </summary>
        /// <param name="folder">Folder info.</param>
        /// <returns>List of the items.</returns>
        public async Task<Entry> GetItems(Folder folder)
        {
            return await GetItems(folder.FullPath);
        }


        /// <summary>
        /// Abort all prolonged async operations.
        /// </summary>
        public void AbortAllAsyncThreads()
        {
            CloudApi.CancelToken.Cancel(true);
        }

        /// <summary>
        /// Copying folder in another space on the server.
        /// </summary>
        /// <param name="folder">Folder info to copying.</param>
        /// <param name="destinationEntry">Destination entry on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(Folder folder, Entry destinationEntry)
        {
            return await Copy(folder, destinationEntry.FullPath);
        }

        /// <summary>
        /// Copying folder in another space on the server.
        /// </summary>
        /// <param name="folder">Folder info to copying.</param>
        /// <param name="destinationFolder">Destination folder on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(Folder folder, Folder destinationFolder)
        {
            return await Copy(folder, destinationFolder.FullPath);
        }

        /// <summary>
        /// Copying folder in another space on the server.
        /// </summary>
        /// <param name="folder">Folder info to copying.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(Folder folder, string destinationPath)
        {
            return !string.IsNullOrEmpty(await MoveOrCopy(folder.FullPath, destinationPath, false));
        }

        /// <summary>
        /// Copying file in another space on the server.
        /// </summary>
        /// <param name="file">File info to copying.</param>
        /// <param name="destinationEntry">Destination entry on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(File file, Entry destinationEntry)
        {
            return await Copy(file, destinationEntry.FullPath);
        }

        /// <summary>
        /// Copying file in another space on the server.
        /// </summary>
        /// <param name="file">File info to copying.</param>
        /// <param name="destinationFolder">Destination folder on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(File file, Folder destinationFolder)
        {
            return await Copy(file, destinationFolder.FullPath);
        }

        /// <summary>
        /// Copying file in another space on the server.
        /// </summary>
        /// <param name="file">File info to copying.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(File file, string destinationPath)
        {
            var result = !string.IsNullOrEmpty(await MoveOrCopy(file.FullPath, destinationPath, false));

            return result;
        }

        /// <summary>
        /// Rename folder on the server.
        /// </summary>
        /// <param name="folder">Source folder info.</param>
        /// <param name="newFileName">New folder name.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Rename(Folder folder, string newFileName)
        {
            return await Rename(folder.FullPath, newFileName);
        }

        /// <summary>
        /// Rename file on the server.
        /// </summary>
        /// <param name="file">Source file info.</param>
        /// <param name="newFileName">New file name.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Rename(File file, string newFileName)
        {
            var result = await Rename(file.FullPath, newFileName);  //var result = await Rename(file.FullPath, newFileName);
            if (file.Files.Count > 1)
            {
                foreach (var splitFile in file.Files)
                {
                    string newSplitName = newFileName + ".wdmrc" + splitFile.Extension; //TODO: refact with .wdmrc
                    await Rename(splitFile.FullPath, newSplitName);
                }
            }

            return result;
        }

        /// <summary>
        /// Move folder in another space on the server.
        /// </summary>
        /// <param name="folder">Folder info to moving.</param>
        /// <param name="destinationFolder">Destination folder on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Move(Folder folder, Folder destinationFolder)
        {
            return await Move(folder, destinationFolder.FullPath);
        }

        /// <summary>
        /// Move folder in another space on the server.
        /// </summary>
        /// <param name="folder">Folder info to moving.</param>
        /// <param name="destinationEntry">Destination entry on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Move(Folder folder, Entry destinationEntry)
        {
            return await Move(folder, destinationEntry.FullPath);
        }

        /// <summary>
        /// Move folder in another space on the server.
        /// </summary>
        /// <param name="folder">Folder info to move.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Move(Folder folder, string destinationPath)
        {
            return !string.IsNullOrEmpty(await MoveOrCopy(folder.FullPath, destinationPath, true));
        }

        /// <summary>
        /// Move file in another space on the server.
        /// </summary>
        /// <param name="file">File info to move.</param>
        /// <param name="destinationEntry">Destination entry on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Move(File file, Entry destinationEntry)
        {
            return await Move(file, destinationEntry.FullPath);
        }

        /// <summary>
        /// Move file in another space on the server.
        /// </summary>
        /// <param name="file">File info to move.</param>
        /// <param name="destinationFolder">Destination folder on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Move(File file, Folder destinationFolder)
        {
            return await Move(file, destinationFolder.FullPath);
        }

        /// <summary>
        /// Move file in another space on the server.
        /// </summary>
        /// <param name="file">File info to move.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Move(File file, string destinationPath)
        {
            var result = !string.IsNullOrEmpty(await MoveOrCopy(file.FullPath, destinationPath, true));
            if (file.Files.Count > 1)
            {
                foreach (var splitFile in file.Files)
                    await MoveOrCopy(splitFile.FullPath, destinationPath, true);
            }
            return result;
        }

        /// <summary>
        /// Create folder on the server.
        /// </summary>
        /// <param name="name">New path name.</param>
        /// <param name="createIn">Destination path.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> CreateFolder(string name, string createIn)
        {
            await new CreateFolderRequest(CloudApi, WebDavPath.Combine(createIn, name))
                .MakeRequestAsync();

            return true;
        }

        /// <summary>
        /// Remove the file on server.
        /// </summary>
        /// <param name="file">File info.</param>
        /// <returns>True or false operation result.</returns>
        public virtual async Task<bool> Remove(File file)
        {
            if (file.IsSplitted)
            {
                foreach (var fileFile in file.Files)
                {
                    await Remove(fileFile.FullPath);
                }
            }
            var result = await Remove(file.FullPath);

            return result;
        }

        /// <summary>
        /// Remove the folder on server.
        /// </summary>
        /// <param name="folder">Folder info.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Remove(Folder folder)
        {
            return await Remove(folder.FullPath);
        }

        public async Task<bool> CloneItem(string path, string url)
        {
            var data = await new CloneItemRequest(CloudApi, url, path)
                .MakeRequestAsync();
            return data.status == 200;
        }



        public async Task<Stream> GetFileDownloadStream(File file, long? start, long? end)
        {
            var filelst = file.Files.Count == 0 ? new List<File>{file} : file.Files;

            var task = Task.FromResult(new DownloadStream(filelst, CloudApi, start, end));
            Stream stream = await task;
            return stream;
        }


        public Stream GetFileUploadStream(string destinationPath, long size)
        {
            var stream = new SplittedUploadStream(destinationPath, CloudApi, size);

            // refresh linked folders
            stream.FileUploaded += files =>
            {
                var file = files?.FirstOrDefault();
                if (null == file) return;

                if (file.Path == "/" && file.Name == PathResolver.LinkContainerName)
                    _pathResolver.Load();
            };

            return stream;
        }

        /// <summary>
        /// Rename item on server.
        /// </summary>
        /// <param name="fullPath">Full path of the file or folder.</param>
        /// <param name="newName">New file or path name.</param>
        /// <returns>True or false result operation.</returns>
        private async Task<bool> Rename(string fullPath, string newName)
        {
            await new RenameRequest(CloudApi, fullPath, newName)
                .MakeRequestAsync();
            return true;
        }

        /// <summary>
        /// Move or copy item on server.
        /// </summary>
        /// <param name="sourceFullPath">Full path source or file name.</param>
        /// <param name="destinationPath">Destination path to cope or move.</param>
        /// <param name="move">Move or copy operation.</param>
        /// <returns>New created file name.</returns>
        public async Task<string> MoveOrCopy(string sourceFullPath, string destinationPath, bool move)
        {
            var data = await new MoveOrCopyRequest(CloudApi, sourceFullPath, destinationPath, move)
                .MakeRequestAsync();
            return data.ToString();
        }




        /// <summary>
        /// Remove file or folder.
        /// </summary>
        /// <param name="fullPath">Full file or folder name.</param>
        /// <returns>True or false result operation.</returns>
        private async Task<bool> Remove(string fullPath)
        {
            //TODO: refact
            string link = _pathResolver.AsRelationalWebLink(fullPath);

            if (!string.IsNullOrEmpty(link))
            {
                //if folder is linked - do not delete inner files/folders if client deleting recursively
                //just try to unlink folder
                _pathResolver.RemoveItem(fullPath);

                return true;
            }


            await new RemoveRequest(CloudApi, fullPath)
                .MakeRequestAsync();

            return true;
        }

        #region IDisposable Support
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CloudApi?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public void LinkItem(string url, string path, string name, bool isFile, long size, DateTime? creationDate)
        {
            _pathResolver.Add(url, path, name, isFile, size, creationDate);
        }
    }

    public delegate void FileUploadedDelegate(IEnumerable<File> file);
}

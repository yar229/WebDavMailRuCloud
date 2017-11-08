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
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Extensions;
using YaR.MailRuCloud.Api.Links;
using File = YaR.MailRuCloud.Api.Base.File;

namespace YaR.MailRuCloud.Api
{


    /// <summary>
    /// Cloud client.
    /// </summary>
    public class MailRuCloud : IDisposable
    {
        private readonly LinkManager _linkManager;



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
            _linkManager = new LinkManager(this);
        }

        public enum ItemType
        {
            File,
            Folder,
            Unknown
        }

        /// <summary>
        /// Get list of files and folders from account.
        /// </summary>
        /// <param name="path">Path in the cloud to return the list of the items.</param>
        /// <param  name="itemType">Unknown, File/Folder if you know for sure</param>
        /// <param name="resolveLinks">True if you know for sure that's not a linked item</param>
        /// <returns>List of the items.</returns>
        public virtual async Task<IEntry> GetItem(string path, ItemType itemType = ItemType.Unknown, bool resolveLinks = true)
        {
            //TODO: subject to refact!!!
            var ulink = resolveLinks ? await _linkManager.GetItemLink(path) : null;

            // bad link detected, just return stub
            // cause client cannot, for example, delete it if we return NotFound for this item
            if (ulink != null && ulink.IsBad)
                return ulink.ToBadEntry();


            var datares = await new FolderInfoRequest(CloudApi, null == ulink ? path : ulink.Href, ulink != null)
                .MakeRequestAsync().ConfigureAwait(false);

            //if (itemType == ItemType.Unknown)
            //    itemType = ulink != null
            //        ? ulink.IsFile ? ItemType.File : ItemType.Folder
            //        : datares.body.home == path
            //            ? ItemType.Folder
            //            : ItemType.File;

            if (itemType == ItemType.Unknown && ulink != null)
            {
                itemType = ulink.IsFile ? ItemType.File : ItemType.Folder;
            }

            if (itemType == ItemType.Unknown && null == ulink)
            {
                itemType = datares.body.home == path
                    ? ItemType.Folder
                    : ItemType.File;
            }

            var entry = itemType == ItemType.File
                ? (IEntry)datares.ToFile(
                    home:  itemType != ItemType.File ? path : WebDavPath.Parent(path),
                    ulink: ulink,
                    filename: ulink == null ? WebDavPath.Name(path) : ulink.OriginalName,
                    nameReplacement: WebDavPath.Name(path))
                : datares.ToFolder();

            // fill folder with links if any
            if (itemType == ItemType.Folder && entry is Folder folder)
            {
                var flinks = _linkManager.GetItems(folder.FullPath);
                if (flinks.Any())
                {
                    foreach (var flink in flinks)
                    {
                        string linkpath = WebDavPath.Combine(folder.FullPath, flink.Name);

                        if (!flink.IsFile)
                            folder.Folders.Add(new Folder(0, linkpath) { CreationTimeUtc = flink.CreationDate ?? DateTime.MinValue });
                        else
                        {
                            if (folder.Files.All(inf => inf.FullPath != linkpath))
                                folder.Files.Add(new File(linkpath, flink.Size));
                        }
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
            //return !string.IsNullOrEmpty(await MoveOrCopy(folder.FullPath, destinationPath, false));

            // if it linked - just clone
            var link = await _linkManager.GetItemLink(folder.FullPath, false);
            if (link != null)
            {
                var cloneres = await CloneItem(destinationPath, link.Href);
                if (cloneres.IsSuccess && WebDavPath.Name(cloneres.Path) != link.Name)
                {
                    var renRes = await Rename(cloneres.Path, link.Name);
                    return renRes;
                }
                return cloneres.IsSuccess;
            }

            var copyRes = await new CopyRequest(CloudApi, folder.FullPath, destinationPath)
                .MakeRequestAsync();
            if (copyRes.status != 200) return false;

            //clone all inner links
            var links = _linkManager.GetChilds(folder.FullPath, false);
            foreach (var linka in links)
            {
                try
                {
                    var linkdest = WebDavPath.ModifyParent(linka.MapTo, WebDavPath.Parent(folder.FullPath), destinationPath);
                    var cloneres = await CloneItem(linkdest, linka.Href);
                    if (cloneres.IsSuccess && WebDavPath.Name(cloneres.Path) != linka.Name)
                    {
                        var renRes = Rename(cloneres.Path, linka.Name);
                    }
                }
                catch (Exception e)
                {
                    
                }
            }

            return true;
        }

        public async Task<bool> Copy(string sourcePath, string destinationPath)
        {
            var entry = await GetItem(sourcePath);
            if (null == entry) return false;

            return await Copy(entry, destinationPath);
        }

        public async Task<bool> Copy(IEntry source, string destinationPath, string newname = null)
        {
            if (source is File file)
                return await Copy(file, destinationPath, string.IsNullOrEmpty(newname) ? file.Name : newname);

            if (source is Folder folder)
                return await Copy(folder, destinationPath);

            throw new ArgumentException("Source is not a file or folder", nameof(source));
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
        /// <param name="destinationFilePath">Destination path (with filename) on the server.</param>
        /// ///
        /// <param name="newname"></param>
        /// <param name="maxParallelRequests">Maximum parallel requests to server</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(File file, string destinationFilePath, string newname, int maxParallelRequests = 5)
        {
            string destPath = destinationFilePath;
            newname = string.IsNullOrEmpty(newname) ? file.Name : newname;
            bool doRename = file.Name != newname;

            var link = await _linkManager.GetItemLink(file.FullPath, false);
            if (link != null)
            {
                var cloneRes = await CloneItem(destPath, link.Href);
                if (doRename || WebDavPath.Name(cloneRes.Path) != newname)
                {
                    string newFullPath = WebDavPath.Combine(destPath, WebDavPath.Name(cloneRes.Path));
                    var renameRes = await Rename(newFullPath, link.Name);
                    if (!renameRes) return false;
                }
                return cloneRes.IsSuccess;
            }

            var qry = file.Files
                    .AsParallel()
                    .WithDegreeOfParallelism(Math.Min(maxParallelRequests, file.Files.Count))
                    .Select(async pfile =>
                    {
                        var copyRes = await new CopyRequest(CloudApi, pfile.FullPath, destPath, ConflictResolver.Rewrite)
                            .MakeRequestAsync();
                        if (copyRes.status != 200) return false;

                        if (doRename || WebDavPath.Name(copyRes.body) != newname)
                        {
                            string newFullPath = WebDavPath.Combine(destPath, WebDavPath.Name(copyRes.body));
                            var renameRes = await Rename(newFullPath, pfile.Name.Replace(file.Name, newname));
                            if (!renameRes) return false;
                        }
                        return true;
                    });

            bool res = (await Task.WhenAll(qry))
                .All(r => r);

            return res;
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
            if (file.Parts.Count > 1)
            {
                foreach (var splitFile in file.Parts)
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
            if (file.Parts.Count > 1)
            {
                foreach (var splitFile in file.Parts)
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
            return await CreateFolder(WebDavPath.Combine(createIn, name));
        }

        public async Task<bool> CreateFolder(string fullPath)
        {
            await new CreateFolderRequest(CloudApi, fullPath)
                .MakeRequestAsync();

            return true;
        }

        /// <summary>
        /// Remove item on server by path
        /// </summary>
        /// <param name="entry">File or folder</param>
        /// <returns>True or false operation result.</returns>
        public virtual async Task<bool> Remove(IEntry entry)
        {
            if (entry is File file)
                return await Remove(file);
            if (entry is Folder folder)
                return await Remove(folder);

            return false;
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
                foreach (var fileFile in file.Parts)
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


        public async Task<CloneResult> CloneItem(string path, string url)
        {
            var data = await new CloneItemRequest(CloudApi, url, path)
                .MakeRequestAsync();
            return new CloneResult
            {
                //TODO: move to dtoimport
                IsSuccess = data.status == 200,
                Path = data.body
            };
        }

        public class CloneResult
        {
            public bool IsSuccess { get; set; }
            public string Path { get; set; }
        }

        public async Task<Stream> GetFileDownloadStream(File file, long? start, long? end)
        {
            var filelst = file.Parts.Count == 0 ? new List<File>{file} : file.Parts;

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

                if (file.Path == "/" && file.Name == LinkManager.LinkContainerName)
                    _linkManager.Load();
            };

            return stream;
        }

        public T DownloadFileAsJson<T>(File file)
        {
            DownloadStream stream = new DownloadStream(file, CloudApi);

            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }

        public string DownloadFileAsString(File file)
        {
            using (var stream = new DownloadStream(file, CloudApi))
            using (var reader = new StreamReader(stream))
            {
                string res = reader.ReadToEnd();
                return res;
            }
        }

        /// <summary>
        /// Download content of file
        /// </summary>
        /// <param name="path"></param>
        /// <returns>file content or null if NotFound</returns>
        public async Task<string> DownloadFileAsString(string path)
        {
            try
            {
                var file = (File)await GetItem(path);
                return DownloadFileAsString(file);
            }
            catch (Exception e)
                when (  // let's check if there really no file or just other network error
                    (e is AggregateException && e.InnerException is WebException we && (we.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                    ||
                    (e is WebException wee && (wee.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                )
            {
                return null;
            }
        }

        public void UploadFile(string path, string content)
        {
            var data = Encoding.UTF8.GetBytes(content);

            using (var stream = GetFileUploadStream(path, data.Length))
            {
                stream.Write(data, 0, data.Length);
                //stream.Close();
            }
        }

        /// <summary>
        /// Rename item on server.
        /// </summary>
        /// <param name="fullPath">Full path of the file or folder.</param>
        /// <param name="newName">New file or path name.</param>
        /// <returns>True or false result operation.</returns>
        private async Task<bool> Rename(string fullPath, string newName)
        {
            var link = await _linkManager.GetItemLink(fullPath, false);

            //rename item
            if (link == null)
            {
                var data = await new RenameRequest(CloudApi, fullPath, newName)
                    .MakeRequestAsync();

                if (data.status == 200)
                {
                    _linkManager.ProcessRename(fullPath, newName);
                }

                return data.status == 200;
            }

            //rename link
            var res = _linkManager.RenameLink(link, newName);

            return res;
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
            //TODO: refact
            if (!move) //copy
            {
                var entry = await GetItem(sourceFullPath);
                if (entry is File file)
                {
                    await Copy(file, destinationPath);
                }
                else if (entry is Folder folder)
                {
                    await Copy(folder, destinationPath);
                }
            }

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
            //string link = _linkManager.AsRelationalWebLink(fullPath);
            var link = await _linkManager.GetItemLink(fullPath, false);

            if (link != null)
            {
                //if folder is linked - do not delete inner files/folders if client deleting recursively
                //just try to unlink folder
                _linkManager.RemoveItem(fullPath);

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
            _linkManager.Add(url, path, name, isFile, size, creationDate);
        }

        public void RemoveDeadLinks()
        {
            _linkManager.RemoveDeadLinks(true);
        }
    }

    public delegate void FileUploadedDelegate(IEnumerable<File> file);
}

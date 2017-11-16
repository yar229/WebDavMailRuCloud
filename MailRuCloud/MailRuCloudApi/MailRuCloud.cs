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
using System.Threading.Tasks;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Extensions;
using YaR.MailRuCloud.Api.Links;
using YaR.MailRuCloud.Api.XTSSharp;
using YaR.WebDavMailRu.CloudStore.XTSSharp;
using File = YaR.MailRuCloud.Api.Base.File;

namespace YaR.MailRuCloud.Api
{
    //TODO: not thread-safe, refact

    /// <summary>
    /// Cloud client.
    /// </summary>
    public class MailRuCloud : IDisposable
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Account));

        private readonly LinkManager _linkManager;



        public CloudApi CloudApi { get; }

        /// <summary>
        /// Caching files for multiple small reads
        /// </summary>
        private readonly StoreItemCache _itemCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailRuCloud" /> class.
        /// </summary>
        /// <param name="login">Login name as the email.</param>
        /// <param name="password">Password, associated with this email.</param>
        /// <param name="twoFaHandler"></param>
        public MailRuCloud(string login, string password, ITwoFaHandler twoFaHandler)
        {
            CloudApi = new CloudApi(login, password, twoFaHandler);

            //TODO: wow very dummy linking, refact cache realization globally!
            _itemCache = new StoreItemCache(TimeSpan.FromSeconds(60)) { CleanUpPeriod = TimeSpan.FromMinutes(5) };
            _linkManager = new LinkManager(this);
        }

        public enum ItemType
        {
            File,
            Folder,
            Unknown
        }

        ///// <summary>
        ///// Get list of files and folders from account.
        ///// </summary>
        ///// <param name="path">Path in the cloud to return the list of the items.</param>
        ///// <param  name="itemType">Unknown, File/Folder if you know for sure</param>
        ///// <param name="resolveLinks">True if you know for sure that's not a linked item</param>
        ///// <returns>List of the items.</returns>
        public virtual async Task<IEntry> GetItem(string path, ItemType itemType = ItemType.Unknown, bool resolveLinks = true)
        {
            var cached = _itemCache.Get(path);
            if (null != cached)
                return cached;

            //TODO: subject to refact!!!
            var ulink = resolveLinks ? await _linkManager.GetItemLink(path) : null;

            // bad link detected, just return stub
            // cause client cannot, for example, delete it if we return NotFound for this item
            if (ulink != null && ulink.IsBad)
            {
                var res = ulink.ToBadEntry();
                _itemCache.Add(res.FullPath, res);
                return res;
            }

            FolderInfoResult datares;
            try
            {
                datares = await new FolderInfoRequest(CloudApi, null == ulink ? path : ulink.Href, ulink != null)
                    .MakeRequestAsync().ConfigureAwait(false);
            }
            catch (WebException e) when ((e.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (itemType == ItemType.Unknown && ulink != null)
                itemType = ulink.ItemType;

            if (itemType == ItemType.Unknown && null == ulink)
                itemType = datares.body.home == path
                    ? ItemType.Folder
                    : ItemType.File;

            // TODO: cache (parent) folder for file 
            //if (itemType == ItemType.File)
            //{
            //    var cachefolder = datares.ToFolder(path, ulink);
            //    _itemCache.Add(cachefolder.FullPath, cachefolder);
            //    //_itemCache.Add(cachefolder.Files);
            //}

            var entry = itemType == ItemType.File
                ? (IEntry)datares.ToFile(
                    home: WebDavPath.Parent(path),
                    ulink: ulink,
                    filename: ulink == null ? WebDavPath.Name(path) : ulink.OriginalName,
                    nameReplacement: WebDavPath.Name(path))
                : datares.ToFolder(path, ulink);

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

            _itemCache.Add(entry.FullPath, entry);
            if (entry is Folder cfolder)
                _itemCache.Add(cfolder.Files);
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


        #region == Copy =============================================================================================================================

        /// <summary>
        /// Copy folder.
        /// </summary>
        /// <param name="folder">Source folder.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(Folder folder, string destinationPath)
        {
            destinationPath = WebDavPath.Clean(destinationPath);

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
            var links = _linkManager.GetChilds(folder.FullPath);
            foreach (var linka in links)
            {
                var linkdest = WebDavPath.ModifyParent(linka.MapPath, WebDavPath.Parent(folder.FullPath), destinationPath);
                var cloneres = await CloneItem(linkdest, linka.Href);
                if (cloneres.IsSuccess && WebDavPath.Name(cloneres.Path) != linka.Name)
                {
                    var renRes = await Rename(cloneres.Path, linka.Name);
                    if (!renRes)
                    {
                        _itemCache.Invalidate(destinationPath);
                        return false;
                    }
                }
            }

            _itemCache.Invalidate(destinationPath);
            return true;
        }

        /// <summary>
        /// Copy item.
        /// </summary>
        /// <param name="sourcePath">Source item.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(string sourcePath, string destinationPath)
        {
            var entry = await GetItem(sourcePath);
            if (null == entry) return false;

            return await Copy(entry, destinationPath);
        }

        /// <summary>
        /// Copy item.
        /// </summary>
        /// <param name="source">Source item.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <param name="newname">Rename target item.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(IEntry source, string destinationPath, string newname = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(destinationPath)) throw new ArgumentNullException(nameof(destinationPath));

            if (source is File file)
                return await Copy(file, destinationPath, string.IsNullOrEmpty(newname) ? file.Name : newname);

            if (source is Folder folder)
                return await Copy(folder, destinationPath);

            throw new ArgumentException("Source is not a file or folder", nameof(source));
        }

        /// <summary>
        /// Copy file to another path.
        /// </summary>
        /// <param name="file">Source file info.</param>
        /// <param name="destinationPath">Destination path.</param>
        /// <param name="newname">Rename target file.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Copy(File file, string destinationPath, string newname)
        {
            string destPath = destinationPath;
            newname = string.IsNullOrEmpty(newname) ? file.Name : newname;
            bool doRename = file.Name != newname;

            var link = await _linkManager.GetItemLink(file.FullPath, false);
            // копируем не саму ссылку, а её содержимое
            if (link != null)
            {
                var cloneRes = await CloneItem(destPath, link.Href);
                if (doRename || WebDavPath.Name(cloneRes.Path) != newname)
                {
                    string newFullPath = WebDavPath.Combine(destPath, WebDavPath.Name(cloneRes.Path));
                    var renameRes = await Rename(newFullPath, link.Name);
                    if (!renameRes) return false;
                }
                if (cloneRes.IsSuccess) _itemCache.Invalidate(destinationPath);
                return cloneRes.IsSuccess;
            }

            var qry = file.Files
                    .AsParallel()
                    .WithDegreeOfParallelism(Math.Min(MaxInnerParallelRequests, file.Files.Count))
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

            _itemCache.Invalidate(destinationPath);
            bool res = (await Task.WhenAll(qry))
                .All(r => r);

            return res;
        }

        #endregion == Copy ==========================================================================================================================

        #region == Rename ===========================================================================================================================

        /// <summary>
        /// Rename item on the server.
        /// </summary>
        /// <param name="source">Source item info.</param>
        /// <param name="newName">New item name.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Rename(IEntry source, string newName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(newName)) throw new ArgumentNullException(nameof(newName));

            if (source is File file)
                return await Rename(file, newName);

            if (source is Folder folder)
                return await Rename(folder, newName);

            throw new ArgumentException("Source item is not a file nor folder", nameof(source));
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
            var result = await Rename(file.FullPath, newFileName);
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
                    _itemCache.Invalidate(WebDavPath.Parent(fullPath));
                }

                return data.status == 200;
            }

            //rename link
            var res = _linkManager.RenameLink(link, newName);
            if (res) _itemCache.Invalidate(WebDavPath.Parent(fullPath));

            return res;
        }

        #endregion == Rename ========================================================================================================================

        #region == Move =============================================================================================================================

        /// <summary>
        /// Move item.
        /// </summary>
        /// <param name="source">source item info.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Move(IEntry source, string destinationPath)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(destinationPath)) throw new ArgumentNullException(nameof(destinationPath));

            if (source is File file)
                return await Move(file, destinationPath);

            if (source is Folder folder)
                return await Move(folder, destinationPath);

            throw new ArgumentException("Source item is not a file nor folder", nameof(source));
        }

        /// <summary>
        /// Move folder in another space on the server.
        /// </summary>
        /// <param name="folder">Folder info to move.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Move(Folder folder, string destinationPath)
        {
            var link = await _linkManager.GetItemLink(folder.FullPath, false);
            if (link != null)
            {
                var remapped = await _linkManager.RemapLink(link, destinationPath);
                if (remapped)
                    _itemCache.Invalidate(WebDavPath.Parent(folder.FullPath), destinationPath);
                return remapped;
            }

            var res = await MoveOrCopy(folder.FullPath, destinationPath, true);
            if (!string.IsNullOrEmpty(res)) return false;

            //clone all inner links
            var links = _linkManager.GetChilds(folder.FullPath).ToList();
            foreach (var linka in links)
            {
                // некоторые клиенты сначала делают структуру каталогов, а потом по одному переносят файлы
                // в таких условиях на каждый файл получится свой собственный линк, если делать правильно, т.е. в итоге расплодится миллин линков
                // поэтому делаем неправильно - копируем содержимое линков

                var linkdest = WebDavPath.ModifyParent(linka.MapPath, WebDavPath.Parent(folder.FullPath), destinationPath);
                var cloneres = await CloneItem(linkdest, linka.Href);
                if (cloneres.IsSuccess )
                {
                    _itemCache.Invalidate(destinationPath);
                    if (WebDavPath.Name(cloneres.Path) != linka.Name)
                    {
                        var renRes = await Rename(cloneres.Path, linka.Name);
                        if (!renRes) return false;
                    }
                }
                //await _linkManager.RemapLink(linka, linkdest, false);
            }
            if (links.Any()) _linkManager.Save();

            return true;
        }

        /// <summary>
        /// Move file in another space on the server.
        /// </summary>
        /// <param name="file">File info to move.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Move(File file, string destinationPath)
        {
            var link = await _linkManager.GetItemLink(file.FullPath, false);
            if (link != null)
            {
                var remapped = await _linkManager.RemapLink(link, destinationPath);
                if (remapped)
                    _itemCache.Invalidate(file.Path, destinationPath);
                return remapped;
            }

            var qry = file.Files
                .AsParallel()
                .WithDegreeOfParallelism(Math.Min(MaxInnerParallelRequests, file.Files.Count))
                .Select(async pfile => await MoveOrCopy(pfile.FullPath, destinationPath, true));

            bool res = (await Task.WhenAll(qry))
                .All(r => !string.IsNullOrEmpty(r));
            return res;
        }

        #endregion == Move ==========================================================================================================================

        #region == Remove ===========================================================================================================================

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
        /// Remove the folder on server.
        /// </summary>
        /// <param name="folder">Folder info.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Remove(Folder folder)
        {
            return await Remove(folder.FullPath);
        }

        /// <summary>
        /// Remove the file on server.
        /// </summary>
        /// <param name="file">File info.</param>
        /// <returns>True or false operation result.</returns>
        public virtual async Task<bool> Remove(File file)
        {
            var qry = file.Files
                .AsParallel()
                .WithDegreeOfParallelism(Math.Min(MaxInnerParallelRequests, file.Files.Count))
                .Select(async pfile =>
                {
                    var removed = await Remove(pfile.FullPath);
                    return removed;
                });

            bool res = (await Task.WhenAll(qry)).All(r => r);
            _itemCache.Invalidate(file.Path, file.FullPath);
            return res;
        }

        #endregion == Remove ========================================================================================================================

        public byte MaxInnerParallelRequests
        {
            get => _maxInnerParallelRequests;
            set => _maxInnerParallelRequests = value != 0 ? value : (byte)1;
        }
        private byte _maxInnerParallelRequests = 5;

        /// <summary>
        /// Create folder on the server.
        /// </summary>
        /// <param name="name">New path name.</param>
        /// <param name="basePath">Destination path.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> CreateFolder(string name, string basePath)
        {
            return await CreateFolder(WebDavPath.Combine(basePath, name));
        }

        public async Task<bool> CreateFolder(string fullPath)
        {
            var req = await new CreateFolderRequest(CloudApi, fullPath)
                .MakeRequestAsync();
            var res = req.ToPathResult();

            if (res.IsSuccess) _itemCache.Invalidate(WebDavPath.Parent(fullPath));
            return res.IsSuccess;
        }

        public async Task<PathResult> CloneItem(string path, string url)
        {
            var data = await new CloneItemRequest(CloudApi, url, path)
                .MakeRequestAsync();

            var res = data.ToPathResult();
            if (res.IsSuccess) _itemCache.Invalidate(path);
            return res;
        }

        //TODO : move upper
        public class PathResult
        {
            public bool IsSuccess { get; set; }
            public string Path { get; set; }
        }

        public async Task<Stream> GetFileDownloadStream(File file, long? start, long? end)
        {
            var task = Task.FromResult(new DownloadStreamFabric(CloudApi).Create(file, start, end))
                .ConfigureAwait(false);
            Stream stream = await task;
            return stream;
        }


        public Stream GetFileUploadStream(string destinationPath, long size)
        {
            //var stream = new SplittedUploadStream(destinationPath, CloudApi, size);

            //// refresh linked folders
            //stream.FileUploaded += OnFileUploaded;

            //return stream;
            //=============================================================================================================

            var key1 = new byte[32];
            var key2 = new byte[32];
            Array.Copy(Encoding.ASCII.GetBytes("01234567890123456789012345678900zzzzzzzzzzzzzzzzzzzzzz"), key1, 32);
            Array.Copy(Encoding.ASCII.GetBytes("01234567890123456789012345678900zzzzzzzzzzzzzzzzzzzzzz"), key2, 32);
            var xts = XtsAes256.Create(key1, key2);

            //using (var streamread = System.IO.File.Open(@"d:\4\original.pdf", FileMode.Open, FileAccess.Read, FileShare.Read))
            //using (var streamwrite = System.IO.File.OpenWrite(@"d:\4\local_encoded_xtsw.pdf"))
            using (var streamread = System.IO.File.Open(@"d:\4\1.txt", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var streamwrite = System.IO.File.OpenWrite(@"d:\4\1_local_encoded_xtsw.pdf"))
            {
                using (var xtswritestream = new XTSWriteOnlyStream(streamwrite, xts, 512))
                {
                    streamread.CopyTo(xtswritestream);
                }
            }

            using (var streamread = System.IO.File.Open(@"d:\4\1_local_encoded_xtsw.pdf", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var streamwrite = System.IO.File.OpenWrite(@"d:\4\1_local_decoded_xtsw.pdf"))
            {
                using (var xtsreadstream = new XtsStream(streamread, xts, 512))
                {
                    xtsreadstream.CopyTo(streamwrite);
                }
            }

            //================================================================================================================================

            long delta = XTSWriteOnlyStream.BlockSize - size % XTSWriteOnlyStream.BlockSize;
            destinationPath += $".c{delta:x}.wdmrc";

            size = size % XTSWriteOnlyStream.BlockSize == 0
                ? size
                : (size / XTSWriteOnlyStream.BlockSize + 1) * XTSWriteOnlyStream.BlockSize;

            

            var ustream = new SplittedUploadStream(destinationPath, CloudApi, size, false);
            var encustream = new XTSWriteOnlyStream(ustream, xts, XTSWriteOnlyStream.DefaultSectorSize);

            // refresh linked folders
            ustream.FileUploaded += OnFileUploaded;

            return encustream;


            //////================================================================================================================================
            //var xtsa = XtsAes256.Create(key1, key2);
            //using (FileStream sourceStream = System.IO.File.Open(@"d:\4\original.pdf", FileMode.Open, FileAccess.Read, FileShare.Read))
            //{
            //    sourceStream.Seek(0, SeekOrigin.End);

            //    FileStream targetStream = System.IO.File.Open(@"d:\4\local_encoded_xts.pdf", FileMode.OpenOrCreate);
            //    var encstream = new XtsStream(targetStream, xtsa, 512);
            //    sourceStream.Seek(0, SeekOrigin.Begin);
            //    sourceStream.CopyTo(encstream);
            //    encstream.Flush();
            //    encstream.Close();
            //    targetStream.Flush();
            //    targetStream.Close();
            //}

            //return stream;
        }

        public event FileUploadedDelegate FileUploaded;

        private void OnFileUploaded(IEnumerable<File> files)
        {
            var lst = files.ToList();
            _itemCache.Invalidate(lst);
            _itemCache.Invalidate(lst.Select(file => file.Path).Distinct());
            FileUploaded?.Invoke(lst);
        }

        public T DownloadFileAsJson<T>(File file)
        {
            using (var stream = new DownloadStream(file, CloudApi))
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

            ////TODO: refact, bad stream realization
            //StreamReader reader = null;
            //try
            //{
            //    var stream = new DownloadStream(file, CloudApi);
            //    reader = new StreamReader(stream);
            //    string res = reader.ReadToEnd();
            //    return res;
            //}
            //finally
            //{
            //    reader?.Close();
            //}
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
            _itemCache.Invalidate(path, WebDavPath.Parent(path));
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
            if (!move)
            {
                var entry = await GetItem(sourceFullPath);
                await Copy(entry, destinationPath);
            }

            var data = await new MoveOrCopyRequest(CloudApi, sourceFullPath, destinationPath, move)
                .MakeRequestAsync();

            if (data.status == 200) _itemCache.Invalidate(WebDavPath.Parent(sourceFullPath), destinationPath);
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
            var link = await _linkManager.GetItemLink(fullPath, false);

            if (link != null)
            {
                //if folder is linked - do not delete inner files/folders if client deleting recursively
                //just try to unlink folder
                _linkManager.RemoveLink(fullPath);

                _itemCache.Invalidate(WebDavPath.Parent(fullPath));
                return true;
            }

            var res = await new RemoveRequest(CloudApi, fullPath)
                .MakeRequestAsync();
            if (res.status == 200)
            {
                //remove inner links
                var innerLinks = _linkManager.GetChilds(fullPath);
                _linkManager.RemoveLinks(innerLinks);

                _itemCache.Invalidate(WebDavPath.Parent(fullPath));
            }
            return res.status == 200;
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

        public async Task<bool> LinkItem(string url, string path, string name, bool isFile, long size, DateTime? creationDate)
        {
            var res = await _linkManager.Add(url, path, name, isFile, size, creationDate);
            if (res)
            {
                _linkManager.Save();
                _itemCache.Invalidate(path);
            }
            return res;
        }

        public async void RemoveDeadLinks()
        {
            var count = await _linkManager.RemoveDeadLinks(true);
            if (count > 0) _itemCache.Invalidate();
        }

        public async Task<StatusResult> AddFile(string hash, string fullFilePath, long size, ConflictResolver? conflict = null)
        {
            var res = await new CreateFileRequest(CloudApi, fullFilePath, hash, size, conflict)
                .MakeRequestAsync();

            return res;
        }

        public async Task<StatusResult> AddFileInCloud(File fileInfo, ConflictResolver? conflict = null)
        {
            var res = await AddFile(fileInfo.Hash, fileInfo.FullPath, fileInfo.Size, conflict);

            return res;
        }
    }

    public delegate void FileUploadedDelegate(IEnumerable<File> file);
}

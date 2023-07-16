using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YaR.Clouds.Base;
using YaR.Clouds.Base.Repos;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Common;
using YaR.Clouds.Extensions;
using YaR.Clouds.Links;
using YaR.Clouds.Streams;
using File = YaR.Clouds.Base.File;

namespace YaR.Clouds
{
    //TODO: not thread-safe, refact

    /// <summary>
    /// Cloud client.
    /// </summary>
    public class Cloud : IDisposable
    {
        //private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Account));

        public LinkManager LinkManager { get; }

        /// <summary>
        /// Async tasks cancelation token.
        /// </summary>
        public readonly CancellationTokenSource CancelToken = new();

        public CloudSettings Settings { get; }

        /// <summary>
        /// Gets or sets account to connect with cloud.
        /// </summary>
        /// <value>Account info.</value>
        public Account Account { get; }


        /// <summary>
        /// Caching files for multiple small reads
        /// </summary>
        private readonly ItemCache<string, IEntry> _itemCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cloud" /> class.
        /// </summary>
        public Cloud(CloudSettings settings, Credentials credentials)
        {
            Settings = settings;
            Account = new Account(settings, credentials);
            if (!Account.Login())
            {
                throw new AuthenticationException("Auth token has't been retrieved.");
            }

            //TODO: wow very dummy linking, refact cache realization globally!
            _itemCache = new ItemCache<string, IEntry>(TimeSpan.FromSeconds(settings.CacheListingSec)) { CleanUpPeriod = TimeSpan.FromMinutes(5) };
            LinkManager = settings.DisableLinkManager ? null : new LinkManager(this);
        }

        public enum ItemType
        {
            File,
            Folder,
            Unknown
        }

        public virtual async Task<IEntry> GetPublicItemAsync(Uri url, ItemType itemType = ItemType.Unknown)
        {
            var entry = await Account.RequestRepo.FolderInfo(RemotePath.Get(new Link(url)));

            return entry;
        }

        ///// <summary>
        ///// Get list of files and folders from account.
        ///// </summary>
        ///// <param name="path">Path in the cloud to return the list of the items.</param>
        ///// <param  name="itemType">Unknown, File/Folder if you know for sure</param>
        ///// <param name="resolveLinks">True if you know for sure that's not a linked item</param>
        ///// <returns>List of the items.</returns>
        public virtual async Task<IEntry> GetItemAsync(string path, ItemType itemType = ItemType.Unknown, bool resolveLinks = true)
        {
            //TODO: вообще, всё плохо стало, всё запуталось, всё надо переписать
            var uriMatch = Regex.Match(path, @"\A/(?<uri>https://cloud\.mail\.\w+/public/\S+/\S+(/.*)?)\Z");
            if (uriMatch.Success)
                return await GetPublicItemAsync(new Uri(uriMatch.Groups["uri"].Value, UriKind.Absolute), itemType);

            if (Account.IsAnonymous)
                return null;

            path = WebDavPath.Clean(path);

            if (Settings.CacheListingSec > 0)
            {
                var cached = CacheGetEntry(path);
                if (cached != null)
                    return cached;
            }

            //TODO: subject to refact!!!
            Link ulink = resolveLinks && LinkManager != null ? await LinkManager.GetItemLink(path) : null;

            // bad link detected, just return stub
            // cause client cannot, for example, delete it if we return NotFound for this item
            if (ulink is { IsBad: true })
            {
                var res = ulink.ToBadEntry();
                _itemCache.Add(res.FullPath, res);
                return res;
            }

            if (itemType == ItemType.Unknown && ulink != null)
                itemType = ulink.ItemType;

            // TODO: cache (parent) folder for file 
            //if (itemType == ItemType.File)
            //{
            //    var cachefolder = datares.ToFolder(path, ulink);
            //    _itemCache.Add(cachefolder.FullPath, cachefolder);
            //    //_itemCache.Add(cachefolder.Files);
            //}

            var rp = null == ulink ? RemotePath.Get(path) : RemotePath.Get(ulink);
            var entry = await Account.RequestRepo.FolderInfo(rp, depth:Settings.ListDepth);
            if (null == entry)
                return null;

            if (itemType == ItemType.Unknown)
                itemType = entry is Folder 
                    ? ItemType.Folder 
                    : ItemType.File;

            if (itemType == ItemType.Folder && entry is Folder folder) // fill folder with links if any
                FillWithULinks(folder);

            if (Settings.CacheListingSec > 0)
                CacheAddEntry(entry);

            return entry;
        }

        private void FillWithULinks(Folder folder)
        {
            if (!folder.IsChildsLoaded) return;

            var flinks = LinkManager?.GetItems(folder.FullPath);
            if (flinks != null && flinks.Any())
            {
                foreach (var flink in flinks)
                {
                    string linkpath = WebDavPath.Combine(folder.FullPath, flink.Name);

                    if (!flink.IsFile)
                        folder.Folders.Add(new Folder(0, linkpath) { CreationTimeUtc = flink.CreationDate ?? DateTime.MinValue });
                    else
                    {
                        if (folder.Files.Any(inf => inf.FullPath == linkpath))
                            continue;

                        var newfile = new File(linkpath, flink.Size, new PublicLinkInfo(flink.Href));
                        if (flink.CreationDate != null)
                            newfile.LastWriteTimeUtc = flink.CreationDate.Value;
                        folder.Files.Add(newfile);
                    }
                }
            }

            foreach (var childFolder in folder.Folders)
                FillWithULinks(childFolder);
        }


        private void CacheAddEntry(IEntry entry)
        {
            switch (entry)
            {
                case File cfile:
                    _itemCache.Add(cfile.FullPath, cfile);
                    break;
                case Folder { IsChildsLoaded: true } cfolder:
                {
                    _itemCache.Add(cfolder.FullPath, cfolder);
                    _itemCache.Add(cfolder.Files.Select(f => new KeyValuePair<string, IEntry>(f.FullPath, f)));

                    foreach (var childFolder in cfolder.Entries)
                        CacheAddEntry(childFolder);
                    break;
                }
            }
        }

        private IEntry CacheGetEntry(string path)
        {
            var cached = _itemCache.Get(path);
            return cached;
        }

        public virtual IEntry GetItem(string path, ItemType itemType = ItemType.Unknown, bool resolveLinks = true)
        {
            return GetItemAsync(path, itemType, resolveLinks).Result;
        }

        public IEnumerable<File> IsFileExists(string filename, IList<string> folderPaths)
        {
            if (folderPaths == null)
            {
#if DEBUG
                // This case should not be happened, let's find out why we are here.
                System.Diagnostics.Debugger.Break();
#endif
                return Enumerable.Empty<File>();
            }

            var folder = folderPaths
                .AsParallel()
                .WithDegreeOfParallelism(Math.Min(MaxInnerParallelRequests, folderPaths.Count))
                .Select(async path => (Folder)await GetItemAsync(path, ItemType.Folder, false));

            if (folder == null)
            {
#if DEBUG
                // This case should not be happened, let's find out why we are here.
                System.Diagnostics.Debugger.Break();
#endif
                return Enumerable.Empty<File>();
            }

            var files = folder
                .SelectMany(fld => (fld.Result?.Files ?? new List<File>()).Where(file => WebDavPath.PathEquals(file.Name, filename)));

            return files;
        }

        #region == Publish ==========================================================================================================================

        private async Task<bool> Unpublish(Uri publicLink, string fullPath)
        {
            //var res = (await new UnpublishRequest(CloudApi, publicLink).MakeRequestAsync())
            var res = (await  Account.RequestRepo.Unpublish(publicLink, fullPath))
                .ThrowIf(r => !r.IsSuccess, _ => new Exception($"Unpublish error, link = {publicLink}"));

            return res.IsSuccess;
        }

        public async Task  Unpublish(File file)
        {
            foreach (var innerFile in file.Files)
            {
                await Unpublish(innerFile.GetPublicLinks(this).First().Uri, innerFile.FullPath);
                innerFile.PublicLinks.Clear();
            }
            _itemCache.Invalidate(file.FullPath, file.Path);
        }


        private async Task<Uri> Publish(string fullPath)
        {
            var res = (await Account.RequestRepo.Publish(fullPath))
                .ThrowIf(r => !r.IsSuccess, _ => new Exception($"Publish error, path = {fullPath}"));
                
            var uri = new Uri(res.Url, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
                uri = new Uri($"{Account.RequestRepo.PublicBaseUrlDefault.TrimEnd('/')}/{res.Url.TrimStart('/')}", UriKind.Absolute);

            return uri;
        }

        public async Task<PublishInfo> Publish(File file, bool makeShareFile = true, 
            bool generateDirectVideoLink = false, bool makeM3UFile = false, SharedVideoResolution videoResolution = SharedVideoResolution.All)
        {
            if (file.Files.Count > 1 && (generateDirectVideoLink || makeM3UFile))
                throw new ArgumentException($"Cannot generate direct video link for splitted file {file.FullPath}");

            foreach (var innerFile in file.Files)
            {
                var url = await Publish(innerFile.FullPath);
                innerFile.PublicLinks.Clear();
                innerFile.PublicLinks.Add(new PublicLinkInfo(url));
            }
            var info = file.ToPublishInfo(this, generateDirectVideoLink, videoResolution);

            if (makeShareFile)
            {
                string path = $"{file.FullPath}{PublishInfo.SharedFilePostfix}";
                UploadFileJson(path, info)
                    .ThrowIf(r => !r, _ => new Exception($"Cannot upload JSON file, path = {path}"));
            }


            if (makeM3UFile)
            {
                string path = $"{file.FullPath}{PublishInfo.PlaylistFilePostfix}";
                var content = new StringBuilder();
                {
                    content.Append("#EXTM3U\r\n");
                    foreach (var item in info.Items)
                    {
                        content.Append($"#EXTINF:-1,{WebDavPath.Name(item.Path)}\r\n");
                        content.Append($"{item.PlaylistUrl}\r\n");
                    }
                }
                UploadFile(path, content.ToString())
                    .ThrowIf(r => !r, _ => new Exception($"Cannot upload JSON file, path = {path}"));
            }

            return info;
        }

        public async Task<PublishInfo> Publish(Folder folder, bool makeShareFile = true)
        {
            var url = await Publish(folder.FullPath);
            folder.PublicLinks.Clear();
            folder.PublicLinks.Add(new PublicLinkInfo(url));
            var info = folder.ToPublishInfo();

            if (!makeShareFile) 
                return info;

            string path = WebDavPath.Combine(folder.FullPath, PublishInfo.SharedFilePostfix);
            UploadFileJson(path, info)
                .ThrowIf(r => !r, _ => new Exception($"Cannot upload JSON file, path = {path}"));

            return info;
        }

        public async Task<PublishInfo> Publish(IEntry entry, bool makeShareFile = true, 
            bool generateDirectVideoLink = false, bool makeM3UFile = false, SharedVideoResolution videoResolution = SharedVideoResolution.All)
        {
            return entry switch
            {
                null => throw new ArgumentNullException(nameof(entry)),
                File file => await Publish(file, makeShareFile, generateDirectVideoLink, makeM3UFile, videoResolution),
                Folder folder => await Publish(folder, makeShareFile),
                _ => throw new Exception($"Unknown entry type, type = {entry.GetType()},path = {entry.FullPath}")
            };
        }
        #endregion == Publish =======================================================================================================================

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
            var link = await LinkManager?.GetItemLink(folder.FullPath, false);
            if (link != null)
            {
                var cloneres = await CloneItem(destinationPath, link.Href.OriginalString);
                if (!cloneres.IsSuccess || WebDavPath.Name(cloneres.Path) == link.Name) 
                    return cloneres.IsSuccess;
                var renRes = await Rename(cloneres.Path, link.Name);
                return renRes;
            }

            //var copyRes = await new CopyRequest(CloudApi, folder.FullPath, destinationPath).MakeRequestAsync();
            var copyRes = await Account.RequestRepo.Copy(folder.FullPath, destinationPath);
            if (!copyRes.IsSuccess) return false;

            //clone all inner links
            var links = LinkManager?.GetChilds(folder.FullPath);
            if (links != null)
            {
                foreach (var linka in links)
                {
                    var linkdest = WebDavPath.ModifyParent(linka.MapPath, WebDavPath.Parent(folder.FullPath), destinationPath);
                    var cloneres = await CloneItem(linkdest, linka.Href.OriginalString);
                    if (!cloneres.IsSuccess || WebDavPath.Name(cloneres.Path) == linka.Name)
                        continue;

                    if (await Rename(cloneres.Path, linka.Name))
                        continue;

                    _itemCache.Invalidate(destinationPath);
                    return false;
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
            var entry = await GetItemAsync(sourcePath);
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

            return source switch
            {
                File file => await Copy(file, destinationPath, string.IsNullOrEmpty(newname) ? file.Name : newname),
                Folder folder => await Copy(folder, destinationPath),
                _ => throw new ArgumentException("Source is not a file or folder", nameof(source))
            };
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

            var link = await LinkManager?.GetItemLink(file.FullPath, false);
            // копируем не саму ссылку, а её содержимое
            if (link != null)
            {
                var cloneRes = await CloneItem(destPath, link.Href.OriginalString);
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
                        //var copyRes = await new CopyRequest(CloudApi, pfile.FullPath, destPath, ConflictResolver.Rewrite).MakeRequestAsync();
                        var copyRes = await Account.RequestRepo.Copy(pfile.FullPath, destPath, ConflictResolver.Rewrite);
                        if (!copyRes.IsSuccess) return false;

                        if (!doRename && WebDavPath.Name(copyRes.NewName) == newname) 
                            return true;

                        string newFullPath = WebDavPath.Combine(destPath, WebDavPath.Name(copyRes.NewName));
                        return await Rename(newFullPath, pfile.Name.Replace(file.Name, newname));
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

            return source switch
            {
                File file => await Rename(file, newName),
                Folder folder => await Rename(folder, newName),
                _ => throw new ArgumentException("Source item is not a file nor folder", nameof(source))
            };
        }

        /// <summary>
        /// Rename folder on the server.
        /// </summary>
        /// <param name="folder">Source folder info.</param>
        /// <param name="newFileName">New folder name.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Rename(Folder folder, string newFileName) 
            => await Rename(folder.FullPath, newFileName);

        /// <summary>
        /// Rename file on the server.
        /// </summary>
        /// <param name="file">Source file info.</param>
        /// <param name="newFileName">New file name.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> Rename(File file, string newFileName)
        {
            var result = await Rename(file.FullPath, newFileName).ConfigureAwait(false);

            if (file.Files.Count <= 1) 
                return result;

            foreach (var splitFile in file.Parts)
            {
                string newSplitName = newFileName + splitFile.ServiceInfo.ToString(false);
                await Rename(splitFile.FullPath, newSplitName).ConfigureAwait(false);
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
            var link = await LinkManager?.GetItemLink(fullPath, false);

            //rename item
            if (link == null)
            {
                var data = await Account.RequestRepo.Rename(fullPath, newName);

                if (!data.IsSuccess) 
                    return data.IsSuccess;

                LinkManager?.ProcessRename(fullPath, newName);
                _itemCache.Invalidate(fullPath, WebDavPath.Parent(fullPath));

                return data.IsSuccess;
            }

            //rename link
            if (LinkManager != null)
            {
                bool res = LinkManager.RenameLink(link, newName);
                if (res)
                    _itemCache.Invalidate(fullPath, WebDavPath.Parent(fullPath));
                return res;
            }
            return false;
        }

        #endregion == Rename ========================================================================================================================

        #region == Move =============================================================================================================================

        /// <summary>
        /// Move item.
        /// </summary>
        /// <param name="source">source item info.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> MoveAsync(IEntry source, string destinationPath)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(destinationPath)) throw new ArgumentNullException(nameof(destinationPath));

            return source switch
            {
                File file => await MoveAsync(file, destinationPath),
                Folder folder => await MoveAsync(folder, destinationPath),
                _ => throw new ArgumentException("Source item is not a file nor folder", nameof(source))
            };
        }

        public async Task<bool> MoveAsync(string sourcePath, string destinationPath)
        {
            var entry = await GetItemAsync(sourcePath);
            if (null == entry)
                return false;

            return await MoveAsync(entry, destinationPath);
        }

        public bool Move(string sourcePath, string destinationPath)
        {
            return MoveAsync(sourcePath, destinationPath).Result;
        }


        /// <summary>
        /// Move folder in another space on the server.
        /// </summary>
        /// <param name="folder">Folder info to move.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> MoveAsync(Folder folder, string destinationPath)
        {
            var link = await LinkManager?.GetItemLink(folder.FullPath, false);
            if (link != null)
            {
                var remapped = await LinkManager.RemapLink(link, destinationPath);
                if (remapped)
                    _itemCache.Invalidate(WebDavPath.Parent(folder.FullPath), destinationPath);
                return remapped;
            }

            var res = await Account.RequestRepo.Move(folder.FullPath, destinationPath);
            _itemCache.Invalidate(WebDavPath.Parent(folder.FullPath), destinationPath);
            if (!res.IsSuccess) return false;

            //clone all inner links
            var links = LinkManager?.GetChilds(folder.FullPath).ToList();
            foreach (var linka in links)
            {
                // некоторые клиенты сначала делают структуру каталогов, а потом по одному переносят файлы
                // в таких условиях на каждый файл получится свой собственный линк, если делать правильно, т.е. в итоге расплодится миллион линков
                // поэтому делаем неправильно - копируем содержимое линков

                var linkdest = WebDavPath.ModifyParent(linka.MapPath, WebDavPath.Parent(folder.FullPath), destinationPath);
                var cloneres = await CloneItem(linkdest, linka.Href.OriginalString);
                if (!cloneres.IsSuccess) 
                    continue;

                _itemCache.Invalidate(destinationPath);
                if (WebDavPath.Name(cloneres.Path) != linka.Name)
                {
                    var renRes = await Rename(cloneres.Path, linka.Name);
                    if (!renRes) return false;
                }
            }
            if (links.Any())
                LinkManager?.Save();

            return true;
        }

        /// <summary>
        /// Move file in another space on the server.
        /// </summary>
        /// <param name="file">File info to move.</param>
        /// <param name="destinationPath">Destination path on the server.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> MoveAsync(File file, string destinationPath)
        {
            var link = await LinkManager?.GetItemLink(file.FullPath, false);
            if (link != null)
            {
                var remapped = await LinkManager.RemapLink(link, destinationPath);
                if (remapped)
                    _itemCache.Invalidate(file.Path, destinationPath);
                return remapped;
            }

            var qry = file.Files
                .AsParallel()
                .WithDegreeOfParallelism(Math.Min(MaxInnerParallelRequests, file.Files.Count))
                .Select(async pfile =>
                {
                    var moveres =  await Account.RequestRepo.Move(pfile.FullPath, destinationPath);
                    _itemCache.Forget(file.Path, pfile.FullPath);
                    return moveres;
                });

            _itemCache.Forget(file.Path, file.FullPath);
            _itemCache.Invalidate(destinationPath);

            bool res = (await Task.WhenAll(qry))
                .All(r => r.IsSuccess);

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
            return entry switch
            {
                File file => await Remove(file),
                Folder folder => await Remove(folder),
                _ => false
            };
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
        /// <param name="removeShareDescription">Also remove share description file (.share.wdmrc)</param>
        /// <returns>True or false operation result.</returns>
        public virtual async Task<bool> Remove(File file, bool removeShareDescription = true) //, bool doInvalidateCache = true)
        {
            // remove all parts if file splitted
            var qry = file.Files
                .AsParallel()
                .WithDegreeOfParallelism(Math.Min(MaxInnerParallelRequests, file.Files.Count))
                .Select(async pfile =>
                {
                    var removed = await Remove(pfile.FullPath);
                    return removed;
                });
            bool res = (await Task.WhenAll(qry)).All(r => r);

            if (res)
            {
                if (file.Name.EndsWith(PublishInfo.SharedFilePostfix))  //unshare master item
                {
                    var mpath = WebDavPath.Clean(file.FullPath.Substring(0, file.FullPath.Length - PublishInfo.SharedFilePostfix.Length));
                    var item = await GetItemAsync(mpath);


                    switch (item)
                    {
                        case Folder folder:
                            await Unpublish(folder.GetPublicLinks(this).First().Uri, folder.FullPath);
                            break;
                        case File ifile:
                            await Unpublish(ifile);
                            break;
                    }
                }
                else
                {
                    if (removeShareDescription) //remove share description (.wdmrc.share)
                    {
                        if (await GetItemAsync(file.FullPath + PublishInfo.SharedFilePostfix) is File sharefile)
                            await Remove(sharefile, false);
                    }
                }

            }

            return res;
        }

        /// <summary>
        /// Remove file or folder.
        /// </summary>
        /// <param name="fullPath">Full file or folder name.</param>
        /// <returns>True or false result operation.</returns>
        private async Task<bool> Remove(string fullPath)
        {
            //TODO: refact
            var link = await LinkManager?.GetItemLink(fullPath, false);

            if (link != null)
            {
                //if folder is linked - do not delete inner files/folders if client deleting recursively
                //just try to unlink folder
                LinkManager.RemoveLink(fullPath);

                _itemCache.Invalidate(WebDavPath.Parent(fullPath));
                return true;
            }

            var res = await Account.RequestRepo.Remove(fullPath);
            if (!res.IsSuccess) 
                return res.IsSuccess;

            //remove inner links
            if (LinkManager != null)
            {
                var innerLinks = LinkManager.GetChilds(fullPath);
                LinkManager.RemoveLinks(innerLinks);
            }

            _itemCache.Forget(WebDavPath.Parent(fullPath), fullPath); //_itemCache.Invalidate(WebDavPath.Parent(fullPath));
            return res.IsSuccess;
        }

        #endregion == Remove ========================================================================================================================

        public IEnumerable<PublicLinkInfo> GetSharedLinks(string fullPath)
        {
            return Account.RequestRepo.GetShareLinks(fullPath);
        }

        /// <summary>
        /// Get disk usage for account.
        /// </summary>
        /// <returns>Returns Total/Free/Used size.</returns>
        public async Task<DiskUsage> GetDiskUsageAsync()
        {
            var data = await Account.RequestRepo.AccountInfo();
            return data.DiskUsage;
        }
        public DiskUsage GetDiskUsage()
        {
            return GetDiskUsageAsync().Result;
        }


        /// <summary>
        /// Abort all prolonged async operations.
        /// </summary>
        public void AbortAllAsyncThreads()
        {
            CancelToken.Cancel(false);
        }

        public byte MaxInnerParallelRequests
        {
            get => _maxInnerParallelRequests;
            set => _maxInnerParallelRequests = value != 0 ? value : (byte)1;
        }

        public IRequestRepo Repo => Account.RequestRepo;

        private byte _maxInnerParallelRequests = 5;

        /// <summary>
        /// Create folder on the server.
        /// </summary>
        /// <param name="name">New path name.</param>
        /// <param name="basePath">Destination path.</param>
        /// <returns>True or false operation result.</returns>
        public async Task<bool> CreateFolderAsync(string name, string basePath)
        {
            return await CreateFolderAsync(WebDavPath.Combine(basePath, name));
        }

        public bool CreateFolder(string name, string basePath)
        {
            return CreateFolderAsync(name, basePath).Result;
        }

        public async Task<bool> CreateFolderAsync(string fullPath)
        {
            var res = await Account.RequestRepo.CreateFolder(fullPath);

            if (res.IsSuccess) _itemCache.Invalidate(WebDavPath.Parent(fullPath));
            return res.IsSuccess;
        }

        //public bool CreateFolder(string fullPath)
        //{
        //    return CreateFolderAsync(fullPath).Result;
        //}


        public async Task<CloneItemResult> CloneItem(string path, string url)
        {
            var res = await Account.RequestRepo.CloneItem(url, path);

            if (res.IsSuccess) _itemCache.Invalidate(path);
            return res;
        }

        public async Task<Stream> GetFileDownloadStream(File file, long? start, long? end)
        {
            var task = Task.FromResult(new DownloadStreamFabric(this).Create(file, start, end))
                .ConfigureAwait(false);
            Stream stream = await task;
            return stream;
        }


        public async Task<Stream> GetFileUploadStream(string fullFilePath, long size, Action fileStreamSent, Action serverFileProcessed, bool discardEncryption = false)
        {
            var file = new File(fullFilePath, size);


            var f = new UploadStreamFabric(this)
            {
                FileStreamSent = fileStreamSent, 
                ServerFileProcessed = serverFileProcessed
            };

            var task = await Task.FromResult(f.Create(file, OnFileUploaded, discardEncryption))
                .ConfigureAwait(false);
            var stream = await task;

            return stream;
        }

        public event FileUploadedDelegate FileUploaded;

        private void OnFileUploaded(IEnumerable<File> files)
        {
            var lst = files.ToList();
            _itemCache.Invalidate(lst.Select(f => f.FullPath));
            _itemCache.Invalidate(lst.Select(file => file.Path).Distinct());
            FileUploaded?.Invoke(lst);
        }

        public T DownloadFileAsJson<T>(File file)
        {
            using var stream = Account.RequestRepo.GetDownloadStream(file);
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);

            var ser = new JsonSerializer();
            return ser.Deserialize<T>(jsonReader);
        }

        public string DownloadFileAsString(File file)
        {
            using var stream = Account.RequestRepo.GetDownloadStream(file);
            using var reader = new StreamReader(stream);

            string res = reader.ReadToEnd();
            return res;
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
                var file = (File)await GetItemAsync(path);
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


        public bool UploadFile(string path, byte[] content, bool discardEncryption = false)
        {
            using (var stream = GetFileUploadStream(path, content.Length, null, null,  discardEncryption).Result)
            {
                stream.Write(content, 0, content.Length);
            }
            _itemCache.Invalidate(path, WebDavPath.Parent(path));

            return true;
        }


        public bool UploadFile(string path, string content, bool discardEncryption = false)
        {
            var data = Encoding.UTF8.GetBytes(content);
            return UploadFile(path, data, discardEncryption);
            
        }

        public bool UploadFileJson<T>(string fullFilePath, T data, bool discardEncryption = false)
        {
            string content = JsonConvert.SerializeObject(data, Formatting.Indented);
            UploadFile(fullFilePath, content, discardEncryption);
            return true;
        }

        #region IDisposable Support
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                CancelToken.Dispose();
            }
            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public async Task<bool> LinkItem(Uri url, string path, string name, bool isFile, long size, DateTime? creationDate)
        {
            if (LinkManager == null)
                return false;

            var res = await LinkManager.Add(url, path, name, isFile, size, creationDate);
            if (res)
            {
                LinkManager.Save();
                _itemCache.Invalidate(path);
            }
            return res;
        }

        public async void RemoveDeadLinks()
        {
            if (LinkManager == null)
                return;

            var count = await LinkManager.RemoveDeadLinks(true);
            if (count > 0) _itemCache.Invalidate();
        }

        public async Task<AddFileResult> AddFile(IFileHash hash, string fullFilePath, long size, ConflictResolver? conflict = null)
        {
            var res = await Account.RequestRepo.AddFile(fullFilePath, hash, size, DateTime.Now, conflict);
            
            if (res.Success)
                _itemCache.Invalidate(fullFilePath, WebDavPath.Parent(fullFilePath));

            return res;
        }

        public async Task<AddFileResult> AddFileInCloud(File fileInfo, ConflictResolver? conflict = null)
        {
            var res = await AddFile(fileInfo.Hash, fileInfo.FullPath, fileInfo.OriginalSize, conflict);

            return res;
        }

        public async Task<bool> SetFileDateTime(File file, DateTime dateTime)
        {
            if (file.LastWriteTimeUtc == dateTime)
                return true;

            var added = await Account.RequestRepo.AddFile(file.FullPath, file.Hash, file.Size, dateTime, ConflictResolver.Rename);
            bool res = added.Success;
            if (res)
            {
                file.LastWriteTimeUtc = dateTime;
                _itemCache.Invalidate(file.Path, file.FullPath);
            }

            return res;
        }

        /// <summary>
        /// Создаёт в каталоге признак, что файлы в нём будут шифроваться
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task<bool> CryptInit(Folder folder)
        {
            // do not allow to crypt root path... don't know for what
            if (WebDavPath.PathEquals(folder.FullPath, WebDavPath.Root))
                return false;

            string filepath = WebDavPath.Combine(folder.FullPath, CryptFileInfo.FileName);
            var file = await GetItemAsync(filepath).ConfigureAwait(false);

            if (file != null)
                return false;

            var content = new CryptFileInfo
            {
                Initialized = DateTime.Now
            };

            var res = UploadFileJson(filepath, content);
            return res;
        }

        public void CleanTrash()
        {
            Repo.CleanTrash();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Links.Dto;
using File = YaR.MailRuCloud.Api.Base.File;

namespace YaR.MailRuCloud.Api.Links
{
    /// <summary>
    /// Управление ссылками, привязанными к облаку
    /// </summary>
    public class LinkManager
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(LinkManager));

        public static readonly string LinkContainerName = "item.links.wdmrc";
        public static readonly string HistoryContainerName = "item.links.history.wdmrc";
        private readonly MailRuCloud _cloud;
        private ItemList _itemList = new ItemList();
        private readonly StoreItemCache _itemCache;

        private readonly object _lockContainer = new object();


        public LinkManager(MailRuCloud cloud)
        {
            _cloud = cloud;
            _itemCache = new StoreItemCache(TimeSpan.FromSeconds(60)) { CleanUpPeriod = TimeSpan.FromMinutes(5) };

            cloud.FileUploaded += OnFileUploaded;

            Load();
        }

        private void OnFileUploaded(IEnumerable<File> files)
        {
            var file = files?.FirstOrDefault();
            if (null == file) return;

            if (file.Path == "/" && file.Name == LinkContainerName && file.Size > 3)
                Load();
        }

        /// <summary>
        /// Сохранить в файл в облаке список ссылок
        /// </summary>
        public void Save()
        {
            lock (_lockContainer)
            {
                
                Logger.Info($"Saving links to {LinkContainerName}");

                string content = JsonConvert.SerializeObject(_itemList, Formatting.Indented);
                string path = WebDavPath.Combine(WebDavPath.Root, LinkContainerName);
                try
                {
                    _cloud.FileUploaded -= OnFileUploaded;
                    _cloud.UploadFile(path, content);
                }
                finally
                {
                    _cloud.FileUploaded += OnFileUploaded;
                }
            }
        }

        /// <summary>
        /// Загрузить из файла в облаке список ссылок
        /// </summary>
        public void Load()
        {
            Logger.Info($"Loading links from {LinkContainerName}");

            try
            {
                lock (_lockContainer)
                {
                    string filepath = WebDavPath.Combine(WebDavPath.Root, LinkContainerName);
                    var file = (File)_cloud.GetItem(filepath, MailRuCloud.ItemType.File, false).Result;

                    if (file != null && file.Size > 3) //some clients put one/two/three-byte file before original file
                            _itemList = _cloud.DownloadFileAsJson<ItemList>(file);
                }
            }
            catch (Exception e)
            {
                Logger.Warn("Cannot load links", e);
            }

            if (null == _itemList) _itemList = new ItemList();

            foreach (var f in _itemList.Items)
            {
                f.MapTo = WebDavPath.Clean(f.MapTo);
            }
        }

        /// <summary>
        /// Получить список ссылок, привязанных к указанному пути в облаке
        /// </summary>
        /// <param name="path">Путь к каталогу в облаке</param>
        /// <returns></returns>
        public List<ItemLink> GetItems(string path)
        {
            var z = _itemList.Items
                .Where(f => f.MapTo == path)
                .ToList();

            return z;
        }

        /// <summary>
        /// Убрать ссылку
        /// </summary>
        /// <param name="path"></param>
        /// <param name="doSave">Save container after removing</param>
        public bool RemoveLink(string path, bool doSave = true)
        {
            var name = WebDavPath.Name(path);
            var parent = WebDavPath.Parent(path);

            var z = _itemList.Items.FirstOrDefault(f => f.MapTo == parent && f.Name == name);

            if (z != null)
            {
                _itemList.Items.Remove(z);
                _itemCache.Invalidate(path, parent);
                if (doSave) Save();
                return true;
            }
            return false;
        }

        public void RemoveLinks(IEnumerable<Link> innerLinks, bool doSave = true)
        {
            bool removed = false;
            foreach (var link in innerLinks)
            {
                var res = RemoveLink(link.FullPath, false);
                if (res) removed = true;
            }
            if (doSave && removed) Save();
        }


        /// <summary>
        /// Убрать все привязки на мёртвые ссылки
        /// </summary>
        /// <param name="doWriteHistory"></param>
        public async Task<int> RemoveDeadLinks(bool doWriteHistory)
        {
            var z = _cloud.GetItem(@"/__enc/3/_linked_test", MailRuCloud.ItemType.Folder, false).Result;

            var removes = _itemList.Items
                .AsParallel()
                .WithDegreeOfParallelism(5)
                .Select(it => GetItemLink(WebDavPath.Combine(it.MapTo, it.Name)).Result)
                .Where(itl => 
                    itl.IsBad || 
                    _cloud.GetItem(itl.MapPath, MailRuCloud.ItemType.Folder, false).Result == null)
                .ToList();
            if (removes.Count == 0) return 0;

            _itemList.Items.RemoveAll(it => removes.Any(rem => WebDavPath.PathEquals(rem.MapPath, it.MapTo) && rem.Name == it.Name));

            if (removes.Any())
            {
                if (doWriteHistory)
                {
                    foreach (var link in removes)
                    {
                        _itemCache.Invalidate(link.FullPath, link.MapPath);
                    }

                    string path = WebDavPath.Combine(WebDavPath.Root, HistoryContainerName);
                    string res = await _cloud.DownloadFileAsString(path);
                    var history = new StringBuilder(res ?? string.Empty);
                    foreach (var link in removes)
                    {
                        history.Append($"{DateTime.Now} REMOVE: {link.Href} {link.Name}\r\n");
                    }
                    _cloud.UploadFile(path, history.ToString());
                }
                Save();
                return removes.Count;
            }

            return 0;
        }

        ///// <summary>
        ///// Проверка доступности ссылки
        ///// </summary>
        ///// <param name="link"></param>
        ///// <returns></returns>
        //private bool IsLinkAlive(ItemLink link)
        //{
        //    string path = WebDavPath.Combine(link.MapTo, link.Name);
        //    try
        //    {
        //        var entry = _cloud.GetItem(path).Result;
        //        return entry != null;
        //    }
        //    catch (AggregateException e) 
        //    when (  // let's check if there really no file or just other network error
        //            e.InnerException is WebException we && 
        //            (we.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound
        //         )
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="doResolveType">Resolving file/folder type requires addition request to cloud</param>
        /// <returns></returns>
        public async Task<Link> GetItemLink(string path, bool doResolveType = true)
        {
            var cached = _itemCache.Get(path);
            if (null != cached)
                return (Link)cached;

            //TODO: subject to refact
            string parent = path;
            ItemLink wp;
            string right = string.Empty;
            do
            {
                string name = WebDavPath.Name(parent);
                parent = WebDavPath.Parent(parent);
                wp = _itemList.Items.FirstOrDefault(ip => parent == ip.MapTo && name == ip.Name);
                if (null == wp) right = WebDavPath.Combine(name, right);
            } while (parent != WebDavPath.Root && null == wp);

            if (null == wp) return null;

            var link = new Link(wp, path, wp.Href + right);

            //resolve additional link properties, e.g. OriginalName, ItemType, Size
            if (doResolveType)
                await ResolveLink(link);

            _itemCache.Add(link.FullPath, link);
            return link;
        }

        private async Task ResolveLink(Link link)
        {
            try
            {
                var infores = await new ItemInfoRequest(_cloud.CloudApi, link.Href, true).MakeRequestAsync();
                link.ItemType = infores.body.kind == "file"
                    ? MailRuCloud.ItemType.File
                    : MailRuCloud.ItemType.Folder;
                link.OriginalName = infores.body.name;
                link.Size = infores.body.size;

                link.IsResolved = true;
            }
            catch (Exception) //TODO check 404 etc.
            {
                //this means a bad link
                // don't know what to do
                link.IsBad = true;
            }
        }

        //public IEnumerable<ItemLink> GetChilds(string folderFullPath, bool doResolveType)
        //{
        //    var lst = _itemList.Items
        //        .Where(it => 
        //        WebDavPath.IsParentOrSame(folderFullPath, it.MapTo));

        //    return lst;
        //}

        public IEnumerable<Link> GetChilds(string folderFullPath)
        {
            var lst = _itemList.Items
                .Where(it => WebDavPath.IsParentOrSame(folderFullPath, it.MapTo))
                .Select(it => GetItemLink(WebDavPath.Combine(it.MapTo, it.Name), false).Result);

            return lst;
        }


        /// <summary>
        /// Привязать ссылку к облаку
        /// </summary>
        /// <param name="url">Ссылка</param>
        /// <param name="path">Путь в облаке, в который поместить ссылку</param>
        /// <param name="name">Имя для ссылки</param>
        /// <param name="isFile">Признак, что ссылка ведёт на файл, иначе - на папку</param>
        /// <param name="size">Размер данных по ссылке</param>
        /// <param name="creationDate">Дата создания</param>
        public async Task<bool> Add(string url, string path, string name, bool isFile, long size, DateTime? creationDate)
        {
            path = WebDavPath.Clean(path);

            var folder = (Folder)await _cloud.GetItem(path);
            if (folder.Entries.Any(entry => entry.Name == name))
                return false;

            url = GetRelaLink(url);
            path = WebDavPath.Clean(path);

            if (folder.Entries.Any(entry => entry.Name == name))
                return false;
            if (_itemList.Items.Any(it => WebDavPath.PathEquals(it.MapTo, path) && it.Name == name))
                return false;

            _itemList.Items.Add(new ItemLink
            {
                Href = url,
                MapTo = path,
                Name = name,
                IsFile = isFile,
                Size = size,
                CreationDate = creationDate
            });

            _itemCache.Invalidate(path);
            return true;
        }





        private const string PublicBaseLink = "https://cloud.mail.ru/public";
        private const string PublicBaseLink1 = "https:/cloud.mail.ru/public"; //TODO: may be obsolete?

        private string GetRelaLink(string url)
        {
            if (url.StartsWith(PublicBaseLink)) return url.Remove(PublicBaseLink.Length);
            if (url.StartsWith(PublicBaseLink1)) return url.Remove(PublicBaseLink1.Length);
            return url;
        }

        public void ProcessRename(string fullPath, string newName)
        {
            string newPath = WebDavPath.Combine(WebDavPath.Parent(fullPath), newName);

            bool changed = false;
            foreach (var link in _itemList.Items)
            {
                if (WebDavPath.IsParentOrSame(fullPath, link.MapTo))
                {
                    link.MapTo = WebDavPath.ModifyParent(link.MapTo, fullPath, newPath);
                    changed = true;
                }
            }
            if (changed)
            {
                _itemCache.Invalidate(fullPath, newPath);
                Save();
            }
        }

        public bool RenameLink(Link link, string newName)
        {
            // can't rename items within linked folder
            if (!link.IsRoot) return false;

            var ilink = _itemList.Items.FirstOrDefault(it => WebDavPath.PathEquals(it.MapTo, link.MapPath) && it.Name == link.Name);
            if (null == ilink) return false;
            if (ilink.Name == newName) return true;

            ilink.Name = newName;
            Save();
            _itemCache.Invalidate(link.MapPath);
            return true;
        }


        /// <summary>
        /// Перемещение ссылки из одного каталога в другой
        /// </summary>
        /// <param name="link"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        /// <remarks>            
        /// Корневую ссылку просто перенесем
        ///
        /// Если это вложенная ссылка, то перенести ее нельзя, а можно
        /// 1. сделать новую ссылку на эту вложенность
        /// 2. скопировать содержимое
        /// если следовать логике, что при копировании мы копируем содержимое ссылок, а при перемещении - перемещаем ссылки, то надо делать новую ссылку
        ///</remarks>
        public async Task<bool> RemapLink(Link link, string destinationPath, bool doSave = true)
        {
            if (WebDavPath.PathEquals(link.MapPath, destinationPath))
                return true;

            if (link.IsRoot)
            {
                var rootlink = _itemList.Items.FirstOrDefault(it => WebDavPath.PathEquals(it.MapTo, link.MapPath) && it.Name == link.Name);
                if (rootlink != null)
                {
                    string oldmap = rootlink.MapTo;
                    rootlink.MapTo = destinationPath;
                    Save();
                    _itemCache.Invalidate(link.FullPath, oldmap, destinationPath);
                    return true;
                }
                return false;
            }

            // it's a link on inner item of root link, creating new link
            if (!link.IsResolved)
                await ResolveLink(link);

            var res = await Add(
                link.Href,
                destinationPath,
                link.Name,
                link.ItemType == MailRuCloud.ItemType.File,
                link.Size,
                DateTime.Now);

            if (res)
            {
                if (doSave) Save();
                _itemCache.Invalidate(destinationPath);
            }

            return res;
        }


    }
}
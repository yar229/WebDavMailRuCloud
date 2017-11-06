using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests;
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


        public LinkManager(MailRuCloud api)
        {
            _cloud = api;

            Load();
        }


        /// <summary>
        /// Сохранить в файл в облаке список ссылок
        /// </summary>
        public void Save()
        {
            Logger.Info($"Saving links to {LinkContainerName}");

            string content = JsonConvert.SerializeObject(_itemList, Formatting.Indented);
            string path = WebDavPath.Combine(WebDavPath.Root, LinkContainerName);
            _cloud.UploadFile(path, content);
        }

        /// <summary>
        /// Загрузить из файла в облаке список ссылок
        /// </summary>
        public void Load()
        {
            Logger.Info($"Loading links from {LinkContainerName}");

            try
            {
                var file = (File)_cloud.GetItem(WebDavPath.Combine(WebDavPath.Root, LinkContainerName), MailRuCloud.ItemType.File, false).Result;

                if (file != null && file.Size > 3) //some clients put one/two/three-byte file before original file
                    _itemList = _cloud.DownloadFileAsJson<ItemList>(file);
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
        public void RemoveItem(string path, bool doSave = true)
        {
            var name = WebDavPath.Name(path);
            var pa = WebDavPath.Parent(path);

            var z = _itemList.Items
                .FirstOrDefault(f => f.MapTo == pa && f.Name == name);

            if (z != null)
            {
                _itemList.Items.Remove(z);
                if (doSave) Save();
            }
        }

        /// <summary>
        /// Убрать все привязки на мёртвые ссылки
        /// </summary>
        /// <param name="doWriteHistory"></param>
        public async void RemoveDeadLinks(bool doWriteHistory)
        {
            var removes = _itemList.Items
                .AsParallel()
                .WithDegreeOfParallelism(5)
                .Select(it => GetItemLink(WebDavPath.Combine(it.MapTo, it.Name)).Result)
                .Where(itl => itl.IsBad)
                .ToList();
            if (removes.Count == 0) return;

            _itemList.Items.RemoveAll(it => removes.Any(rem => rem.MapTo == it.MapTo && rem.Name == it.Name));

            if (removes.Any())
            {
                if (doWriteHistory)
                {
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
            }
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
        public async Task<ItemfromLink> GetItemLink(string path, bool doResolveType = true)
        {
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

            var res = new ItemfromLink(wp) { Href = wp.Href + right, Path = path };

            if (doResolveType)
            {
                try
                {
                    var infores = await new ItemInfoRequest(_cloud.CloudApi, res.Href, true).MakeRequestAsync()
                        .ConfigureAwait(false);
                    res.IsFile = infores.body.kind == "file";
                    res.OriginalName = infores.body.name;
                }
                catch (Exception) //TODO check 404 etc.
                {
                    //this means a bad link
                    // don't know what to do
                    res.IsBad = true;
                }
            }

            return res;
        }

        public class ItemfromLink : ItemLink
        {
            private ItemfromLink() { }

            public ItemfromLink(ItemLink link) : this()
            {
                Href = link.Href;
                MapTo = link.MapTo;
                Name = link.Name;
                IsFile = link.IsFile;
                Size = link.Size;
                CreationDate = link.CreationDate;
            }

            public bool IsBad { get; set; }
            public string Path { get; set; }

            public bool IsRoot => WebDavPath.PathEquals(WebDavPath.Parent(Path), MapTo);
            public string OriginalName { get; set; }

            public IEntry ToBadEntry()
            {
                var res = IsFile
                    ? (IEntry)new File(Path, Size, string.Empty)
                    : new Folder(Size, Path, string.Empty);

                return res;
            }
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
        public async void Add(string url, string path, string name, bool isFile, long size, DateTime? creationDate)
        {
            Load();

            path = WebDavPath.Clean(path);

            var folder = (Folder)await _cloud.GetItem(path);
            if (folder.Entries.Any(entry => entry.Name == name))
                return;

            url = GetRelaLink(url);

            _itemList.Items.Add(new ItemLink
            {
                Href = url,
                MapTo = WebDavPath.Clean(path),
                Name = name,
                IsFile = isFile,
                Size = size,
                CreationDate = creationDate
            });
            Save();
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
            if (changed) Save();
        }

        public bool RenameLink(ItemfromLink link, string newName)
        {
            // can't rename items within linked folder
            if (!link.IsRoot) return false;

            var ilink = _itemList.Items.FirstOrDefault(it => it.MapTo == link.MapTo && it.Name == link.Name);
            if (null == ilink) return false;

            ilink.Name = newName;
            Save();
            return true;
        }
    }
}
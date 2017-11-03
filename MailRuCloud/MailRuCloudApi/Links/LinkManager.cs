using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base;
using File = YaR.MailRuCloud.Api.Base.File;

namespace YaR.MailRuCloud.Api.Links
{
    /// <summary>
    /// Управление ссылками, привязанными к облаку
    /// </summary>
    public class LinkManager
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(LinkManager));

        public static string LinkContainerName = "item.links.wdmrc";
        private readonly MailRuCloud _cloud;
        private ItemList _itemList;


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
            var data = Encoding.UTF8.GetBytes(content);

            using (var stream = _cloud.GetFileUploadStream(WebDavPath.Combine(WebDavPath.Root, LinkContainerName), data.Length))
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
        }

        /// <summary>
        /// Загрузить из файла в облаке список ссылок
        /// </summary>
        public void Load()
        {
            Logger.Info($"Loading links from {LinkContainerName}");

            var file = (File)_cloud.GetItem(WebDavPath.Combine(WebDavPath.Root, LinkContainerName), MailRuCloud.ItemType.File, false).Result;

            if (file != null && file.Size > 3) //some clients put one/two/three-byte file before original file
            {
                DownloadStream stream = new DownloadStream(file, _cloud.CloudApi);

                using (StreamReader reader = new StreamReader(stream))
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    var ser = new JsonSerializer();
                    _itemList = ser.Deserialize<ItemList>(jsonReader);
                }
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

        public ItemLink GetItem(string path)
        {
            var name = WebDavPath.Name(path);
            var pa = WebDavPath.Parent(path);

            var item = _itemList.Items
                .FirstOrDefault(f => f.MapTo == pa && f.Name == name);

            return item;
        }

        /// <summary>
        /// Убрать ссылку
        /// </summary>
        /// <param name="path"></param>
        public void RemoveItem(string path)
        {
            var name = WebDavPath.Name(path);
            var pa = WebDavPath.Parent(path);

            var z = _itemList.Items
                .FirstOrDefault(f => f.MapTo == pa && f.Name == name);

            if (z != null)
            {
                _itemList.Items.Remove(z);
                Save();
            }
        }

        /// <summary>
        /// Убрать все привязки на мёртвые ссылки
        /// </summary>
        /// <param name="doWriteHistory"></param>
        public void RemoveDeadLinks(bool doWriteHistory)
        {
            var removes = _itemList.Items
                .AsParallel()
                .WithDegreeOfParallelism(5)
                .Where(it => !IsLinkAlive(it)).ToList();
            if (removes.Count == 0) return;

            _itemList.Items.RemoveAll(it => removes.Contains(it));

            if (doWriteHistory)
            {
                //TODO:load item.links.history.wdmrc
                //TODO:append removed
                //TODO:save item.links.history.wdmrc
            }

            Save();
        }

        /// <summary>
        /// Проверка доступности ссылки
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private bool IsLinkAlive(ItemLink link)
        {
            string path = WebDavPath.Combine(link.MapTo, link.Name);
            try
            {
                var entry = _cloud.GetItem(path).Result;
                return entry != null;
            }
            catch (AggregateException e) 
            when (  // let's check if there really no file or just other network error
                    e.InnerException is WebException we && 
                    (we.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound
                 )
            {
                return false;
            }
        }

        public string AsWebLink(string path)
        {
            //TODO: subject to refact
            string parent = path;
            string wp;
            string right = string.Empty;
            do
            {
                string name = WebDavPath.Name(parent);
                parent = WebDavPath.Parent(parent);
                wp = _itemList.Items.FirstOrDefault(ip => parent == ip.MapTo && name == ip.Name)?.Href;
                if (string.IsNullOrEmpty(wp)) right = WebDavPath.Combine(name, right);
            } while (parent != WebDavPath.Root  && string.IsNullOrEmpty(wp));
            
            return string.IsNullOrEmpty(wp)
                ? string.Empty
                : wp + right;
        }



        public string AsRelationalWebLink(string path)
        {
            //TODO: subject to refact
            string link = AsWebLink(path);
            link = GetRelaLink(link);

            return link;
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
        public void Add(string url, string path, string name, bool isFile, long size, DateTime? creationDate)
        {
            Load();

            path = WebDavPath.Clean(path);
            url = GetRelaLink(url);

            if (!_itemList.Items.Any(ii => ii.MapTo == path && ii.Name == name))
            {
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
            //TODO: implement
        }
    }
}
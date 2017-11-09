using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Extensions;
using YaR.MailRuCloud.Api.Links;

namespace YaR.MailRuCloud.Api
{
    public class StoreItemCache
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(StoreItemCache));

        public StoreItemCache(MailRuCloud store, LinkManager linkManager, TimeSpan expirePeriod)
        {
            _store = store;
            _linkManager = linkManager;
            _expirePeriod = expirePeriod;

            long cleanPeriod = (long)CleanUpPeriod.TotalMilliseconds;

            _cleanTimer = new Timer(state => RemoveExpired(), null, cleanPeriod, cleanPeriod);
        }

        private readonly MailRuCloud _store;
        private readonly LinkManager _linkManager;
        private readonly Timer _cleanTimer;
        private readonly ConcurrentDictionary<string, TimedItem<IEntry>> _items = new ConcurrentDictionary<string, TimedItem<IEntry>>();
        private readonly object _locker = new object();

        public TimeSpan CleanUpPeriod
        {
            get => _cleanUpPeriod;
            set
            {
                _cleanUpPeriod = value;
                long cleanPreiod = (long)value.TotalMilliseconds;
                _cleanTimer.Change(cleanPreiod, cleanPreiod);
            }
        }

        public int RemoveExpired()
        {
            if (!_items.Any()) return 0;

            int removedCount = 0;
            foreach (var item in _items)
            {
                if (DateTime.Now - item.Value.Created > TimeSpan.FromMinutes(5))
                {
                    bool removed = _items.TryRemove(item.Key, out _);
                    if (removed) removedCount++;
                }
            }
            if (removedCount > 0)
                Logger.Debug($"Items cache clean: removed {removedCount} expired items");

            return removedCount;
        }

        public async Task<IEntry> Get(string path, MailRuCloud.ItemType itemType = MailRuCloud.ItemType.Unknown, bool resolveLinks = true)
        {
            if (_items.TryGetValue(path, out var item))
            {
                if (IsExpired(item))
                    _items.TryRemove(path, out item);
                else
                {
                    Logger.Debug($"Cache hit: {item.Item.FullPath}");
                    return item.Item;
                }
            }

            var newitem = await GetItem(path, itemType, resolveLinks);
            lock (_locker)
            {
                if (!_items.TryGetValue(path, out item))
                {
                    item = new TimedItem<IEntry>
                    {
                        Created = DateTime.Now,
                        Item = newitem
                    };

                    if (!_items.TryAdd(path, item))
                        _items.TryGetValue(path, out item);
                }
            }

            return item.Item;
        }


        /// <summary>
        /// Get list of files and folders from account.
        /// </summary>
        /// <param name="path">Path in the cloud to return the list of the items.</param>
        /// <param  name="itemType">Unknown, File/Folder if you know for sure</param>
        /// <param name="resolveLinks">True if you know for sure that's not a linked item</param>
        /// <returns>List of the items.</returns>
        public virtual async Task<IEntry> GetItem(string path, MailRuCloud.ItemType itemType = MailRuCloud.ItemType.Unknown, bool resolveLinks = true)
        {
            //TODO: subject to refact!!!
            var ulink = resolveLinks ? await _linkManager.GetItemLink(path) : null;

            // bad link detected, just return stub
            // cause client cannot, for example, delete it if we return NotFound for this item
            if (ulink != null && ulink.IsBad)
                return ulink.ToBadEntry();


            var datares = await new FolderInfoRequest(_store.CloudApi, null == ulink ? path : ulink.Href, ulink != null)
                .MakeRequestAsync().ConfigureAwait(false);

            if (itemType == MailRuCloud.ItemType.Unknown && ulink != null)
                itemType = ulink.ItemType;

            if (itemType == MailRuCloud.ItemType.Unknown && null == ulink)
                itemType = datares.body.home == path
                    ? MailRuCloud.ItemType.Folder
                    : MailRuCloud.ItemType.File;

            var entry = itemType == MailRuCloud.ItemType.File
                ? (IEntry)datares.ToFile(
                    home: itemType != MailRuCloud.ItemType.File ? path : WebDavPath.Parent(path),
                    ulink: ulink,
                    filename: ulink == null ? WebDavPath.Name(path) : ulink.OriginalName,
                    nameReplacement: WebDavPath.Name(path))
                : datares.ToFolder(
                    itemType != MailRuCloud.ItemType.File ? path : WebDavPath.Parent(path),
                    ulink);

            // fill folder with links if any
            if (itemType == MailRuCloud.ItemType.Folder && entry is Folder folder)
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

        public IEntry Invalidate(string path)
        {
            _items.TryRemove(path, out var item);
            return item?.Item;
        }

        public void Invalidate()
        {
            _items.Clear();
        }

        public void Invalidate(params string[] paths)
        {
            foreach (var path in paths)
            {
                _items.TryRemove(path, out _);
            }
        }

        public IEntry Invalidate(IEntry item)
        {
            return Invalidate(item.FullPath);
        }

        private bool IsExpired(TimedItem<IEntry> item)
        {
            return DateTime.Now - item.Created > _expirePeriod;
        }

        private readonly TimeSpan _expirePeriod;
        private TimeSpan _cleanUpPeriod = TimeSpan.FromMinutes(5);

        private class TimedItem<T>
        {
            public DateTime Created { get; set; }
            public T Item { get; set; }
        }
    }
}
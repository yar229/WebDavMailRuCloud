using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public StoreItemCache(TimeSpan expirePeriod)
        {
            _expirePeriod = expirePeriod;

            long cleanPeriod = (long)CleanUpPeriod.TotalMilliseconds;

            _cleanTimer = new Timer(state => RemoveExpired(), null, cleanPeriod, cleanPeriod);
        }

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

        public IEntry Get(string path)
        {
            if (_items.TryGetValue(path, out var item))
            {
                if (IsExpired(item))
                    _items.TryRemove(path, out item);
                else
                {
                    Logger.Warn($"Cache hit: {item.Item.FullPath}");
                    return item.Item;
                }
            }
            return null;
        }

        public void Add(string path, IEntry itemin)
        {
            var item = new TimedItem<IEntry>
            {
                Created = DateTime.Now,
                Item = itemin
            };

            _items.AddOrUpdate(path, item, (key, oldValue) => item);
        }

        public void Add(IEnumerable<IEntry> items)
        {
            foreach (var item in items)
            {
                Add(item.FullPath, item);
            }
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
            Invalidate(paths.AsEnumerable());
        }

        internal void Invalidate(IEnumerable<string> paths)
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

        public void Invalidate(IEnumerable<IEntry> items)
        {
            foreach (var item in items)
            {
                _items.TryRemove(item.FullPath, out _);
            }
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
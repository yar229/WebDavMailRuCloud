using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace YaR.MailRuCloud.Api.Common
{
    public class ItemCache<TKey, TValue>
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(ItemCache<TKey, TValue>));

        public ItemCache(TimeSpan expirePeriod)
        {
            _expirePeriod = expirePeriod;
            _noCache = Math.Abs(_expirePeriod.TotalMilliseconds) < 0.001;

            long cleanPeriod = (long)CleanUpPeriod.TotalMilliseconds;

            _cleanTimer = new Timer(state => RemoveExpired(), null, cleanPeriod, cleanPeriod);
        }

        private readonly bool _noCache;

        private readonly Timer _cleanTimer;
        private readonly ConcurrentDictionary<TKey, TimedItem<TValue>> _items = new ConcurrentDictionary<TKey, TimedItem<TValue>>();
        //private readonly object _locker = new object();

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

        public TValue Get(TKey key)
        {
            if (_noCache)
                return default;

            if (_items.TryGetValue(key, out var item))
            {
                if (IsExpired(item))
                    _items.TryRemove(key, out item);
                else
                {
                    Logger.Debug($"Cache hit: {key}");
                    return item.Item;
                }
            }
            return default;
        }

        public void Add(TKey key, TValue value)
        {
            if (_noCache) return;

            var item = new TimedItem<TValue>
            {
                Created = DateTime.Now,
                Item = value
            };

            _items.AddOrUpdate(key, item, (key1, oldValue) => item);
        }

        public void Add(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var item in items)
            {
                Add(item.Key, item.Value);
            }
        }

        public TValue Invalidate(TKey key)
        {
            _items.TryRemove(key, out var item);
            return item != null ? item.Item : default;
        }

        public void Invalidate()
        {
            _items.Clear();
        }

        public void Invalidate(params TKey[] keys)
        {
            Invalidate(keys.AsEnumerable());
        }

        internal void Invalidate(IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
            {
                _items.TryRemove(key, out _);
            }
        }

        //public TValue Invalidate(TValue item)
        //{
        //    return Invalidate(item.FullPath);
        //}

        //public void Invalidate(IEnumerable<IEntry> items)
        //{
        //    foreach (var item in items)
        //    {
        //        _items.TryRemove(item.FullPath, out _);
        //    }
        //}

        private bool IsExpired(TimedItem<TValue> item)
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
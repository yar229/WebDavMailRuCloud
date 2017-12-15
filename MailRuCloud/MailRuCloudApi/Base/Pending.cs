using System;
using System.Collections.Generic;
using System.Text;

namespace YaR.MailRuCloud.Api.Base
{
    class Pending<T>
    {
        private readonly List<PendingItem<T>> _items = new List<PendingItem<T>>();
        private readonly int _maxLocks;
        private readonly Func<T> _valueFactory;

        public Pending(int maxLocks, Func<T> valueFactory)
        {
            _maxLocks = maxLocks;
            _valueFactory = valueFactory;
        }

        private readonly object _lock = new object();

        public T Use()
        {
            lock (_lock)
            {
                T res;

                foreach (var item in _items)
                {
                    if (item.LockCount < _maxLocks)
                    {
                        item.LockCount++;
                        res = item.Item;
                        return res;
                    }
                }

                res = _valueFactory();
                _items.Add(new PendingItem<T>{Item = res, LockCount = 1});
                return res;
            }
        }

        public void Free(T value)
        {
            lock (_lock)
            {
                foreach (var item in _items)
                    if (item.Item.Equals(value))
                    {
                        if (item.LockCount <= 0)
                            throw new Exception("Pending item count <= 0");
                        if (item.LockCount > 0)
                            item.LockCount--;
                    }
            }
        }



        
    }

    class PendingItem<T>
    {
        public T Item { get; set; }
        public int LockCount { get; set; }
    }
}

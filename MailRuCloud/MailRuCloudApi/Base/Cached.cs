using System;

namespace YaR.MailRuCloud.Api.Base
{
    public class Cached<T>
    {
        private readonly TimeSpan _duration;
        private DateTime _expiration;
        private Lazy<T> _value;
        private readonly Func<T> _valueFactory;

        public T Value
        {
            get
            {
                RefreshValueIfNeeded();
                return _value.Value;
            }
        }

        public Cached(Func<T> valueFactory, TimeSpan duration)
        {
            _duration = duration;
            _valueFactory = valueFactory;

            RefreshValueIfNeeded();
        }

        private readonly object _refreshLock = new object();

        private void RefreshValueIfNeeded()
        {
            if (DateTime.Now >= _expiration)
            {
                lock (_refreshLock)
                {
                    if (DateTime.Now >= _expiration)
                    {
                        _value = new Lazy<T>(_valueFactory);
                        _expiration = DateTime.Now.Add(_duration);
                    }
                }
            }
        }
    }
}
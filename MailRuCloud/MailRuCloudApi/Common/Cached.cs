using System;

namespace YaR.MailRuCloud.Api.Common
{
    public class Cached<T>
    {
        private readonly Func<T, TimeSpan> _duration;
        private DateTime _expiration;
        private Lazy<T> _value;
        private readonly Func<T, T> _valueFactory;

        public T Value
        {
            get
            {
                RefreshValueIfNeeded();
                return _value.Value;
            }
        }

        public Cached(Func<T, T> valueFactory, Func<T, TimeSpan> duration)
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
                        T oldValue =  null != _value && _value.IsValueCreated ? _value.Value : default(T);
                        _value = new Lazy<T>(() => _valueFactory(oldValue));

                        var duration = _duration(_value.Value);
                        _expiration = DateTime.Now.Add(duration);
                    }
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void Expire()
        {
            lock (_refreshLock)
            {
                _expiration = DateTime.MinValue;
            }
        }
    }
}
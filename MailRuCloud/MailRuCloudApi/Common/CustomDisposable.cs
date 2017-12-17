using System;

namespace YaR.MailRuCloud.Api.Common
{
    class CustomDisposable<T> : IDisposable
    {
        public T Value { get; set; }
        public Action OnDispose { get; set; }

        public void Dispose()
        {
            var value = Value;
            if (value != null && value is IDisposable disp)
                disp.Dispose();

            OnDispose?.Invoke();
        }
    }
}

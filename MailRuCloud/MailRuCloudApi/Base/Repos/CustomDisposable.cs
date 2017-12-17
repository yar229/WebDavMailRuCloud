using System;
using System.Collections.Generic;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Repos
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

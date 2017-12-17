using System;
using System.Collections.Generic;
using System.Threading;

namespace YaR.MailRuCloud.Api
{
    public static class Retry
    {
        public static T Do<T>(
            Func<T> action,
            Func<Exception, bool> retryIf,
            Action<Exception> onException,
            TimeSpan retryInterval,
            int maxAttemptCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0)
                        Thread.Sleep(retryInterval);

                    return action();
                }
                catch (Exception ex)
                {
                    onException?.Invoke(ex);
                    exceptions.Add(ex);
                    if (retryIf != null && !retryIf(ex))
                        throw;
                }
            }
            throw new AggregateException(exceptions);
        }
    }
}

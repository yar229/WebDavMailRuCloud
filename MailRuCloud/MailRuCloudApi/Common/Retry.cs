using System;
using System.Collections.Generic;
using System.Threading;

namespace YaR.Clouds.Common
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

        public static T Do<T>(
            Func<TimeSpan> sleepBefore,
            Func<T> action,
            Func<T, bool> retryIf,
            TimeSpan retryInterval,
            int maxAttemptCount = 3)
        {
            var sleep = sleepBefore();
            if (sleep > TimeSpan.Zero)
                Thread.Sleep(sleep);

            T res = default;
            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                    if (attempted > 0)
                        Thread.Sleep(retryInterval);

                    res = action();
                    if (!retryIf(res))
                        return res;
            }

            return res;
        }
    }
}

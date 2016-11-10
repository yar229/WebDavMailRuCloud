using System;
using NWebDav.Server.Logging;

namespace NWebDav.Sample.HttpListener.LogAdapters
{
    public class Log4NetAdapter : ILoggerFactory
    {
        private class Log4NetLoggerAdapter : ILogger
        {
            private readonly log4net.ILog _log;

            public Log4NetLoggerAdapter(Type type)
            {
                // Obtain the Log4NET logger for this type
                _log = log4net.LogManager.GetLogger(type);
            }

            public bool IsLogEnabled(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        return _log.IsDebugEnabled;
                    case LogLevel.Info:
                        return _log.IsInfoEnabled;
                    case LogLevel.Warning:
                        return _log.IsWarnEnabled;
                    case LogLevel.Error:
                        return _log.IsErrorEnabled;
                    case LogLevel.Fatal:
                        return _log.IsFatalEnabled;
                }
                throw new ArgumentException($"Log level '{logLevel}' is not supported.", nameof(logLevel));
            }

            public void Log(LogLevel logLevel, string message, Exception exception)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        _log.Debug(message, exception);
                        break;
                    case LogLevel.Info:
                        _log.Info(message, exception);
                        break;
                    case LogLevel.Warning:
                        _log.Warn(message, exception);
                        break;
                    case LogLevel.Error:
                        _log.Error(message, exception);
                        break;
                    case LogLevel.Fatal:
                        _log.Fatal(message, exception);
                        break;
                    default:
                        throw new ArgumentException($"Log level '{logLevel}' is not supported.", nameof(logLevel));
                }
            }
        }

        public ILogger CreateLogger(Type type)
        {
            return new Log4NetLoggerAdapter(type);
        }
    }
}

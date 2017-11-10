using System;
using NWebDav.Server.Logging;

namespace YaR.WebDavMailRu
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

            public void Log(LogLevel logLevel, Func<string> messageFunc, Exception exception)
            {
                string msg;
                try
                {
                    msg = messageFunc();
                }
                catch (Exception e)
                {
                    msg = "Failed to get error message: " + e.Message;
                }

                switch (logLevel)
                {
                    case LogLevel.Debug:
                        //if (_log.IsDebugEnabled)
                        //    _log.Debug(msg, exception);
                        break;
                    case LogLevel.Info:
                        if (_log.IsInfoEnabled)
                            _log.Info(msg, exception);
                        break;
                    case LogLevel.Warning:
                        if (_log.IsWarnEnabled)
                            _log.Warn(msg, exception);
                        break;
                    case LogLevel.Error:
                        if (_log.IsErrorEnabled)
                            _log.Error(msg, exception);
                        break;
                    case LogLevel.Fatal:
                        if (_log.IsFatalEnabled)
                            _log.Fatal(msg, exception);
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
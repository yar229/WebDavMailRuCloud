using System;

namespace NWebDav.Server.Logging
{
    public class NullLoggerFactory : ILoggerFactory
    {
        private class Logger : ILogger
        {
            public bool IsLogEnabled(LogLevel logLevel) => false;
            public void Log(LogLevel logLevel, string message, Exception exception)
            {
            }
        }

        private static readonly ILogger s_defaultLogger = new Logger();

        public ILogger CreateLogger(Type type) => s_defaultLogger;
    }
}

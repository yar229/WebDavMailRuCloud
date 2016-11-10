using System;

namespace NWebDav.Server.Logging
{
    public interface ILogger
    {
        bool IsLogEnabled(LogLevel logLevel);
        void Log(LogLevel logLevel, string message, Exception exception = null);
    }
}

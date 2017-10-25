using System;

namespace NWebDav.Server.Logging
{
    /// <summary>
    /// Implementation of a logger factory that creates <see cref="ILogger"/>
    /// instances that don't log anything.
    /// </summary>
    public class NullLoggerFactory : ILoggerFactory
    {
        private class NullLogger : ILogger
        {
            public bool IsLogEnabled(LogLevel logLevel) => false;
            public void Log(LogLevel logLevel, Func<string> messageFunc, Exception exception)
            {
            }
        }

        private static readonly ILogger s_defaultLogger = new NullLogger();

        /// <summary>
        /// Create a dummy logger for the specified type.
        /// </summary>
        /// <param name="type">Type for which a logger should be created.</param>
        /// <returns>Logger object (doesn't do anything).</returns>
        public ILogger CreateLogger(Type type) => s_defaultLogger;
    }
}

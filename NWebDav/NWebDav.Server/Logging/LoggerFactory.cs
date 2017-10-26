using System;

namespace NWebDav.Server.Logging
{
    /// <summary>
    /// Helper class to specify the default logger for the NWebDAV code.
    /// </summary>
    public static class LoggerFactory
    {
        private static readonly NullLoggerFactory s_defaultLoggerFactory = new NullLoggerFactory();

        /// <summary>
        /// Get and set the default logger factory.
        /// </summary>
        public static ILoggerFactory Factory { get; set; }

        public static ILogger CreateLogger(Type type)
        {
            var factory = Factory ?? s_defaultLoggerFactory;
            return factory.CreateLogger(type);
        }
    }
}

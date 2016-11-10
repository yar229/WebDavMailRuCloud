using System;

namespace NWebDav.Server.Logging
{
    public static class LoggerFactory
    {
        private static readonly NullLoggerFactory s_defaultLoggerFactory = new NullLoggerFactory();

        public static ILoggerFactory Factory { get; set; }

        public static ILogger CreateLogger(Type type)
        {
            var factory = Factory ?? s_defaultLoggerFactory;
            return factory.CreateLogger(type);
        }
    }
}

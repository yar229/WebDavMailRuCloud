using System;

namespace NWebDav.Server.Logging
{
    /// <summary>
    /// Interface for logging events for a specific type.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Check if the specified log level is enabled.
        /// </summary>
        /// <param name="logLevel">Log level that should be checked.</param>
        /// <returns>
        /// Flag indicating whether the specified log-level is enabled.
        /// </returns>
        bool IsLogEnabled(LogLevel logLevel);

        /// <summary>
        /// Log a message and an optional exception with the specified log level.
        /// </summary>
        /// <param name="logLevel">
        /// Log level that specified the priority of the event.
        /// </param>
        /// <param name="messageFunc">
        /// Function that returns the message that should be logged if the log
        /// level is enabled.
        /// </param>
        /// <param name="exception">
        /// Optional exception that is logged with the event.
        /// </param>
        /// <remarks>
        /// This method uses a function for the <paramref name="messageFunc"/>
        /// parameter that is only evaluated if the log level is enabled. This
        /// reduces excessive string formatting for disabled log levels.
        /// </remarks>
        void Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null);
    }
}

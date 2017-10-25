namespace NWebDav.Server.Logging
{
    /// <summary>
    /// The log level specifies the priority of the log event.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug level designates fine-grained informational events that are most useful to debug an application.
        /// </summary>
        Debug,

        /// <summary>
        /// Info level designates informational messages that highlight the progress of the application at coarse-grained level.
        /// </summary>
        Info,

        /// <summary>
        /// Warning level designates potentially harmful situations.
        /// </summary>
        Warning,

        /// <summary>
        /// Error level designates error events that might still allow the application to continue running.
        /// </summary>
        Error,

        /// <summary>
        /// Fatal level designates very severe error events that will presumably lead the application to abort.
        /// </summary>
        Fatal
    }
}

using System;

namespace NWebDav.Server.Logging
{
    /// <summary>
    /// Interface to create <see cref="ILogger"/> instances.
    /// </summary>
    /// <seealso cref="ILogger"/>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Create a logger for the specified type.
        /// </summary>
        /// <param name="type">Type for which a logger should be created.</param>
        /// <returns>Logger object.</returns>
        ILogger CreateLogger(Type type);
    }
}

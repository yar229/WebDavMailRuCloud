//-----------------------------------------------------------------------
// <created file="ProgressChangeTaskState.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

namespace MailRuCloudApi
{
    /// <summary>
    /// Currently operation to cloud.
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Operation type is not defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// Upload file on server operation.
        /// </summary>
        Upload = 1,

        /// <summary>
        /// Download file from server operation.
        /// </summary>
        Download = 2
    }

    /// <summary>
    /// Task state is used for progress changed event.
    /// </summary>
    public class ProgressChangeTaskState
    {
        /// <summary>
        /// Gets the currently operation to cloud.
        /// </summary>
        /// <value>Operation type.</value>
        public OperationType Type { get; internal set; }

        /// <summary>
        /// Gets the total operation bytes.
        /// </summary>
        /// <value>Total bytes.</value>
        public FileSize TotalBytes { get; internal set; }

        /// <summary>
        /// Gets the bytes in progress for currently operation.
        /// </summary>
        /// <value>Bytes in progress.</value>
        public FileSize BytesInProgress { get; internal set; }
    }
}

//-----------------------------------------------------------------------
// <created file="MultiFilePart.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

namespace MailRuCloudApi
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Part of the multi file.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Serializable]
    public class MultiFilePart
    {
        /// <summary>
        /// Gets or sets index number.
        /// </summary>
        /// <value>Index number.</value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets original file name.
        /// </summary>
        /// <value>Original file name.</value>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets file size.
        /// </summary>
        /// <value>File size.</value>
        public long Size { get; set; }
    }
}

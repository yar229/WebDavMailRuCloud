// <created file="MultiFile.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

namespace MailRuCloudApi
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// File composed from several pieces.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Serializable]
    public class MultiFile
    {
        /// <summary>
        /// Gets or sets original file name.
        /// </summary>
        /// <value>Original file name.</value>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets file size.
        /// </summary>
        /// <value>Total file size.</value>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets multi file parts.
        /// </summary>
        /// <value>Multi file parts.</value>
        [XmlElement("Part")]
        public MultiFilePart[] Parts { get; set; }
    }
}

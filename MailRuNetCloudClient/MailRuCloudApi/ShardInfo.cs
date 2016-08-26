//-----------------------------------------------------------------------
// <created file="ShardInfo.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

namespace MailRuCloudApi
{
    using System.ComponentModel;

    /// <summary>
    /// Shard types.
    /// </summary>
    internal enum ShardType
    {
        /// <summary>
        /// Get video shard.
        /// </summary>
        [Description("video")]
        Video = 0,

        /// <summary>
        /// Get view direct shard.
        /// </summary>
        [Description("view_direct")]
        ViewDirect = 1,

        /// <summary>
        /// Web link view shard.
        /// </summary>
        [Description("weblink_view")]
        WeblinkView = 2,

        /// <summary>
        /// Web link view shard.
        /// </summary>
        [Description("weblink_video")]
        WeblinkVideo = 3,

        /// <summary>
        /// Web link get shard.
        /// </summary>
        [Description("weblink_get")]
        WeblinkGet = 4,

        /// <summary>
        /// Web link get thumbnails shard.
        /// </summary>
        [Description("weblink_thumbnails")]
        WeblinkThumbnails = 5,

        /// <summary>
        /// Authorization shard.
        /// </summary>
        [Description("auth")]
        Auth = 6,

        /// <summary>
        /// View shard.
        /// </summary>
        [Description("view")]
        View = 7,

        /// <summary>
        /// Get shard.
        /// </summary>
        [Description("get")]
        Get = 8,

        /// <summary>
        /// Upload items shard.
        /// </summary>
        [Description("upload")]
        Upload = 9,

        /// <summary>
        /// Thumbnails shard.
        /// </summary>
        [Description("thumbnails")]
        Thumbnails = 10
    }

    /// <summary>
    /// Servers info class.
    /// </summary>
    internal class ShardInfo
    {
        /// <summary>
        /// Gets or sets shard type.
        /// </summary>
        /// <value>Shard type.</value>
        public ShardType Type { get; internal set; }

        /// <summary>
        /// Gets or sets number of the shards.
        /// </summary>
        /// <value>Number of the shards.</value>
        public int Count { get; internal set; }

        /// <summary>
        /// Gets or sets shard link.
        /// </summary>
        /// <value>Shard link.</value>
        public string Url { get; internal set; }
    }
}

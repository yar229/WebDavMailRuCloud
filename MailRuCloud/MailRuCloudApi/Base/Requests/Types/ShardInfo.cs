namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    /// <summary>
    /// Servers info class.
    /// </summary>
    public class ShardInfo
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

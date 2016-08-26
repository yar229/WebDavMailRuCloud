namespace WebDAVSharp.Server
{
    /// <summary>
    /// The property with all the information of a lock
    /// </summary>
   internal class LockProperty
    {
        public string Locktype { get; set; }
        public string Lockscope { get; set; }
        public string Depth { get; set; }
        public string Owner { get; set; }
        public string Timeout { get; set; }
        public string Locktoken { get; set; }

        /// <summary>
        /// The standard constructor
        /// </summary>
        public LockProperty()
        {
            Locktype = string.Empty;
            Lockscope = string.Empty;
            Depth = string.Empty;
            Owner = string.Empty;
            Timeout = string.Empty;
            Locktoken = string.Empty;
        }

        /// <summary>
        /// The constructor with all the specific values
        /// </summary>
        /// <param name="locktype">The locktype of the lock</param>
        /// <param name="lockscope">The lockscope of the lock</param>
        /// <param name="depth">The depth of the lock</param>
        /// <param name="owner">The owner of the lock</param>
        /// <param name="timeout">The timeout of the lock</param>
        /// <param name="locktoken">The locktoken.</param>
        public LockProperty(string locktype, string lockscope, string depth, string owner, string timeout, string locktoken)
        {
            Locktype = locktype;
            Lockscope = lockscope;
            Depth = depth;
            Owner = owner;
            Timeout = timeout;
            Locktoken = locktoken;
        }


    }
}
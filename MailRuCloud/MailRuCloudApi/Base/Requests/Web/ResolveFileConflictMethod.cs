namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    public struct ConflictResolver
    {
        private readonly string _value;

        private ConflictResolver(string value)
        {
            _value = value;
        }

        public static ConflictResolver Rename => new ConflictResolver("rename");
        public static ConflictResolver Rewrite => new ConflictResolver("rewrite");

        public override string ToString()
        {
            return _value;
        }
    }
}
using System;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    public struct ConflictResolver : IEquatable<ConflictResolver>
    {
        private readonly string _value;

        private ConflictResolver(string value)
        {
            _value = value;
        }

        public static ConflictResolver Rename => new ConflictResolver("rename");
        public static ConflictResolver Rewrite => new ConflictResolver("rewrite");

        public override bool Equals(object obj)
        {
            return obj is ConflictResolver && Equals((ConflictResolver)obj);
        }

        public bool Equals(ConflictResolver other)
        {
            return _value == other._value;
        }

        public override string ToString()
        {
            return _value;
        }

        public static bool operator ==(ConflictResolver resolver1, ConflictResolver resolver2)
        {
            return resolver1.Equals(resolver2);
        }

        public static bool operator !=(ConflictResolver resolver1, ConflictResolver resolver2)
        {
            return !(resolver1 == resolver2);
        }
    }
}
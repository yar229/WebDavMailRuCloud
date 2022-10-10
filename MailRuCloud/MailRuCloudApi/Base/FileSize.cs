using System;
using System.Diagnostics;

namespace YaR.Clouds.Base
{
    /// <summary>
    /// File size definition.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DefaultValue) + "}")]
    public readonly struct FileSize : IEquatable<FileSize>
    {
        public FileSize(long defaultValue) : this()
        {
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Gets default size in bytes.
        /// </summary>
        /// <value>File size.</value>
        public long DefaultValue { get; } //TODO: make it ulong


        #region == Equality ===================================================================================================================
        public static implicit operator FileSize(long defaultValue)
        {
            return new FileSize(defaultValue);
        }

        public static implicit operator long(FileSize fsize)
        {
            return fsize.DefaultValue;
        }

        public static implicit operator ulong(FileSize fsize)
        {
            return (ulong)fsize.DefaultValue;
        }

        public static FileSize operator +(FileSize first, FileSize second)
        {
            return new FileSize(first.DefaultValue + second.DefaultValue);
        }

        public static FileSize operator -(FileSize first, FileSize second)
        {
            return new FileSize(first.DefaultValue - second.DefaultValue);
        }

        public bool Equals(FileSize other)
        {
            return DefaultValue == other.DefaultValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) 
                return false;

            return obj.GetType() == GetType() && Equals((FileSize)obj);
        }

        public override int GetHashCode()
        {
            return DefaultValue.GetHashCode();
        }
        #endregion == Equality ===================================================================================================================
    }
}

using System;
using System.Diagnostics;

namespace YaR.Clouds.Base
{
    /// <summary>
    /// File size definition.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DefaultValue) + "}")]
    public struct FileSize : IEquatable<FileSize>
    {
        public FileSize(long defaultValue) : this()
        {
            _defValue = defaultValue;
        }

        /// <summary>
        /// Private variable for default value.
        /// </summary>
        private readonly long _defValue;

        /// <summary>
        /// Gets default size in bytes.
        /// </summary>
        /// <value>File size.</value>
        public long DefaultValue => _defValue; //TODO: make it ulong


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
            return _defValue == other._defValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != GetType()) return false;
            return Equals((FileSize)obj);
        }

        public override int GetHashCode()
        {
            return _defValue.GetHashCode();
        }
        #endregion == Equality ===================================================================================================================
    }
}

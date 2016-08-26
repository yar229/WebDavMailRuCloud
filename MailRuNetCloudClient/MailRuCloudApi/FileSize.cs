//-----------------------------------------------------------------------
// <created file="FileSize.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

namespace MailRuCloudApi
{
    /// <summary>
    /// The unit metter.
    /// </summary>
    public enum StorageUnit
    {
        /// <summary>
        /// Unit as byte type.
        /// </summary>
        Byte = 0,

        /// <summary>
        /// Unit as kilobyte.
        /// </summary>
        Kb = 1,

        /// <summary>
        /// Unit as megabyte.
        /// </summary>
        Mb = 2,

        /// <summary>
        /// Unit as gigabyte.
        /// </summary>
        Gb = 3
    }

    /// <summary>
    /// File size definition.
    /// </summary>
    public class FileSize
    {
        /// <summary>
        /// Private variable for default value.
        /// </summary>
        private long defValue = 0;

        /// <summary>
        /// Gets default size in bytes.
        /// </summary>
        /// <value>File size.</value>
        public long DefaultValue
        {
            get
            {
                return this.defValue;
            }

            internal set
            {
                this.defValue = value;
                this.SetNormalizedValue();
            }
        }

        /// <summary>
        /// Gets normalized  file size, auto detect storage unit.
        /// </summary>
        /// <value>File size.</value>
        public float NormalizedValue { get; private set; }

        /// <summary>
        /// Gets auto detected storage unit by normalized value.
        /// </summary>
        public StorageUnit NormalizedType { get; private set; }

        /// <summary>
        /// Normalized value detection and auto detection storage unit.
        /// </summary>
        private void SetNormalizedValue()
        {
            if (this.defValue < 1024L)
            {
                this.NormalizedType = StorageUnit.Byte;
                this.NormalizedValue = (float)this.defValue;
            }
            else if (this.defValue >= 1024L && this.defValue < 1024L * 1024L)
            {
                this.NormalizedType = StorageUnit.Kb;
                this.NormalizedValue = (float)this.defValue / 1024f;
            }
            else if (this.defValue >= 1024L * 1024L && this.defValue < 1024L * 1024L * 1024L)
            {
                this.NormalizedType = StorageUnit.Mb;
                this.NormalizedValue = (float)this.defValue / 1024f / 1024f;
            }
            else
            {
                this.NormalizedType = StorageUnit.Gb;
                this.NormalizedValue = (float)this.defValue / 1024f / 1024f / 1024f;
            }
        }
    }
}

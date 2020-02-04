namespace YaR.Clouds.Base.Streams
{
    internal class UploadStream : UploadStreamHttpClient
    {
        public UploadStream(string destinationPath, Cloud cloud, long size) : base(destinationPath, cloud, size)
        {
        }
    }
}

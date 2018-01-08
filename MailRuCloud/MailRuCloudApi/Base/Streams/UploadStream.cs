namespace YaR.MailRuCloud.Api.Base.Streams
{
#if NETCOREAPP2_0

    internal class UploadStream : UploadStreamHttpClient
    {
        public UploadStream(string destinationPath, MailRuCloud cloud, long size) : base(destinationPath, cloud, size)
        {
        }
    }

#elif NET452

    internal class UploadStream : UploadStreamHttpWebRequest
    {
        public UploadStream(string destinationPath, MailRuCloud cloud, long size) : base(destinationPath, cloud, size)
        {
        }
    }

#endif
}

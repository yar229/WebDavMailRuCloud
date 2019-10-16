namespace YaR.MailRuCloud.Api.Base.Streams
{
#if NETCOREAPP3_0

    internal class UploadStream : UploadStreamHttpClient
    {
        public UploadStream(string destinationPath, MailRuCloud cloud, long size) : base(destinationPath, cloud, size)
        {
        }
    }

#elif NET461

    internal class UploadStream : UploadStreamHttpWebRequest
    {
        public UploadStream(string destinationPath, MailRuCloud cloud, long size) : base(destinationPath, cloud, size)
        {
        }
    }

#endif
}

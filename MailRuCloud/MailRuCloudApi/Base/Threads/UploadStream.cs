namespace YaR.MailRuCloud.Api.Base.Threads
{
#if NETCOREAPP2_0

    internal class UploadStream : UploadStreamHttpClient
    {
        public UploadStream(string destinationPath, MailRuCloud cloud, long size) : base(destinationPath, cloud, size)
        {
        }
    }

#elif NET45

    internal class UploadStream : UploadStreamHttpWebRequest
    {
        public UploadStream(string destinationPath, MailRuCloud cloud, long size) : base(destinationPath, cloud, size)
        {
        }
    }

#endif
}

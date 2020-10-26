using System.Net;

namespace YaR.Clouds.Base.Requests.Types
{
    public class UploadFileResult
    {
        public IFileHash Hash { get; set; }
        public long Size{ get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public bool HasReturnedData { get; set; }
        public bool NeedToAddFile { get; set; } = true;
    }
}
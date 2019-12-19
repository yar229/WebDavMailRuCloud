using System.Net;

namespace YaR.Clouds.Base.Requests.Types
{
    public class UploadFileResult
    {
        public string Hash { get; set; }
        public long Size{ get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
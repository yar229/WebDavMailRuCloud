using System;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Streams
{
    public class UploadMultipartBoundary
    {
        public UploadMultipartBoundary(File file)
        {
            _file = file;
        }

        private readonly File _file;

        public Guid Guid = Guid.NewGuid();

        public byte[] Start
        {
            get
            {
                if (null == _start)
                {
                    var boundaryBuilder = new StringBuilder();
                    boundaryBuilder.AppendFormat("------{0}\r\n", Guid);
                    boundaryBuilder.AppendFormat("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\n", Uri.EscapeDataString(_file.Name));
                    boundaryBuilder.AppendFormat("Content-Type: {0}\r\n\r\n", ConstSettings.GetContentType(_file.Extension));

                    _start = Encoding.UTF8.GetBytes(boundaryBuilder.ToString());
                }
                return _start;
            }
        }
        private byte[] _start;

        public byte[] End
        {
            get
            {
                if (null == _end)
                {
                    var endBoundaryBuilder = new StringBuilder();
                    endBoundaryBuilder.AppendFormat("\r\n------{0}--\r\n", Guid);

                    _end = Encoding.UTF8.GetBytes(endBoundaryBuilder.ToString());
                }
                return _end;
            }
        }

        private byte[] _end;
    }
}
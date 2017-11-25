using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class ResponseBodyStream
    {
        private readonly BinaryReader _stream;

        public ResponseBodyStream(Stream stream)
        {
            _stream = new BinaryReader(stream);
            OperationResult = (OperationResult)ReadShort();
        }
        public short ReadShort()
        {
            return (short)(ReadInt() & 255);
        }

        public int ReadInt()
        {
            int b = _stream.ReadInt16();
            if (b == -1)
                throw new Exception("End of stream");
            return b;
        }

        public OperationResult OperationResult { get; }
    }
}

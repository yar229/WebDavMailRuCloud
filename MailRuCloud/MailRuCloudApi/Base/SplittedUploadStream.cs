using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YaR.MailRuCloud.Api.Base
{
    class SplittedUploadStream : Stream
    {
        private readonly string _destinationPath;
        private readonly MailRuCloud _cloud;
        private long _size;
        private readonly bool _checkHash;
        private readonly CryptInfo _cryptInfo;
        private readonly long _maxFileSize;
        private File _origfile;

        private int _currFileId = -1;
        private long _bytesWrote;
        private UploadStream _uploadStream;


        private readonly List<File> _files = new List<File>();
        private bool _performAsSplitted;

        public SplittedUploadStream(string destinationPath, MailRuCloud cloud, long size, bool checkHash = true, CryptInfo cryptInfo = null)
        {
            _destinationPath = destinationPath;
            _cloud = cloud;
            _size = size;
            _checkHash = checkHash;
            _cryptInfo = cryptInfo;

            _maxFileSize = _cloud.CloudApi.Account.Info.FileSizeLimit > 0
                ? _cloud.CloudApi.Account.Info.FileSizeLimit - 1024
                : long.MaxValue - 1024;

            Initialize();
        }

        private void Initialize()
        {
            long allowedSize = _maxFileSize; //TODO: make it right //- BytesCount(_file.Name);
            _performAsSplitted = _size > allowedSize || _cryptInfo != null;
            _origfile = new File(_destinationPath, _size, null);

            if (!_performAsSplitted) // crypted are performed alike splitted file
            {
                _files.Add(_origfile);
            }
            else
            {
                var sinfo = new FilenameServiceInfo
                {
                    CleanName = _origfile.Name,
                    CryptInfo = _cryptInfo,
                    SplitInfo = new FileSplitInfo {IsHeader = false}
                };


                int nfiles = (int) (_size / allowedSize + 1);
                if (nfiles > 999)
                    throw new OverflowException("Cannot upload more than 999 file parts");
                for (int i = 1; i <= nfiles; i++)
                {
                    sinfo.SplitInfo.PartNumber = i;
                    var f = new File($"{_origfile.FullPath}{sinfo}",
                        i != nfiles ? allowedSize : _size % allowedSize,
                        null);
                    _files.Add(f);
                }
            }

            NextFile();
        }

        private void NextFile()
        {
            if (_currFileId >= 0)
                _uploadStream.Dispose();

            _currFileId++;
            if (_currFileId >= _files.Count)
                return;

            _bytesWrote = 0;
            var currFile = _files[_currFileId];
            _uploadStream = new UploadStream(currFile.FullPath, _cloud.CloudApi, currFile.Size) {CheckHashes = _checkHash};
        }


        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            _size = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long diff = _bytesWrote + count - _files[_currFileId].Size;

            if (diff > 0)
            {
                var zbuffer = new byte[count - diff];
                Array.Copy(buffer, 0, zbuffer, 0, count - diff);
                long zcount = count;

                _uploadStream.Write(zbuffer, offset, (int) (zcount - diff));

                NextFile();
            }

            long ncount = diff <= 0 ? count : diff;
            var nbuffer = new byte[ncount];
            Array.Copy(buffer, count - ncount, nbuffer, 0, ncount);

            _bytesWrote += ncount;

            _uploadStream.Write(nbuffer, offset, (int) ncount);
        }

        public event FileUploadedDelegate FileUploaded;

        private void OnFileUploaded(IEnumerable<File> files)
        {
            var e = FileUploaded;
            e?.Invoke(files);
        }

        ~SplittedUploadStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            _uploadStream?.Close();

            if (_performAsSplitted)
            {
                var header = new HeaderFileContent
                {
                    CreationDate = DateTime.Now,
                    Name = _origfile.Name,
                    Size = _origfile.Size,
                    PublicKey = _cryptInfo?.PublicKey
                };
                _cloud.UploadFileJson(_origfile.FullPath, header, true);
            }

            OnFileUploaded(_files);
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _size;
        public override long Position { get; set; }

    }

    public class HeaderFileContent
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public byte[] PublicKey { get; set; }
        public DateTime CreationDate { get; set; }
    }

}

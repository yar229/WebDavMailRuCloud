using System;
using System.IO;
using FileInfo = Fsp.Interop.FileInfo;

namespace YaR.MailRuCloud.Fs
{
    class FileNode
    {
        public FileNode(String fileName)
        {
            FileName = fileName;
            FileInfo.CreationTime =
                FileInfo.LastAccessTime =
                    FileInfo.LastWriteTime =
                        FileInfo.ChangeTime = (UInt64)DateTime.Now.ToFileTimeUtc();
            FileInfo.IndexNumber = _indexNumber++;
        }
        public FileInfo GetFileInfo()
        {
            if (null == MainFileNode)
                return FileInfo;

            FileInfo fileInfo = MainFileNode.FileInfo;
            fileInfo.FileAttributes &= ~(UInt32)FileAttributes.Directory;
            /* named streams cannot be directories */
            fileInfo.AllocationSize = FileInfo.AllocationSize;
            fileInfo.FileSize = FileInfo.FileSize;
            return fileInfo;
        }

        private static UInt64 _indexNumber = 1;
        public String FileName;
        public FileInfo FileInfo;
        public Byte[] FileSecurity;
        public Byte[] FileData;
        public Byte[] ReparseData;
        public FileNode MainFileNode;
        public int OpenCount;
    }
}
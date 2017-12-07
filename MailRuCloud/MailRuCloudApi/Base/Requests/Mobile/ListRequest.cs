using System;
using System.Collections.Generic;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class ListRequest : BaseRequestMobile<ListRequest.Result>
    {
        private readonly string _fullPath;

        public ListRequest(RequestInit init, string metaServer, string fullPath)
            : base(init, metaServer)
        {
            _fullPath = fullPath;
        }


        public long Depth { get; set; } = 1;
        public Option Options { get; set; } = Option.Unknown128 | Option.Unknown256 | Option.Unknown32 | Option.TotalSpace | Option.UsedSpace;
    
        [Flags]
        internal enum Option : Int32
        {
            TotalSpace = 1,
            Delete = 2,
            Fingerprint = 4,
            Unknown8 = 8,
            Unknown16 = 16,
            Unknown32 = 32,
            UsedSpace = 64,
            Unknown128 = 128,
            Unknown256 = 256
        }

        protected override byte[] CreateHttpContent()
        {
            
            using (var stream = new RequestBodyStream())
            {
                stream.WritePu16((byte)Operation.FolderList);
                stream.WriteString(_fullPath);
                stream.WritePu32(Depth);

                stream.WritePu32((int)Options);

                stream.WriteWithLength(new byte[0]);

                var body = stream.GetBytes();
                return body;
            }
        }

        protected override RequestResponse<Result> DeserializeMessage(ResponseBodyStream data)
        {
            if (data.OperationResult != OperationResult.Ok)
                throw new Exception($"{nameof(ListRequest)} failed with result code {data.OperationResult}");

            var res = new Result
            {
                OperationResult = data.OperationResult,
                Revision = Revision.FromStream(data),
                FullPath = _fullPath
            };
            if ((Options & Option.TotalSpace) != 0)
                res.TotalSpace = data.ReadULong();
            if ((Options & Option.UsedSpace) != 0)
                res.UsedSpace = data.ReadULong();

            res.FingerPrint = data.ReadBytesByLength();

            res.Item = GetItem01(data, _fullPath);
            

            return new RequestResponse<Result>
            {
                Ok = data.OperationResult == OperationResult.Ok,
                Result = res
            };
        }

        private FsItem GetItem01(ResponseBodyStream data, string fullPath)
        {
            var folder = new FsFolder(fullPath, null, CloudFolderType.Generic);
            FsFolder rootFolder = folder;
            FsFolder currFolder = null;

            int itemStart = data.ReadShort();
            while (itemStart != 0)
            {
                FsFolder tmpFolder;
                switch (itemStart)
                {
                    case 1:
                        break;
                    case 2:
                        if (currFolder != null)
                        {
                            tmpFolder = currFolder;
                            itemStart = data.ReadShort();
                            continue;
                            //break;
                        }
                        else
                            throw new Exception("lastCreatedFolder = null");
                    case 3:
                        if (rootFolder == folder)
                        {
                            tmpFolder = rootFolder;
                            itemStart = data.ReadShort();
                            continue;
                            //break;
                        }
                        else if (rootFolder.ParentFolder != null)
                        {
                            tmpFolder = rootFolder.ParentFolder;
                            if (tmpFolder == null)
                                throw new Exception("No parent folder");

                            continue;
                        }
                        else
                            throw new Exception("No parent folder");
                    case 15:
                        var skip = data.ReadPu32();
                        for (int i = 0; i < skip; i++)
                        {
                            data.ReadPu32();
                            data.ReadPu32();
                        }
                        break;
                    default:
                        throw new Exception("Unknown itemStart");
                }
                FsItem item = GetItem(data, rootFolder);

                if (item is FsFolder fsFolder) currFolder = fsFolder;
                else currFolder = null;

                rootFolder.Items.Add(item);
                tmpFolder = rootFolder;
                rootFolder = tmpFolder;

                //folder.Items.Add(item);
                itemStart = data.ReadShort();
            }


            return folder;
        }


        private FsItem GetItem(ResponseBodyStream data, FsFolder folder)
        {
            FsItem item = null;
            int head = data.ReadIntSpl();
            byte[] nodeId = null;
            if ((head & 4096) != 0)
            {
                nodeId = data.ReadNBytes(16);
            }
            string name = data.ReadNBytesAsString(data.ReadShort());

            data.ReadULong(); // dunno

            long? l;
            long? l2;

            int opresult = head & 3;
            switch (opresult)
            {
                case 0: // folder?
                    var treeId = data.ReadTreeId();
                    ulong unk1 = data.ReadULong();  // dunno
                    ulong unk2 = data.ReadULong();  // dunno

                    if ((Options & Option.Delete) != 0)
                    {
                        long unk3 = data.ReadPu32();  // dunno
                        long unk4 = data.ReadPu32();  // dunno
                    }

                    if ((Options & Option.Unknown32) != 0)
                    {
                        ulong unk5 = data.ReadULong();  // dunno
                    }

                    item = new FsFolder(WebDavPath.Combine(_fullPath, name), treeId, CloudFolderType.MountPoint);
                    break;

                case 1:
                    var val = data.ReadULong();
                    //var modifDate = data.ReadDate();  // ??? exception
                    var modifDate = DateTime.Now;


                    ulong unk6 = data.ReadULong();  // dunno
                    byte[] sha1 = data.ReadNBytes(20);
                    item = new FsFile(WebDavPath.Combine(_fullPath, name), modifDate, sha1);
                    break;

                case 2:
                    ulong unk7 = data.ReadULong();
                    object z = null;
                    object z1 = null;
                    if ((Options & Option.Delete) != 0)
                    {
                        l = data.ReadPu32();
                        l2 = data.ReadPu32();
                    }
                    ulong? bgVar = null;
                    if ((Options & Option.Unknown32) != 0)
                    {
                        bgVar = data.ReadULong();
                    }
                    item = new FsFolder(WebDavPath.Combine(folder.FullPath, name), null, CloudFolderType.Generic);
                    break;

                case 3:
                    var e = data.ReadULong();
                    treeId = data.ReadTreeId();
                    l = null;
                    l2 = null;
                    if ((Options & Option.Delete) != 0)
                    {
                        l = data.ReadPu32();
                        l2 = data.ReadPu32();
                    }
                    bgVar = null;
                    if ((Options & Option.Unknown32) != 0)
                    {
                        bgVar = data.ReadULong();
                    }
                    item = new FsFolder(WebDavPath.Combine(folder.FullPath, name), null, CloudFolderType.Shared);
                    break;
                default:
                    throw new Exception("unknown opresult " + opresult);

            }

            return item;
        }


        public class Result : RevisionResponseResult
        {
            public string FullPath { get; set; }
            public ulong TotalSpace { get; set; }
            public ulong UsedSpace { get; set; }
            public byte[] FingerPrint { get; set; }

            public FsItem Item { get; set; }
            
        }

        public class FsItem
        {
            public List<FsItem> Items { get; } = new List<FsItem>();
        }

        public enum CloudFolderType
        {
            Generic,
            MountPoint,
            Shared,
            MountPointChild,
            SharedChild
        }

        public class FsFolder : FsItem
        {
            public string FullPath { get; }
            public CloudFolderType Type { get; }
            
            private TreeId _treeId;

            public FsFolder ParentFolder { get; set; }

            public FsFolder(string fullPath, TreeId treeId, CloudFolderType cloudFolderType)
            {
                FullPath = fullPath;
                _treeId = treeId;
                Type = cloudFolderType;
            }
        }

        public class FsFile : FsItem
        {
            public string FullPath { get; }
            private readonly DateTime _modifDate;
            private readonly byte[] _sha1;

            public FsFile(string fullPath, DateTime modifDate, byte[] sha1)
            {
                FullPath = fullPath;
                _modifDate = modifDate;
                _sha1 = sha1;
            }
        }
    }
}
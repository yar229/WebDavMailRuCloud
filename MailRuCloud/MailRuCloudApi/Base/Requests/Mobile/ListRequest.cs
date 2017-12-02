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

        public Option Options { get; set; } = Option.TotalSpace | Option.UsedSpace;
    
        [Flags]
        internal enum Option : Int32
        {
            TotalSpace = 1,
            Unknown2 = 2,
            Unknown4 = 4,
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
                stream.WritePu32(LongAlways1Unknown);

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

            res.Item = GetParent(data);
            

            return new RequestResponse<Result>
            {
                Ok = data.OperationResult == OperationResult.Ok,
                Result = res
            };
        }

        private FsItem GetParent(ResponseBodyStream data)
        {
            var pitem = new FsItem();

            int itemStart = data.ReadShort();
            while (itemStart != 0)
            {
                if (15 == itemStart) //dunno
                {
                    var skip = data.ReadPu32();
                    for (int i = 0; i < skip; i++)
                    {
                        data.ReadPu32();
                        data.ReadPu32();
                    }
                }

                FsItem item = GetItem(data);
                pitem.Items.Add(item);

                itemStart = data.ReadShort();
            }


            return pitem;
        }


        private FsItem GetItem(ResponseBodyStream data)
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

            int opresult = head & 3;
            switch (opresult)
            {
                case 0: // folder?
                    var treeId = data.ReadTreeId();
                    ulong unk1 = data.ReadULong();  // dunno
                    ulong unk2 = data.ReadULong();  // dunno

                    if ((Options & Option.Unknown2) != 0)
                    {
                        long unk3 = data.ReadPu32();  // dunno
                        long unk4 = data.ReadPu32();  // dunno
                    }

                    if ((Options & Option.Unknown32) != 0)
                    {
                        ulong unk5 = data.ReadULong();  // dunno
                    }

                    item = new FsFolder(WebDavPath.Combine(_fullPath, name), treeId);
                    break;

                case 1:
                    var modifDate = data.ReadDate();
                    ulong unk6 = data.ReadULong();  // dunno
                    byte[] sha1 = data.ReadNBytes(20);
                    item = new FsFile(WebDavPath.Combine(_fullPath, name), modifDate, sha1);
                    break;
            }

            return item;
        }

        private const long LongAlways1Unknown = 1;
        

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

        public class FsFolder : FsItem
        {
            private string _fullPath;
            private TreeId _treeId;

            public FsFolder(string fullPath, TreeId treeId)
            {
                _fullPath = fullPath;
                _treeId = treeId;
            }
        }

        public class FsFile : FsItem
        {
            private string _fullPath;
            private readonly DateTime _modifDate;
            private readonly byte[] _sha1;

            public FsFile(string fullPath, DateTime modifDate, byte[] sha1)
            {
                _fullPath = fullPath;
                _modifDate = modifDate;
                _sha1 = sha1;
            }
        }
    }
}
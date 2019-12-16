using System;
using System.Net;
using YaR.MailRuCloud.Api.Base.Repos.MailRuCloud.Mobile.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.MailRuCloud.Mobile.Requests
{
    internal class ListRequest : BaseRequestMobile<ListRequest.Result>
    {
        private readonly string _fullPath;

        public ListRequest(HttpCommonSettings settings, IAuth auth, string metaServer, string fullPath, int depth)
            : base(settings, auth, metaServer)
        {
            _fullPath = fullPath;
	        Depth = depth;
        }

        /// <summary>
        /// Folder list depth
        /// </summary>
        public long Depth { get; set; }

        public Option Options { get; set; } = Option.Unknown128 | Option.Unknown256 | Option.FolderSize | Option.TotalSpace | Option.UsedSpace;
    
        [Flags]
        internal enum Option
        {
            /// <summary>
            /// Request total cloud space
            /// </summary>
            TotalSpace = 1,
            /// <summary>
            /// Dunno, something when delete
            /// </summary>
            Delete = 2,
            Fingerprint = 4,
            Unknown8 = 8,
            Unknown16 = 16,
            /// <summary>
            /// Request folders size
            /// </summary>
            FolderSize = 32,
            /// <summary>
            /// Request used cloud space
            /// </summary>
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
            switch (data.OperationResult)
            {
                    case OperationResult.Ok:
                        break;
                    case OperationResult.NotExists:
                        //throw new NotFoundException($"{nameof(ListRequest)} failed with result code {data.OperationResult}", HttpStatusCode.NotFound);
                        throw new RequestException(_fullPath) {StatusCode = HttpStatusCode.NotFound};
                    default:
                        throw new Exception($"{nameof(ListRequest)} failed with result code {data.OperationResult}");
            }


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
            res.Item = Deserialize(data, _fullPath);

            return new RequestResponse<Result>
            {
                Ok = data.OperationResult == OperationResult.Ok,
                Result = res
            };
        }

	    private enum ParseOp
	    {
		    Done = 0,
		    ReadItem = 1,
		    Pin = 2,
		    PinUpper = 3,
		    Unknown15 = 15
	    }
	    
        private FsItem Deserialize(ResponseBodyStream data, string fullPath)
        {
	        fullPath = WebDavPath.Clean(fullPath);

			var fakeRoot = new FsFolder(WebDavPath.Parent(fullPath), null, CloudFolderType.Generic, null, null);
            FsFolder currentFolder = fakeRoot;
            FsFolder lastFolder = null;
	        int lvl = 0;

            var parseOp = (ParseOp)data.ReadShort();
            while (parseOp != ParseOp.Done)
            {
                switch (parseOp)
                {
                    case ParseOp.ReadItem:
                        break;

                    case ParseOp.Pin:
                        if (lastFolder != null)
                        {
                            currentFolder = lastFolder;
	                        lvl++;
                            parseOp = (ParseOp)data.ReadShort();
                            continue;
                        }
                        else
                            throw new Exception("lastFolder = null");

                    case ParseOp.PinUpper:
                        if (currentFolder == fakeRoot)
                        {
                            parseOp = (ParseOp)data.ReadShort();
                            continue;
                        }
                        else if (currentFolder.Parent != null)
						{
                            currentFolder = currentFolder.Parent;
							lvl--;
							parseOp = (ParseOp)data.ReadShort();
							if (currentFolder == null)
                                throw new Exception("No parent folder A");
                            continue;
                        }
                        else
                            throw new Exception("No parent folder B");

                    case ParseOp.Unknown15:
                        long skip = data.ReadPu32();
                        for (long i = 0; i < skip; i++)
                        {
                            data.ReadPu32();
                            data.ReadPu32();
                        }
                        break;
                    default:
                        throw new Exception("Unknown parse operation");
                }
	            FsItem item = GetItem(data, currentFolder);
                currentFolder.Items.Add(item);

	            if (item is FsFolder fsFolder)
	            {
		            lastFolder = fsFolder;
		            fsFolder.IsChildsLoaded = lvl < Depth;
	            }

                parseOp = (ParseOp)data.ReadShort();
            }

            var res = fakeRoot.Items[0];
            return res;
        }


        private FsItem GetItem(ResponseBodyStream data, FsFolder folder)
        {
            FsItem item;
            TreeId treeId;

            int head = data.ReadIntSpl();
            if ((head & 4096) != 0)
            {
                byte[] nodeId = data.ReadNBytes(16);
            }
            string name = data.ReadNBytesAsString(data.ReadShort());

            data.ReadULong(); // dunno

            ulong? GetFolderSize() => (Options & Option.FolderSize) != 0
                ? (ulong?) data.ReadULong()
                : null;
            void ProcessDelete()
            {
                if ((Options & Option.Delete) != 0)
                {
                    data.ReadPu32();  // dunno
                    data.ReadPu32();  // dunno
                }
            }

            int opresult = head & 3;
            switch (opresult)
            {
                case 0: // folder?
                    treeId = data.ReadTreeId();
                    data.ReadULong();  // dunno
                    data.ReadULong();  // dunno
                    ProcessDelete();

                    item = new FsFolder(WebDavPath.Combine(folder.FullPath, name), treeId, CloudFolderType.MountPoint, folder, GetFolderSize());
                    break;

                case 1:
                    var modifDate = data.ReadDate();
                    ulong size = data.ReadULong();
                    byte[] sha1 = data.ReadNBytes(20);

					//item = new FsFile(WebDavPath.Combine(folder.FullPath == string.Empty ? _fullPath : folder.FullPath, name), modifDate, sha1, size);
	                item = new FsFile(WebDavPath.Combine(folder.FullPath, name), modifDate, sha1, size);
					break;

                case 2:
                    data.ReadULong(); // dunno
                    ProcessDelete();

                    item = new FsFolder(WebDavPath.Combine(folder.FullPath, name), null, CloudFolderType.Generic, folder, GetFolderSize());
                    break;

                case 3:
                    data.ReadULong(); // dunno
                    treeId = data.ReadTreeId();
                    ProcessDelete();

                    item = new FsFolder(WebDavPath.Combine(folder.FullPath, name), treeId, CloudFolderType.Shared, folder, GetFolderSize());
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
    }
}
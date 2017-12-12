using System;

namespace YaR.MailRuCloud.Api.Base.Requests.WebBin.Types
{
    class Revision
    {
        private readonly ulong _newBgn;
        private readonly TreeId _newTreeId;
        private readonly TreeId _treeId;
        private readonly ulong _bgn;

        private Revision()
        {
        }

        private Revision(TreeId treeId, ulong bgn) : this()
        {
            _treeId = treeId;
            _bgn = bgn;
        }

        private Revision(TreeId treeId, ulong bgn, TreeId newTreeId) : this(treeId, bgn)
        {
            _newTreeId = newTreeId;
        }

        private Revision(TreeId treeId, ulong bgn, TreeId newTreeId, ulong newBgn) : this(treeId, bgn, newTreeId)
        {
            _newBgn = newBgn;
        }


        public static Revision FromStream(ResponseBodyStream stream)
        {
            short ver = stream.ReadShort();
            switch (ver)
            {
                case 0:
                    return new Revision();
                case 1:
                    return new Revision(TreeId.FromStream(stream), stream.ReadULong());
                case 2:
                    return new Revision(TreeId.FromStream(stream), stream.ReadULong());
                case 3:
                    return new Revision(TreeId.FromStream(stream), stream.ReadULong(), TreeId.FromStream(stream), stream.ReadULong());
                case 4:
                    return new Revision(TreeId.FromStream(stream), stream.ReadULong(), TreeId.FromStream(stream), stream.ReadULong());
                case 5:
                    return new Revision(TreeId.FromStream(stream), stream.ReadULong(), TreeId.FromStream(stream));

                //more revisions?

                default:
                    throw new Exception("Unknown revision " + ver);
            }
        }
    }
}
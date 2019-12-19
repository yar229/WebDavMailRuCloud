using System;

namespace YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests.Types
{
    class TreeId
    {
        private byte[] _treeId;

        public TreeId(byte[] treeId)
        {
            //if (treeId == null || treeId.Length != 12)
            //    throw new Exception("TreeId must be 12b");
            _treeId = treeId;
        }

        public static TreeId FromStream(ResponseBodyStream stream)
        {
            var buffer = stream.ReadNBytes(12);

            if (null == buffer || buffer.Length != 12)
                throw new Exception("Cannot read TreeId");

            return new TreeId(buffer);
        }

    }
}
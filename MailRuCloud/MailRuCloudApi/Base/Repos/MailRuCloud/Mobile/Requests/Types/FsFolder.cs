namespace YaR.MailRuCloud.Api.Base.Repos.MailRuCloud.Mobile.Requests.Types
{
    internal class FsFolder : FsItem
    {
        public string FullPath { get; }
        public CloudFolderType Type { get; }
            
        private TreeId _treeId;

        public FsFolder Parent { get; }
        public ulong? Size { get; }

        public FsFolder(string fullPath, TreeId treeId, CloudFolderType cloudFolderType, FsFolder parent, ulong? size)
        {
            FullPath = fullPath;
            _treeId = treeId;
            Type = cloudFolderType;
            Parent = parent;
            Size = size;
        }

	    public bool IsChildsLoaded { get; set; }
    }
}
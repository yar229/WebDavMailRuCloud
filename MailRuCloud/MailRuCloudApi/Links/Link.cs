using System;
using System.IO;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Links.Dto;
using File = YaR.MailRuCloud.Api.Base.File;

namespace YaR.MailRuCloud.Api.Links
{
    public class Link : IEntry
    {
        public Link(string href, MailRuCloud.ItemType itemType = MailRuCloud.ItemType.Unknown)
        {
            Href = href;
            IsLinkedToFileSystem = false;
            ItemType = itemType;
        }

        public Link(ItemLink rootLink, string fullPath, string href) : this(href)
        {
            _rootLink = rootLink;
            FullPath = fullPath;

            IsRoot = WebDavPath.PathEquals(WebDavPath.Parent(FullPath), _rootLink.MapTo);

            ItemType = IsRoot
                ? rootLink.IsFile ? MailRuCloud.ItemType.File : MailRuCloud.ItemType.Folder
                : MailRuCloud.ItemType.Unknown;

            Size = IsRoot
                ? rootLink.Size
                : 0;

            CreationTimeUtc = rootLink.CreationDate ?? DateTime.Now;
        }

        public bool IsLinkedToFileSystem { get; }

        private readonly ItemLink _rootLink;

        public string OriginalName { get; set; }

        public string Name => WebDavPath.Name(FullPath);

        public MailRuCloud.ItemType ItemType { get; set; }

        public bool IsFile => ItemType == MailRuCloud.ItemType.File;

        public bool IsBad { get; set; }

        public bool IsResolved { get; set; }

        /// <summary>
        /// Filesystem full path from root
        /// </summary>
        public string FullPath { get; }

        public string MapPath => _rootLink.MapTo;

        public bool IsRoot { get; }

        public IEntry ToBadEntry()
        {
            var res = ItemType == MailRuCloud.ItemType.File
                ? (IEntry)new File(FullPath, Size, string.Empty)
                : new Folder(Size, FullPath, string.Empty);

            return res;
        }

        public string Href { get; }

        public string PublicLink => Href;
        public FileAttributes Attributes => FileAttributes.Normal; //TODO: dunno what to do

        public FileSize Size { get; set; }
        public DateTime CreationTimeUtc { get; set; }

    }
}
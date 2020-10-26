using System;
using System.Collections.Generic;
using System.IO;
using YaR.Clouds.Base;
using YaR.Clouds.Links.Dto;
using File = YaR.Clouds.Base.File;

namespace YaR.Clouds.Links
{
    public class Link : IEntry
    {
        public Link(Uri href, Cloud.ItemType itemType = Cloud.ItemType.Unknown)
        {
            Href = href.IsAbsoluteUri ? href : throw new ArgumentException("Absolute uri required");
            IsLinkedToFileSystem = false;
            ItemType = itemType;
        }

        public Link(ItemLink rootLink, string fullPath, Uri href) : this(href)
        {
            _rootLink = rootLink;
            FullPath = fullPath;

            IsRoot = WebDavPath.PathEquals(WebDavPath.Parent(FullPath), _rootLink.MapTo);

            ItemType = IsRoot
                ? rootLink.IsFile ? Cloud.ItemType.File : Cloud.ItemType.Folder
                : Cloud.ItemType.Unknown;

            Size = IsRoot
                ? rootLink.Size
                : 0;

            CreationTimeUtc = rootLink.CreationDate ?? DateTime.Now;
        }

        public bool IsLinkedToFileSystem { get; }

        private readonly ItemLink _rootLink;

        public string OriginalName { get; set; }

        public string Name => WebDavPath.Name(FullPath);

        public Cloud.ItemType ItemType { get; set; }

        public bool IsFile => ItemType == Cloud.ItemType.File;

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
            var res = ItemType == Cloud.ItemType.File
                ? (IEntry)new File(FullPath, Size, null)
                : new Folder(Size, FullPath);

            return res;
        }


        public Uri Href { get; }
        public List<PublicLinkInfo> PublicLinks => new List<PublicLinkInfo> {new PublicLinkInfo("linked", Href) };


        public FileAttributes Attributes => FileAttributes.Normal; //TODO: dunno what to do

        public FileSize Size { get; set; }
        public DateTime CreationTimeUtc { get; set; }

    }
}
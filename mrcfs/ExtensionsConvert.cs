using System;
using YaR.MailRuCloud.Api.Base;

namespace YaR.MailRuCloud.Fs
{
    static class ExtensionsConvert
    {
        public static FileNode ToFileNode(this Folder folder)
        {
            throw new NotImplementedException("Folder -> FileNode");
        }

        public static FileNode ToFileNode(this IEntry entry)
        {
            throw new NotImplementedException("IEntry -> FileNode");
        }

    }
}

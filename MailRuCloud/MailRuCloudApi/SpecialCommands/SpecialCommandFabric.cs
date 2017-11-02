using System;
using YaR.MailRuCloud.Api.Base;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class SpecialCommandFabric
    {
        public SpecialCommand Build(MailRuCloud cloud, string param)
        {
            if (null == param || !param.Contains("/>>"))
                return null;

            //TODO: refact this sheet
            int pos = param.LastIndexOf("/>>", StringComparison.Ordinal);
            string path = WebDavPath.Clean(param.Substring(0, pos + 1));
            string data = param.Substring(pos + 3);

            if (data.StartsWith("link ")) return new SharedFolderLinkCommand(cloud, path, data.Remove(0, 5));
            if (data.StartsWith("del ")) return new DeleteCommand(cloud, path, data.Remove(0, 4));
            if (data == "del") return new DeleteCommand(cloud, path, data.Remove(0, 3));

            return new SharedFolderJoinCommand(cloud, path, data);
        }
    }
}
using System;

namespace YaR.MailRuCloud.Api.Base
{
    public interface IEntry
    {
        bool IsFile { get; }
        FileSize Size { get; }
        string Name { get; }
        DateTime CreationTimeUtc { get; }
    }
}
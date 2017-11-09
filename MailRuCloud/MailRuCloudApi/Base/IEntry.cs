using System;

namespace YaR.MailRuCloud.Api.Base
{
    public interface IEntry
    {
        bool IsFile { get; }
        FileSize Size { get; }
        string Name { get; }
        string FullPath { get; }
        DateTime CreationTimeUtc { get; }
    }
}
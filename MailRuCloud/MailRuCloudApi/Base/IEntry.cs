using System;
using System.Collections.Generic;

namespace YaR.Clouds.Base
{
    public interface IEntry
    {
        bool IsFile { get; }
        FileSize Size { get; }
        string Name { get; }
        string FullPath { get; }
        DateTime CreationTimeUtc { get; }
        List<PublicLinkInfo> PublicLinks { get; }
    }
}
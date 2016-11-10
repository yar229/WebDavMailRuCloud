using System;

namespace NWebDav.Server.Stores
{
    public interface IMailruStoreItem : IStoreItem
    {
        bool IsWritable { get; }
        string FullPath { get; }
    }
}

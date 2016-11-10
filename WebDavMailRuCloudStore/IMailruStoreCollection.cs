using System;

namespace NWebDav.Server.Stores
{
    public interface IMailruStoreCollection : IStoreCollection
    {
        bool IsWritable { get; }
        string FullPath { get; }
    }
}

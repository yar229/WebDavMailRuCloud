using NWebDav.Server.Stores;

namespace YaR.WebDavMailRu.CloudStore.Mailru.StoreBase
{
    public interface IMailruStoreCollection : IStoreCollection
    {
        bool IsWritable { get; }
        string FullPath { get; }
    }
}

using NWebDav.Server.Stores;

namespace YaR.WebDavMailRu.CloudStore.Mailru.StoreBase
{
    public interface IMailruStoreItem : IStoreItem
    {
        bool IsWritable { get; }
        string FullPath { get; }
    }
}

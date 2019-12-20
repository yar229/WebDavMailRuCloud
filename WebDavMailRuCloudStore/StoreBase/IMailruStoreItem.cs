using NWebDav.Server.Stores;
using YaR.Clouds.Base;

namespace YaR.Clouds.WebDavStore.StoreBase
{
    public interface IMailruStoreItem : IStoreItem
    {
        bool IsWritable { get; }
        string FullPath { get; }

        IEntry EntryInfo { get; }
        long Length { get;}
        bool IsReadable { get;}
    }
}

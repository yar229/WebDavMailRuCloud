using NWebDav.Server.Stores;
using YaR.MailRuCloud.Api.Base;

namespace YaR.WebDavMailRu.CloudStore.Mailru.StoreBase
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

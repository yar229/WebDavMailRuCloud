using System;
using System.Text;
using System.Threading.Tasks;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;
using YaR.WebDavMailRu.CloudStore.Mailru.StoreBase;

namespace YaR.WebDavMailRu.CloudStore
{
    internal static class Extensions
    {
        public static Task<bool> Remove(this MailRuCloud.Api.MailRuCloud cloud, IStoreItem item)
        {
            if (null == item) return Task.FromResult(false);

            if (item is MailruStoreItem storeItem)
                return cloud.Remove(storeItem.FileInfo);
            if (item is MailruStoreCollection storeCollection)
                return cloud.Remove(storeCollection.DirectoryInfo);

            throw new ArgumentException(string.Empty, nameof(item));
        }

        public static Task<bool> Rename(this MailRuCloud.Api.MailRuCloud cloud, IStoreItem item, string destinationName)
        {
            if (null == item) return Task.FromResult(false);

            if (item is MailruStoreItem storeItem)
                return cloud.Rename(storeItem.FileInfo, destinationName);
            if (item is MailruStoreCollection storeCollection)
                return cloud.Rename(storeCollection.DirectoryInfo, destinationName);

            throw new ArgumentException(string.Empty, nameof(item));
        }

        public static Task<bool> Move(this MailRuCloud.Api.MailRuCloud cloud, IStoreItem item, string destinationName)
        {
            if (null == item) return Task.FromResult(false);

            if (item is MailruStoreItem storeItem)
                return cloud.Move(storeItem.FileInfo, destinationName);
            if (item is MailruStoreCollection storeCollection)
                return cloud.Move(storeCollection.DirectoryInfo, destinationName);

            throw new ArgumentException(string.Empty, nameof(item));
        }

        public static string GetFullPath(this IStoreItem item)
        {
            if (null == item) return string.Empty;

            if (item is MailruStoreItem storeItem)
                return storeItem.FullPath;
            if (item is MailruStoreCollection storeCollection)
                return storeCollection.FullPath;

            throw new ArgumentException(string.Empty, nameof(item));
        }


        public static long ContentLength(this IHttpRequest request)
        {
            long.TryParse(request.GetHeaderValue("Content-Length"), out var res);
            return res;
        }

        public static long BytesCount(this string value)
        {
            return Encoding.UTF8.GetByteCount(value);
        }
    }
}

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
        public static async Task<bool> Remove(this MailRuCloud.Api.MailRuCloud cloud, IStoreItem item)
        {
            if (null == item) return await Task.FromResult(false);

            if (item is MailruStoreItem storeItem)
                return await cloud.Remove(storeItem.FileInfo);
            if (item is MailruStoreCollection storeCollection)
                return await cloud.Remove(storeCollection.DirectoryInfo);

            throw new ArgumentException(string.Empty, nameof(item));
        }

        public static Task<bool> Rename(this MailRuCloud.Api.MailRuCloud cloud, IStoreItem item, string destinationName)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(destinationName)) throw new ArgumentNullException(nameof(destinationName));
            if (!(item is IMailruStoreItem)) throw new ArgumentException($"{nameof(IMailruStoreItem)} required.", nameof(item));

            return cloud.Rename(((IMailruStoreItem)item).EntryInfo, destinationName);
        }

        public static Task<bool> Move(this MailRuCloud.Api.MailRuCloud cloud, IStoreItem item, string destinationPath)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(destinationPath)) throw new ArgumentNullException(nameof(destinationPath));
            if (!(item is IMailruStoreItem)) throw new ArgumentException($"{nameof(IMailruStoreItem)} required.", nameof(item));

            return cloud.MoveAsync(((IMailruStoreItem)item).EntryInfo, destinationPath);
        }
        public static string GetFullPath(this IStoreItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!(item is IMailruStoreItem)) throw new ArgumentException($"{nameof(IMailruStoreItem)} required.", nameof(item));

            return ((IMailruStoreItem)item).FullPath;
        }


        public static long ContentLength(this IHttpRequest request)
        {
            long.TryParse(request.GetHeaderValue("Content-Length"), out var res);
            return res;
        }

        //public static long BytesCount(this string value)
        //{
        //    return Encoding.UTF8.GetByteCount(value);
        //}
    }
}

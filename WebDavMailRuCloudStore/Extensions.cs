using System;
using System.Threading.Tasks;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;
using YaR.Clouds.WebDavStore.StoreBase;

namespace YaR.Clouds.WebDavStore
{
    internal static class Extensions
    {
        public static async Task<bool> Remove(this Cloud cloud, IStoreItem item)
        {
            return item switch
            {
                null => await Task.FromResult(false),
                LocalStoreItem storeItem => await cloud.Remove(storeItem.FileInfo),
                LocalStoreCollection storeCollection => await cloud.Remove(storeCollection.DirectoryInfo),
                _ => throw new ArgumentException(string.Empty, nameof(item))
            };
        }

        public static Task<bool> Rename(this Cloud cloud, IStoreItem item, string destinationName)
        {
            if (item == null) 
                throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(destinationName)) 
                throw new ArgumentNullException(nameof(destinationName));
            if (!(item is ILocalStoreItem storeItem)) 
                throw new ArgumentException($"{nameof(ILocalStoreItem)} required.", nameof(item));

            return cloud.Rename(storeItem.EntryInfo, destinationName);
        }

        public static Task<bool> Move(this Cloud cloud, IStoreItem item, string destinationPath)
        {
            if (item == null) 
                throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(destinationPath)) 
                throw new ArgumentNullException(nameof(destinationPath));
            if (!(item is ILocalStoreItem storeItem)) 
                throw new ArgumentException($"{nameof(ILocalStoreItem)} required.", nameof(item));

            return cloud.MoveAsync(storeItem.EntryInfo, destinationPath);
        }
        public static string GetFullPath(this IStoreItem item)
        {
            if (item == null) 
                throw new ArgumentNullException(nameof(item));
            if (!(item is ILocalStoreItem storeItem)) 
                throw new ArgumentException($"{nameof(ILocalStoreItem)} required.", nameof(item));

            return storeItem.FullPath;
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

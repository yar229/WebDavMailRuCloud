using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NWebDav.Server;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;
using YaR.MailRuCloud.Api.Base;

namespace YaR.WebDavMailRu.CloudStore.Mailru.StoreBase
{
    public sealed class MailruStore : IStore
    {
        public MailruStore(bool isWritable = true, ILockingManager lockingManager = null)
        {
            LockingManager = lockingManager ?? new InMemoryLockingManager();
            IsWritable = isWritable;
        }

        private bool IsWritable { get; }
        private ILockingManager LockingManager { get; }

        public Task<IStoreItem> GetItemAsync(WebDavUri uri, IHttpContext httpContext)
        {
            var identity = (HttpListenerBasicIdentity)httpContext.Session.Principal.Identity;
            var path = uri.Path;
            
            try
            {
                var item = CloudManager.Instance(identity).GetItem(path).Result;
                if (item != null)
                {
                    return item.IsFile
                        ? Task.FromResult<IStoreItem>(new MailruStoreItem(LockingManager, (File)item, IsWritable))
                        : Task.FromResult<IStoreItem>(new MailruStoreCollection(httpContext, LockingManager, (Folder)item, IsWritable));
                }
            }
            //catch (AggregateException e)
            //{
            //    var we = e.InnerExceptions.OfType<WebException>().FirstOrDefault();
            //    if (we == null || we.Status != WebExceptionStatus.ProtocolError) throw;
            //}


            // ReSharper disable once RedundantCatchClause
            #if DEBUG
            #pragma warning disable 168
            catch (Exception ex)
            {
                throw;
            }
            #pragma warning restore 168
            #endif

            return Task.FromResult<IStoreItem>(null);
        }

        public Task<IStoreCollection> GetCollectionAsync(WebDavUri uri, IHttpContext httpContext)
        {
            var path = uri.Path;

            var folder = (Folder)CloudManager.Instance(httpContext.Session.Principal.Identity)
                .GetItem(path, MailRuCloud.Api.MailRuCloud.ItemType.Folder).Result;

            return Task.FromResult<IStoreCollection>(new MailruStoreCollection(httpContext, LockingManager, folder, IsWritable));
        }
    }
}

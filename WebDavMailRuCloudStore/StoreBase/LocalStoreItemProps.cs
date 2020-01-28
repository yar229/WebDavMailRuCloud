using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NWebDav.Server;
using NWebDav.Server.Props;
using YaR.Clouds.WebDavStore.CustomProperties;

namespace YaR.Clouds.WebDavStore.StoreBase
{
    class LocalStoreItemProps<T> where T : LocalStoreItem
    {
        public LocalStoreItemProps(Func<string, bool> isEnabledPropFunc)
        {
            var props = new DavProperty<T>[]
            {
                new DavIsreadonly<T>
                {
                    Getter = (context, item) => !item.IsWritable
                },

                // RFC-2518 properties
                new DavCreationDate<T>
                {
                    Getter = (context, item) => item.FileInfo.CreationTimeUtc,
                    Setter = (context, item, value) =>
                    {
                        item.FileInfo.CreationTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },
                new DavDisplayName<T>
                {
                    Getter = (context, item) => item.FileInfo.Name
                },
                new DavGetContentLength<T>
                {
                    Getter = (context, item) => item.FileInfo.Size
                },
                new DavGetContentType<T>
                {
                    Getter = (context, item) => item.DetermineContentType()
                },
                new DavGetEtag<T>
                {
                    // Calculating the Etag is an expensive operation,
                    // because we need to scan the entire file.
                    IsExpensive = true,
                    Getter = (context, item) => item.CalculateEtag()
                },
                new DavGetLastModified<T>
                {
                    Getter = (context, item) => item.FileInfo.LastWriteTimeUtc,
                    Setter = (context, item, value) =>
                    {
                        //item._fileInfo.LastWriteTimeUtc = value;

                        var cloud = CloudManager.Instance((HttpListenerBasicIdentity)context.Session.Principal.Identity);
                        bool res = cloud.SetFileDateTime(item.FileInfo, value).Result;
                        return res
                            ? DavStatusCode.Ok
                            : DavStatusCode.InternalServerError;
                    }
                },

                new DavLastAccessed<T>
                {
                    Getter = (context, collection) => collection.FileInfo.LastWriteTimeUtc,
                    Setter = (context, collection, value) =>
                    {
                        collection.FileInfo.LastWriteTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },

                new DavGetResourceType<T>
                {
                    Getter = (context, item) => null
                },

                // Default locking property handling via the LockingManager
                new DavLockDiscoveryDefault<T>(),
                new DavSupportedLockDefault<T>(),

                // Hopmann/Lippert collection properties
                // (although not a collection, the IsHidden property might be valuable)
                new DavExtCollectionIsHidden<T>
                {
                    Getter = (context, item) => false //(item._fileInfo.Attributes & FileAttributes.Hidden) != 0
                },

                // Win32 extensions
                new Win32CreationTime<T>
                {
                    Getter = (context, item) => item.FileInfo.CreationTimeUtc,
                    Setter = (context, item, value) =>
                    {
                        //item._fileInfo.CreationTimeUtc = value;

                        var cloud = CloudManager.Instance((HttpListenerBasicIdentity)context.Session.Principal.Identity);
                        bool res = cloud.SetFileDateTime(item.FileInfo, value).Result;
                        return res
                            ? DavStatusCode.Ok
                            : DavStatusCode.InternalServerError;
                    }
                },
                new Win32LastAccessTime<T>
                {
                    Getter = (context, item) => item.FileInfo.LastAccessTimeUtc,
                    Setter = (context, item, value) =>
                    {
                        item.FileInfo.LastAccessTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },
                new Win32LastModifiedTime<T>
                {
                    Getter = (context, item) => item.FileInfo.LastWriteTimeUtc,
                    Setter = (context, item, value) =>
                    {
                        item.FileInfo.LastWriteTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },
                new Win32FileAttributes<T>
                {
                    Getter = (context, item) => FileAttributes.Normal, //item._fileInfo.Attributes,
                    Setter = (context, item, value) => DavStatusCode.Ok
                },
                new DavSharedLink<T>
                {
                    Getter = (context, item) => !item.FileInfo.PublicLinks.Any() 
                        ? string.Empty
                        : item.FileInfo.PublicLinks.First().Uri.OriginalString,
                    Setter = (context, item, value) => DavStatusCode.Ok
                }
            };

            _props = props.Where(p => isEnabledPropFunc?.Invoke(p.Name.LocalName) ?? true).ToArray();
        }

        public IEnumerable<DavProperty<T>> Props => _props;
        private readonly DavProperty<T>[]  _props;
    }
}
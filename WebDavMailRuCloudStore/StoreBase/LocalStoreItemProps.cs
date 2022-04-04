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
                    Getter = (_, item) => !item.IsWritable
                },

                // RFC-2518 properties
                new DavCreationDate<T>
                {
                    Getter = (_, item) => item.FileInfo.CreationTimeUtc,
                    Setter = (_, item, value) =>
                    {
                        item.FileInfo.CreationTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },
                new DavDisplayName<T>
                {
                    Getter = (_, item) => item.FileInfo.Name
                },
                new DavGetContentLength<T>
                {
                    Getter = (_, item) => item.FileInfo.Size
                },
                new DavGetContentType<T>
                {
                    Getter = (_, item) => item.DetermineContentType()
                },
                new DavGetEtag<T>
                {
                    // Calculating the Etag is an expensive operation,
                    // because we need to scan the entire file.
                    IsExpensive = true,
                    Getter = (_, item) => item.CalculateEtag()
                },
                new DavGetLastModified<T>
                {
                    Getter = (_, item) => item.FileInfo.LastWriteTimeUtc,
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
                    Getter = (_, collection) => collection.FileInfo.LastWriteTimeUtc,
                    Setter = (_, collection, value) =>
                    {
                        collection.FileInfo.LastWriteTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },

                new DavGetResourceType<T>
                {
                    Getter = (_, _) => null
                },

                // Default locking property handling via the LockingManager
                new DavLockDiscoveryDefault<T>(),
                new DavSupportedLockDefault<T>(),

                // Hopmann/Lippert collection properties
                // (although not a collection, the IsHidden property might be valuable)
                new DavExtCollectionIsHidden<T>
                {
                    Getter = (_, _) => false //(item._fileInfo.Attributes & FileAttributes.Hidden) != 0
                },

                // Win32 extensions
                new Win32CreationTime<T>
                {
                    Getter = (_, item) => item.FileInfo.CreationTimeUtc,
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
                    Getter = (_, item) => item.FileInfo.LastAccessTimeUtc,
                    Setter = (_, item, value) =>
                    {
                        item.FileInfo.LastAccessTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },
                new Win32LastModifiedTime<T>
                {
                    Getter = (_, item) => item.FileInfo.LastWriteTimeUtc,
                    Setter = (_, item, value) =>
                    {
                        item.FileInfo.LastWriteTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },
                new Win32FileAttributes<T>
                {
                    Getter = (_, _) => FileAttributes.Normal, //item._fileInfo.Attributes,
                    Setter = (_, _, _) => DavStatusCode.Ok
                },
                new DavSharedLink<T>
                {
                    Getter = (_, item) => !item.FileInfo.PublicLinks.Any() 
                        ? string.Empty
                        : item.FileInfo.PublicLinks.First().Uri.OriginalString,
                    Setter = (_, _, _) => DavStatusCode.Ok
                }
            };

            _props = props.Where(p => isEnabledPropFunc?.Invoke(p.Name.ToString()) ?? true).ToArray();
        }

        public IEnumerable<DavProperty<T>> Props => _props;
        private readonly DavProperty<T>[]  _props;
    }
}
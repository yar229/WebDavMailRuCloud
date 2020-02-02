using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Props;
using YaR.Clouds.WebDavStore.CustomProperties;

namespace YaR.Clouds.WebDavStore.StoreBase
{
    class LocalStoreCollectionProps<T> where T : LocalStoreCollection
    {
        private static readonly XElement SxDavCollection = new XElement(WebDavNamespaces.DavNs + "collection");

        public LocalStoreCollectionProps(Func<string, bool> isEnabledPropFunc)
        {
            var props = new DavProperty<T>[]
            {
                //// was added to to make WebDrive work, but no success
                //new DavHref<LocalStoreCollection>
                //{
                //    Getter = (context, collection) => collection._directoryInfo.Name
                //},

                //new DavLoctoken<LocalStoreCollection>
                //{
                //    Getter = (context, collection) => ""
                //},

                // collection property required for WebDrive
                new DavCollection<T>
                {
                    Getter = (cntext, collection) => string.Empty
                },

                new DavGetEtag<T>
                {
                    Getter = (cntext, item) => item.CalculateEtag()
                },

                //new DavBsiisreadonly<LocalStoreCollection>
                //{
                //    Getter = (context, item) => false
                //},

                //new DavSrtfileattributes<LocalStoreCollection>
                //{
                //    Getter = (context, collection) =>  collection.DirectoryInfo.Attributes,
                //    Setter = (context, collection, value) =>
                //    {
                //        collection.DirectoryInfo.Attributes = value;
                //        return DavStatusCode.Ok;
                //    }
                //},
                ////====================================================================================================
            

                new DavIsreadonly<T>
                {
                    Getter = (cntext, item) => !item.IsWritable
                },

                new DavQuotaAvailableBytes<T>
                {
                    Getter = (cntext, collection) => collection.FullPath == "/" ? CloudManager.Instance(cntext.Session.Principal.Identity).GetDiskUsage().Free.DefaultValue : long.MaxValue,
                    IsExpensive = true  //folder listing performance
                },

                new DavQuotaUsedBytes<T>
                {
                    Getter = (cntext, collection) => 
                        collection.DirectoryInfo.Size
                    //IsExpensive = true  //folder listing performance
                },

                // RFC-2518 properties
                new DavCreationDate<T>
                {
                    Getter = (cntext, collection) => collection._directoryInfo.CreationTimeUtc,
                    Setter = (cntext, collection, value) =>
                    {
                        collection._directoryInfo.CreationTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },
                new DavDisplayName<T>
                {
                    Getter = (cntext, collection) => collection._directoryInfo.Name
                },
                new DavGetLastModified<T>
                {
                    Getter = (cntext, collection) => collection._directoryInfo.LastWriteTimeUtc,
                    Setter = (cntext, collection, value) =>
                    {
                        collection._directoryInfo.LastWriteTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },

                new DavLastAccessed<T>
                {
                    Getter = (cntext, collection) => collection._directoryInfo.LastWriteTimeUtc,
                    Setter = (cntext, collection, value) =>
                    {
                        collection._directoryInfo.LastWriteTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },


                //new DavGetResourceType<LocalStoreCollection>
                //{
                //    Getter = (context, collection) => new XElement(WebDavNamespaces.DavNs + "collection")
                //},
                new DavGetResourceType<T>
                {
                    Getter = (cntext, collection) => new []{SxDavCollection}
                },


                // Default locking property handling via the LockingManager
                new DavLockDiscoveryDefault<T>(),
                new DavSupportedLockDefault<T>(),

                //Hopmann/Lippert collection properties
                new DavExtCollectionChildCount<T>
                {
                    Getter = (cntext, collection) =>
                    {
                        int files = collection.DirectoryInfo.NumberOfFiles;
                        int folders = collection.DirectoryInfo.NumberOfFolders;
                        return folders > 0 ? folders : collection.DirectoryInfo.ServerFoldersCount +
                               files > 0 ? files : collection.DirectoryInfo.ServerFilesCount ?? 0;
                    }
                },
                new DavExtCollectionIsFolder<T>
                {
                    Getter = (cntext, collection) => true
                },
                new DavExtCollectionIsHidden<T>
                {
                    Getter = (cntext, collection) => false
                },
                new DavExtCollectionIsStructuredDocument<T>
                {
                    Getter = (cntext, collection) => false
                },

                new DavExtCollectionHasSubs<T> //Identifies whether this collection contains any collections which are folders (see "isfolder").
                {
                    Getter = (cntext, collection) => collection.DirectoryInfo.NumberOfFolders > 0 || collection.DirectoryInfo.ServerFoldersCount > 0
                },

                new DavExtCollectionNoSubs<T> //Identifies whether this collection allows child collections to be created.
                {
                    Getter = (cntext, collection) => false
                },

                new DavExtCollectionObjectCount<T> //To count the number of non-folder resources in the collection.
                {
                    Getter = (cntext, collection) => 
                        collection.DirectoryInfo.NumberOfFiles > 0
                            ? collection.DirectoryInfo.NumberOfFiles
                            : collection.DirectoryInfo.ServerFilesCount ?? 0
                },

                new DavExtCollectionReserved<T>
                {
                    Getter = (cntext, collection) => !collection.IsWritable
                },

                new DavExtCollectionVisibleCount<T>  //Counts the number of visible non-folder resources in the collection.
                {
                    Getter = (cntext, collection) => 
                        collection.DirectoryInfo.NumberOfFiles > 0
                            ? collection.DirectoryInfo.NumberOfFiles
                            : collection.DirectoryInfo.ServerFilesCount ?? 0
                },

                // Win32 extensions
                new Win32CreationTime<T>
                {
                    Getter = (cntext, collection) => collection._directoryInfo.CreationTimeUtc,
                    Setter = (cntext, collection, value) =>
                    {
                        collection.DirectoryInfo.CreationTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },
                new Win32LastAccessTime<T>
                {
                    Getter = (cntext, collection) => collection.DirectoryInfo.LastAccessTimeUtc,
                    Setter = (cntext, collection, value) =>
                    {
                        collection._directoryInfo.LastAccessTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },
                new Win32LastModifiedTime<T>
                {
                    Getter = (cntext, collection) => collection.DirectoryInfo.LastWriteTimeUtc,
                    Setter = (cntext, collection, value) =>
                    {
                        collection.DirectoryInfo.LastWriteTimeUtc = value;
                        return DavStatusCode.Ok;
                    }
                },
                new Win32FileAttributes<T>
                {
                    Getter = (cntext, collection) =>  collection.DirectoryInfo.Attributes,
                    Setter = (cntext, collection, value) =>
                    {
                        collection.DirectoryInfo.Attributes = value;
                        return DavStatusCode.Ok;
                    }
                },
                new DavGetContentLength<T>
                {
                    Getter = (cntext, item) => item.DirectoryInfo.Size
                },
                new DavGetContentType<T>
                {
                    Getter = (cntext, item) => "httpd/unix-directory" //"application/octet-stream"
                },
                new DavSharedLink<T>
                {
                    Getter = (cntext, item) => !item.DirectoryInfo.PublicLinks.Any()
                        ? string.Empty
                        : item.DirectoryInfo.PublicLinks.First().Uri.OriginalString,
                    Setter = (cntext, item, value) => DavStatusCode.Ok
                }
            };

            _props = props.Where(p => isEnabledPropFunc?.Invoke(p.Name.ToString()) ?? true).ToArray();
        }

        public IEnumerable<DavProperty<T>> Props => _props;
        private readonly DavProperty<T>[]  _props;
    }
}
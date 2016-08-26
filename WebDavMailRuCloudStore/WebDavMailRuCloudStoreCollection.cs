using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using MailRuCloudApi;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores;


namespace WebDavMailRuCloudStore
{
    /// <summary>
    /// This class implements a disk-based <see cref="IWebDavStore" /> that maps to a folder on disk.
    /// </summary>
    [DebuggerDisplay("Directory ({Name})")]
    public sealed class WebDavMailRuCloudStoreCollection : WebDavMailRuCloudStoreItem, IWebDavStoreCollection
    {
        private readonly MailRuCloud _cloud;
        public MailRuCloud Cloud => _cloud;
        private readonly Dictionary<string, WeakReference> _items = new Dictionary<string, WeakReference>();

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDAVSharp.Server.Stores.DiskStore.WebDavDiskStoreCollection" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent <see cref="WebDAVSharp.Server.Stores.DiskStore.WebDavDiskStoreCollection" /> that contains this <see cref="WebDAVSharp.Server.Stores.DiskStore.WebDavDiskStoreCollection" />.</param>
        /// <param name="path">The path to the folder on this that this <see cref="WebDAVSharp.Server.Stores.DiskStore.WebDavDiskStoreCollection" /> maps to.</param>
        public WebDavMailRuCloudStoreCollection(MailRuCloud cloud, WebDavMailRuCloudStoreCollection parentCollection, string path)
            : base(parentCollection, path)
        {
            _cloud = cloud;
        }

        #region IWebDAVStoreCollection Members

        /// <summary>
        /// Gets a collection of all the items in this <see cref="IWebDavStoreCollection" />.
        /// </summary>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnauthorizedException">If the user is unauthorized or doesn't have access</exception>
        public IEnumerable<IWebDavStoreItem> Items
        {
            get
            {
                HashSet<WeakReference> toDelete = new HashSet<WeakReference>(_items.Values);
                List<IWebDavStoreItem> items = new List<IWebDavStoreItem>();

                // TODO: Refactor to get rid of duplicate loop code
                List<string> directories = new List<string>();
                try
                {
                    // Impersonate the current user and get all the directories
                    using (Identity.Impersonate())
                    {
                        foreach (var dir in _cloud.GetItems(ItemPath).Result.Folders)
                        {
                            try
                            {
                                directories.Add(dir.FulPath);
                            }
                            catch (Exception)
                            {
                                // do nothing
                            }
                        }
                    }
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }
                foreach (string subDirectoryPath in directories)
                {
                    string name = Path.GetFileName(subDirectoryPath);
                    WebDavMailRuCloudStoreCollection collection = null;

                    WeakReference wr;
                    if (_items.TryGetValue(name, out wr))
                    {
                        collection = wr.Target as WebDavMailRuCloudStoreCollection;
                        if (collection == null)
                            toDelete.Remove(wr);
                    }

                    if (collection == null)
                    {
                        collection = new WebDavMailRuCloudStoreCollection(_cloud, this, subDirectoryPath);
                        _items[name] = new WeakReference(collection);
                    }

                    items.Add(collection);
                }
                var files = new List<MailRuCloudApi.File>();
                try
                {
                    // Impersonate the current user and get all the files
                    using (Identity.Impersonate())
                    {
                        files.AddRange(_cloud.GetItems(ItemPath).Result.Files);
                    }
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }
                foreach (var fle in files)
                {
                    string name = Path.GetFileName(fle.FulPath);
                    WebDavMailRuCloudStoreDocument document = null;

                    WeakReference wr;
                    if (_items.TryGetValue(name, out wr))
                    {
                        document = wr.Target as WebDavMailRuCloudStoreDocument;
                        if (document == null)
                            toDelete.Remove(wr);
                    }

                    if (document == null)
                    {
                        document = new WebDavMailRuCloudStoreDocument(this, fle);
                        _items[name] = new WeakReference(document);
                    }
                    items.Add(document);
                }
                return items.ToArray();
            }
        }

        /// <summary>
        /// Checks if the user has access to the path
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// True if access, false if not
        /// </returns>
        private bool CanRead(string path)
        {
            return true;
        }

        /// <summary>
        /// Checks if access to the file is granted.
        /// </summary>
        /// <param name="path">The path to the file as a <see cref="string" /></param>
        /// <returns>
        /// The <see cref="bool" /> true if the user has access, else false
        /// </returns>
        private static bool CanReadFile(string path)
        {
            return true;
        }

        /// <summary>
        /// Checks if access to the directory is granted.
        /// </summary>
        /// <param name="path">The path to the director as a <see cref="string" /></param>
        /// <returns>
        /// The <see cref="bool" /> true if the user has access, else false
        /// </returns>
        /// <remarks>
        /// Source: <see href="http://stackoverflow.com/questions/11709862/check-if-directory-is-accessible-in-c" />
        /// </remarks>
        private static bool CanReadDirectory(string path)
        {
            return true;
        }

        /// <summary>
        /// Retrieves a store item by its name.
        /// </summary>
        /// <param name="name">The name of the store item to retrieve.</param>
        /// <returns>
        /// The store item that has the specified 
        /// <paramref name="name" />;
        /// or 
        /// <c>null</c> if there is no store item with that name.
        /// </returns>
        public IWebDavStoreItem GetItemByName(string name)
        {
            string path = Path.Combine(string.IsNullOrEmpty(ItemPath) ? "/" : ItemPath, name).Replace("\\", "/");

            using (Identity.Impersonate())
            {
                var items = Items;
                var item = items.FirstOrDefault(it => it.ItemPath == path);

                if (item != null)
                {
                    if (item.IsCollection)
                        return new WebDavMailRuCloudStoreCollection(_cloud, this, path);

                    return (WebDavMailRuCloudStoreDocument) item;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new collection with the specified name.
        /// </summary>
        /// <param name="name">The name of the new collection.</param>
        /// <returns>
        /// The created <see cref="IWebDavStoreCollection" /> instance.
        /// </returns>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnauthorizedException">When the user is unauthorized or has no access</exception>
        public IWebDavStoreCollection CreateCollection(string name)
        {
            try
            {
                WindowsImpersonationContext wic = Identity.Impersonate();
                _cloud.CreateFolder(name, ItemPath).Wait();
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }

            return GetItemByName(name) as IWebDavStoreCollection;
        }

        /// <summary>
        /// Deletes a store item by its name.
        /// </summary>
        /// <param name="item">The name of the store item to delete.</param>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavNotFoundException">If the item was not found.</exception>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnauthorizedException">If the user is unauthorized or has no access.</exception>
        public void Delete(IWebDavStoreItem item)
        {
            try
            {
                WindowsImpersonationContext wic = Identity.Impersonate();

                if (item.IsCollection)
                    _cloud.Remove(new Folder(0, 0, item.Name, new FileSize(), item.ItemPath)).Wait();
                else
                {
                    _cloud.Remove(new MailRuCloudApi.File
                    {
                        FulPath = item.ItemPath,
                        Name = item.Name
                    }).Wait();
                }
                wic.Undo();
            }
            catch (Exception)
            {
                throw new WebDavUnauthorizedException();
            }
        }

        /// <summary>
        /// Creates a new document with the specified name.
        /// </summary>
        /// <param name="name">The name of the new document.</param>
        /// <returns>
        /// The created <see cref="IWebDavStoreDocument" /> instance.
        /// </returns>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavConflictException">If the item already exists</exception>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnauthorizedException">If the user is unauthorized or has no access</exception>
        public IWebDavStoreDocument CreateDocument(string name)
        {
            var stream = _cloud.GetUploadStream(name, ItemPath, ".bin", 0);
            stream.Close();

            var document = new WebDavMailRuCloudStoreDocument(this, Path.Combine(ItemPath, name).Replace("\\", "/"));
            _items.Add(name, new WeakReference(document));
            return document;
        }

        /// <summary>
        /// Copies an existing store item into this collection, overwriting any existing items.
        /// </summary>
        /// <param name="source">The store item to copy from.</param>
        /// <param name="destinationName">The name of the copy to create of <paramref name="source" />.</param>
        /// <param name="includeContent">The boolean for copying the containing files/folders or not.</param>
        /// <returns>
        /// The created <see cref="IWebDavStoreItem" /> instance.
        /// </returns>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnauthorizedException">If the user is unauthorized or has no access</exception>
        public IWebDavStoreItem CopyItemHere(IWebDavStoreItem source, string destinationName, bool includeContent)
        {
            IWebDavStoreItem res;

            string destinationItemPath = Path.Combine(ItemPath, destinationName).Replace("\\", "/");

            try
            {
                WindowsImpersonationContext wic = Identity.Impersonate();
                if (source.IsCollection)
                {
                    _cloud.Copy(new Folder { FulPath = source.ItemPath, Name = source.Name }, destinationName).Wait();
                    res = new WebDavMailRuCloudStoreCollection(_cloud, this, destinationItemPath);
                }
                else
                {
                    _cloud.Copy(new MailRuCloudApi.File { FulPath = source.ItemPath, Name = source.Name }, destinationName).Wait();
                    res = new WebDavMailRuCloudStoreDocument(this, destinationItemPath);
                }
                wic.Undo();

            }
            catch (Exception)
            {
                throw new WebDavUnauthorizedException();
            }

            _items.Add(destinationName, new WeakReference(res));
            return res;
        }


        /// <summary>
        /// Moves an existing store item into this collection, overwriting any existing items.
        /// </summary>
        /// <param name="source">The store item to move.</param>
        /// <param name="destinationName">The
        /// <see cref="IWebDavStoreItem" /> that refers to the item that was moved,
        /// in its new location.</param>
        /// <returns>
        /// The moved <see cref="IWebDavStoreItem" /> instance.
        /// </returns>
        /// <exception cref="System.Exception">Path to the source item not defined.</exception>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnauthorizedException">If the user is unauthorized or has no access</exception>
        /// <remarks>
        /// Note that the method should fail without creating or overwriting content in the
        /// target collection if the move cannot go through.
        /// </remarks>
        public IWebDavStoreItem MoveItemHere(IWebDavStoreItem source, string destinationName)
        {

            IWebDavStoreItem res;

            // try to get the path of the source item
            string sourceItemPath = "";
            var sourceItem = (WebDavMailRuCloudStoreItem)source;
            sourceItemPath = sourceItem.ItemPath;

            if (sourceItemPath.Equals(""))
            {
                throw new Exception("Path to the source item not defined.");
            }

            // We get the path of
            string destinationItemPath = ItemPath; //Path.Combine(ItemPath, destinationName).Replace("\\", "/"); ;

            try
            {
                WindowsImpersonationContext wic = Identity.Impersonate();
                if (source.IsCollection)
                {
                    _cloud.Move(new Folder { FulPath = source.ItemPath, Name = source.Name }, destinationItemPath).Wait();
                    res = new WebDavMailRuCloudStoreCollection(_cloud, this, destinationItemPath);
                }
                else
                {
                    _cloud.Move(new MailRuCloudApi.File { FulPath = source.ItemPath, Name = source.Name }, destinationItemPath).Wait();
                    res = new WebDavMailRuCloudStoreDocument(this, destinationItemPath);
                }
                wic.Undo();

            }
            catch (Exception)
            {
                throw new WebDavUnauthorizedException();
            }

            _items.Add(destinationName, new WeakReference(res));
            return res;

        }

        public override bool IsCollection => true;

        #endregion
    }
}

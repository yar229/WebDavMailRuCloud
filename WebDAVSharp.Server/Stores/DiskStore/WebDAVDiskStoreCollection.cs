using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using WebDAVSharp.Server.Exceptions;

namespace WebDAVSharp.Server.Stores.DiskStore
{
    /// <summary>
    /// This class implements a disk-based <see cref="IWebDavStore" /> that maps to a folder on disk.
    /// </summary>
    [DebuggerDisplay("Directory ({Name})")]
    public sealed class WebDavDiskStoreCollection : WebDavDiskStoreItem, IWebDavStoreCollection
    {
        #region Variables

        private readonly Dictionary<string, WeakReference> _items = new Dictionary<string, WeakReference>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavDiskStoreCollection" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent <see cref="WebDavDiskStoreCollection" /> that contains this <see cref="WebDavDiskStoreCollection" />.</param>
        /// <param name="path">The path to the folder on this that this <see cref="WebDavDiskStoreCollection" /> maps to.</param>
        public WebDavDiskStoreCollection(WebDavDiskStoreCollection parentCollection, string path)
            : base(parentCollection, path)
        {

        }

        #endregion

        #region IWebDAVStoreCollection Members

        /// <summary>
        /// Gets a collection of all the items in this <see cref="IWebDavStoreCollection" />.
        /// </summary>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnauthorizedException">If the user is unauthorized or doesn't have access</exception>
        public IEnumerable<IWebDavStoreItem> Items
        {
            get
            {
                List<IWebDavStoreItem> items = new List<IWebDavStoreItem>();
                try
                {
                    // Impersonate the current user and get all the directories
                    using (Identity.Impersonate())
                    {
                        foreach (string dirName in Directory.GetDirectories(ItemPath))
                        {
                            try
                            {
                                bool canread = CanReadDirectory(Path.Combine(ItemPath, dirName));
                                if (!canread)
                                    continue;
                                string name = Path.GetFileName(dirName);
                                if (String.IsNullOrEmpty(name))
                                    continue;
                                WebDavDiskStoreCollection collection = null;

                                WeakReference wr;
                                if (_items.TryGetValue(name, out wr))
                                {
                                    collection = wr.Target as WebDavDiskStoreCollection;
                                    if (collection == null)
                                        continue;
                                }

                                if (collection == null)
                                {
                                    collection = new WebDavDiskStoreCollection(this, dirName);
                                    _items[name] = new WeakReference(collection);
                                }

                                items.Add(collection);
                            }
                            catch (Exception)
                            {
                                // do nothing
                            }
                        }
                    }


                    foreach (string filePath in (Directory.GetFiles(ItemPath).Where(fileName => CanReadFile(Path.Combine(ItemPath, fileName)))))
                    {
                        string name = Path.GetFileName(filePath);
                        WebDavDiskStoreDocument document = null;
                        if (string.IsNullOrEmpty(name))
                            continue;

                        WeakReference wr;
                        if (_items.TryGetValue(name, out wr))
                        {
                            document = wr.Target as WebDavDiskStoreDocument;
                            if (document == null)
                                continue;
                        }

                        if (document == null)
                        {
                            document = new WebDavDiskStoreDocument(this, filePath);
                            _items[name] = new WeakReference(document);
                        }
                        items.Add(document);
                    }
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
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
            if (File.Exists(path))
                return CanReadFile(path);
            return Directory.Exists(path) && CanReadDirectory(path);
        }

        /// <summary>
        /// Checks if access to the file is granted.
        /// </summary>
        /// <param name="path">The path to the file as a <see cref="string" /></param>
        /// <returns>
        /// The <see cref="bool" /> true if the user has access, else false
        /// </returns>
        /// <remarks>
        /// Source: <see href="http://stackoverflow.com/questions/17318585/check-if-file-can-be-read" />
        /// </remarks>
        private static bool CanReadFile(string path)
        {
            try
            {
                File.Open(path, FileMode.Open, FileAccess.Read).Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
            bool readAllow = false;
            bool readDeny = false;
            DirectorySecurity accessControlList = Directory.GetAccessControl(path);
            if (accessControlList == null)
                return false;
            AuthorizationRuleCollection accessRules = accessControlList.GetAccessRules(true, true, typeof(SecurityIdentifier));

            foreach (FileSystemAccessRule rule in accessRules.Cast<FileSystemAccessRule>().Where(rule => (FileSystemRights.Read & rule.FileSystemRights) == FileSystemRights.Read))
            {
                switch (rule.AccessControlType)
                {
                    case AccessControlType.Allow:
                        readAllow = true;
                        break;
                    case AccessControlType.Deny:
                        readDeny = true;
                        break;
                }
            }

            return readAllow && !readDeny;
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
            string path = Path.Combine(ItemPath, name);

            using (Identity.Impersonate())
            {
                if (File.Exists(path) && CanReadFile(path))
                    return new WebDavDiskStoreDocument(this, path);
                if (Directory.Exists(path) && CanReadDirectory(path))
                    return new WebDavDiskStoreCollection(this, path);
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
            string path = Path.Combine(ItemPath, name);

            try
            {
                // Impersonate the current user and make file changes
                WindowsImpersonationContext wic = Identity.Impersonate();
                Directory.CreateDirectory(path);
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
            WebDavDiskStoreItem diskItem = (WebDavDiskStoreItem)item;
            string itemPath = diskItem.ItemPath;
            if (item is WebDavDiskStoreDocument)
            {
                if (!File.Exists(itemPath))
                    throw new WebDavNotFoundException(String.Format("Cannot delete item {0}", itemPath) );
                try
                {
                    // Impersonate the current user and delete the file
                    WindowsImpersonationContext wic = Identity.Impersonate();
                    File.Delete(itemPath);
                    wic.Undo();

                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }
            }
            else
            {
                if (!Directory.Exists(itemPath))
                    throw new WebDavNotFoundException(String.Format("Directory {0} does not exists", itemPath));
                try
                {
                    // Impersonate the current user and delete the directory
                    WindowsImpersonationContext wic = Identity.Impersonate();
                    Directory.Delete(diskItem.ItemPath);
                    wic.Undo();
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }
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
            string itemPath = Path.Combine(ItemPath, name);
            if (File.Exists(itemPath) || Directory.Exists(itemPath))
                throw new WebDavConflictException();

            try
            {
                // Impersonate the current user and delete the directory
                WindowsImpersonationContext wic = Identity.Impersonate();
                File.Create(itemPath).Dispose();
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }

            WebDavDiskStoreDocument document = new WebDavDiskStoreDocument(this, itemPath);
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
            // We get the path of
            string destinationItemPath = Path.Combine(ItemPath, destinationName);

            // the moving of the item
            if (source.IsCollection)
            {
                try
                {
                    // Impersonate the current user and move the directory
                    WindowsImpersonationContext wic = Identity.Impersonate();
                    DirectoryCopy(source.ItemPath, destinationItemPath, true);
                    wic.Undo();
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }

                // We return the moved file as a WebDAVDiskStoreDocument
                WebDavDiskStoreCollection collection = new WebDavDiskStoreCollection(this, destinationItemPath);
                _items.Add(destinationName, new WeakReference(collection));
                return collection;
            }


            // We copy the file with an override.
            try
            {
                // Impersonate the current user and copy the file
                WindowsImpersonationContext wic = Identity.Impersonate();
                File.Copy(source.ItemPath, destinationItemPath, true);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }

            // We return the moved file as a WebDAVDiskStoreDocument
            WebDavDiskStoreDocument document = new WebDavDiskStoreDocument(this, destinationItemPath);
            _items.Add(destinationName, new WeakReference(document));
            return document;

        }

        /// <summary>
        /// Directories the copy.
        /// </summary>
        /// <param name="sourceDirName">Name of the source dir.</param>
        /// <param name="destDirName">Name of the dest dir.</param>
        /// <param name="copySubDirs">if set to <c>true</c> [copy sub dirs].</param>
        /// <exception cref="System.IO.DirectoryNotFoundException">Source directory does not exist or could not be found: 
        ///                     + sourceDirName</exception>
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (!copySubDirs)
                return;
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
                file.CopyTo(Path.Combine(destDirName, file.Name), false);

            foreach (DirectoryInfo subdir in dirs)
                DirectoryCopy(subdir.FullName, Path.Combine(destDirName, subdir.Name), copySubDirs);
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
            // try to get the path of the source item
            WebDavDiskStoreItem sourceItem = (WebDavDiskStoreItem)source;
            string sourceItemPath = sourceItem.ItemPath;

            if (sourceItemPath.Equals(string.Empty))
            {
                throw new Exception("Path to the source item not defined.");
            }

            // We get the path of
            string destinationItemPath = Path.Combine(ItemPath, destinationName);

            // the moving of the item
            if (source.IsCollection)
            {
                try
                {
                    // Impersonate the current user and move the directory
                    WindowsImpersonationContext wic = Identity.Impersonate();
                    Directory.Move(sourceItemPath, destinationItemPath);
                    wic.Undo();
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }

                // We return the moved file as a WebDAVDiskStoreDocument
                var collection = new WebDavDiskStoreCollection(this, destinationItemPath);
                _items.Add(destinationName, new WeakReference(collection));
                return collection;
            }

            try
            {
                // Impersonate the current user and move the file
                WindowsImpersonationContext wic = Identity.Impersonate();
                File.Move(sourceItemPath, destinationItemPath);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }

            // We return the moved file as a WebDAVDiskStoreDocument
            WebDavDiskStoreDocument document = new WebDavDiskStoreDocument(this, destinationItemPath);
            _items.Add(destinationName, new WeakReference(document));
            return document;

        }

        #endregion
    }
}
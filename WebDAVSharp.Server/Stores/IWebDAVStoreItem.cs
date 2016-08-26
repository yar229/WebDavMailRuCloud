using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using WebDAVSharp.Server.Stores.Locks;

namespace WebDAVSharp.Server.Stores
{
    /// <summary>
    /// This interface must be implemented by classes that will function as a store item,
    /// which is either a document (
    /// <see cref="IWebDavStoreDocument" />) or a
    /// collection of documents (
    /// <see cref="IWebDavStoreCollection" />.)
    /// </summary>
    public interface IWebDavStoreItem
    {
        /// <summary>
        /// Gets the parent <see cref="IWebDavStoreCollection" /> that owns this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <value>
        /// The parent collection.
        /// </value>
        IWebDavStoreCollection ParentCollection
        {
            get;
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the ItemPath of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <value>
        /// The item path.
        /// </value>
        string ItemPath
        {
            get;
        }

        /// <summary>
        /// Gets if this <see cref="IWebDavStoreItem" /> is a collection.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is collection; otherwise, <c>false</c>.
        /// </value>
        bool IsCollection
        {
            get;
        }

        /// <summary>
        /// Gets the creation date of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <value>
        /// The creation date.
        /// </value>
        DateTime CreationDate
        {
            get;
        }

        /// <summary>
        /// Gets the modification date of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <value>
        /// The modification date.
        /// </value>
        DateTime ModificationDate
        {
            get;
        }

        /// <summary>
        /// Gets the hidden state of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <value>
        /// 1 if hidden, 0 if not.
        /// </value>
        int Hidden
        {
            get;
        }

        /// <summary>
        /// Return logical lock key for items, useful if you are using virtual
        /// folder where a document can be located in different paths.
        /// </summary>
        string LockLogicalKey { get; }

        /// <summary>
        /// Try to lock the resource
        /// </summary>
        /// <returns>True if the resource is locked, false if the resource was
        /// already locked and cannot be unlocked.</returns>
        Boolean Lock();

        /// <summary>
        /// Unlock the resource 
        /// </summary>
        /// <param name="token"></param>
        /// <returns>true if the unlock operation is successful</returns>
        Boolean UnLock(String token);

        /// <summary>
        /// return 
        /// </summary>
        /// <returns></returns>
        WebDavItemInfo GetDocumentInfo();

        /// <summary>
        /// Returns all custom properties for this object, it can return null 
        /// if this object has no custom properties.
        /// </summary>
        /// <returns>List of properties grouped by namespaces, if the object does not 
        /// support custom properties it can return null.</returns>
        List<WebDavCustomProperties> GetCustomProperties();

        /// <summary>
        /// Set a series of properties into webdav item
        /// </summary>
        /// <param name="propertiesToSet">List of properties to set</param>
        void SetProperties(IEnumerable<WebDavProperty> propertiesToSet);

        /// <summary>
        /// Gets a property from webdav item
        /// </summary>
        /// <param name="davProperty"></param>
        /// <returns></returns>
        string GetProperty(WebDavProperty davProperty);
    }
}
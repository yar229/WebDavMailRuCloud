using System.Collections.Generic;

namespace WebDAVSharp.Server.Stores
{
    /// <summary>
    /// This interface must be implemented by classes that operate as document collections in a store.
    /// </summary>
    public interface IWebDavStoreCollection : IWebDavStoreItem
    {
        /// <summary>
        /// Gets a collection of all the items in this <see cref="IWebDavStoreCollection" />.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        IEnumerable<IWebDavStoreItem> Items
        {
            get;
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
        IWebDavStoreItem GetItemByName(string name);

        /// <summary>
        /// Creates a new collection with the specified name.
        /// </summary>
        /// <param name="name">The name of the new collection.</param>
        /// <returns>
        /// The created <see cref="IWebDavStoreCollection" /> instance.
        /// </returns>
        IWebDavStoreCollection CreateCollection(string name);

        /// <summary>
        /// Deletes a store item by its name.
        /// </summary>
        /// <param name="item">The name of the store item to delete.</param>
        void Delete(IWebDavStoreItem item);

        /// <summary>
        /// Creates a new document with the specified name.
        /// </summary>
        /// <param name="name">The name of the new document.</param>
        /// <returns>
        /// The created <see cref="IWebDavStoreDocument" /> instance.
        /// </returns>
        IWebDavStoreDocument CreateDocument(string name);

        /// <summary>
        /// Copies an existing store item into this collection, overwriting any existing items.
        /// </summary>
        /// <param name="source">The store item to copy from.</param>
        /// <param name="destinationName">The name of the copy to create of <paramref name="source" />.</param>
        /// <param name="includeContent">The boolean for copying the containing files/folders or not.</param>
        /// <returns>
        /// The created <see cref="IWebDavStoreItem" /> instance.
        /// </returns>
        IWebDavStoreItem CopyItemHere(IWebDavStoreItem source, string destinationName, bool includeContent);

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
        /// <remarks>
        /// Note that the method should fail without creating or overwriting content in the
        /// target collection if the move cannot go through.
        /// </remarks>
        IWebDavStoreItem MoveItemHere(IWebDavStoreItem source, string destinationName);
    }
}
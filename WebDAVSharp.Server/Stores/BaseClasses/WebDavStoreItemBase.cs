using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores.Locks;

namespace WebDAVSharp.Server.Stores.BaseClasses
{
    /// <summary>
    /// This is the base class for <see cref="IWebDavStoreItem" /> implementations.
    /// </summary>
    public class WebDavStoreItemBase : IWebDavStoreItem
    {
        #region Variables

        private readonly IWebDavStoreCollection _parentCollection;
        private string _name;

        #endregion

        #region Constuctor
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavStoreItemBase" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent <see cref="IWebDavStoreCollection" /> that contains this <see cref="IWebDavStoreItem" /> implementation.</param>
        /// <param name="name">The name of this <see cref="IWebDavStoreItem" /></param>
        /// <exception cref="System.ArgumentNullException">name</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        protected WebDavStoreItemBase(IWebDavStoreCollection parentCollection, string name)
        {
            //if (String.IsNullOrWhiteSpace(name))
            //    throw new ArgumentNullException("name");

            _parentCollection = parentCollection;
            _name = name;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the parent <see cref="IWebDavStoreCollection" /> that owns this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public IWebDavStoreCollection ParentCollection
        {
            get
            {
                return _parentCollection;
            }
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavForbiddenException"></exception>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                string fixedName = (value ?? string.Empty).Trim();
                if (fixedName == _name)
                    return;
                if (!OnNameChanging(_name, fixedName))
                    throw new WebDavForbiddenException();
                string oldName = _name;
                _name = fixedName;
                OnNameChanged(oldName, _name);
            }
        }

        /// <summary>
        /// Gets the creation date of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public virtual DateTime CreationDate
        {
            get
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Gets the modification date of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public virtual DateTime ModificationDate
        {
            get
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Gets the path to this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public virtual string ItemPath
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets if this <see cref="IWebDavStoreItem" /> is a collection.
        /// </summary>
        public virtual bool IsCollection
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the hidden state of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public int Hidden
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Standard Lock Logical Key is the path of the document.
        /// </summary>
        public virtual string LockLogicalKey
        {
            get { return ItemPath; }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Called before the name of this <see cref="IWebDavStoreItem" /> is changing.
        /// </summary>
        /// <param name="oldName">The old name of this <see cref="IWebDavStoreItem" />.</param>
        /// <param name="newName">The new name of this <see cref="IWebDavStoreItem" />.</param>
        /// <returns>
        /// <c>true</c> if the name change is allowed;
        /// otherwise, 
        /// <c>false</c>.
        /// </returns>
        protected virtual bool OnNameChanging(string oldName, string newName)
        {
            return true;
        }

        /// <summary>
        /// Called after the name of this <see cref="IWebDavStoreItem" /> has changed.
        /// </summary>
        /// <param name="oldName">The old name of this <see cref="IWebDavStoreItem" />.</param>
        /// <param name="newName">The new name of this <see cref="IWebDavStoreItem" />.</param>
        protected virtual void OnNameChanged(string oldName, string newName)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool Lock()
        {
            //Resource can always be locked
            return true; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual bool UnLock(string token)
        {
            //Resource can always be unlocked
            return true;
        }

        /// <summary>
        /// TODO: we need to override this in derived classes to return correct
        /// information.
        /// </summary>
        /// <returns></returns>
        public virtual WebDavItemInfo GetDocumentInfo()
        {
            return new WebDavItemInfo()
            {
                CreationTime = DateTime.Now,
                LastAccessTime = DateTime.Now,
                LastWriteTime = DateTime.Now,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual List<WebDavCustomProperties> GetCustomProperties()
        {
            return null;
        }

        /// <summary>
        /// Set properties, it should be implemented by derived classes
        /// </summary>
        /// <param name="propertiesToSet"></param>
        public virtual void SetProperties(IEnumerable<WebDavProperty> propertiesToSet)
        {
            
        }

        /// <summary>
        /// Set properties, it should be implemented by derived classes
        /// </summary>
        /// <param name="property">The property we want to retrieve value</param>
        public virtual String GetProperty(WebDavProperty property)
        {
            return String.Empty;
        }

        #endregion
    }
}
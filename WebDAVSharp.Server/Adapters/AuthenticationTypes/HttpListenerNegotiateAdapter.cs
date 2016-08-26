using System;
using System.Net;
using System.Security.Principal;
using System.Threading;

namespace WebDAVSharp.Server.Adapters.AuthenticationTypes
{
    /// <summary>
    /// This 
    /// <see cref="IHttpListener" /> implementation wraps around a
    /// <see cref="HttpListener" /> instance.
    /// </summary>
    internal sealed class HttpListenerNegotiateAdapter : WebDavDisposableBase, IHttpListener, IAdapter<HttpListener>
    {
        #region Private Variables

        private readonly HttpListener _listener;

        #endregion

        #region Properties
        /// <summary>
        /// Gets the internal instance that was adapted for WebDAV#.
        /// </summary>
        /// <value>
        /// The adapted instance.
        /// </value>
        public HttpListener AdaptedInstance
        {
            get
            {
                return _listener;
            }
        }

        /// <summary>
        /// Gets the Uniform Resource Identifier (
        /// <see cref="Uri" />) prefixes handled by the
        /// adapted 
        /// <see cref="HttpListener" /> object.
        /// </summary>
        public HttpListenerPrefixCollection Prefixes
        {
            get
            {
                return _listener.Prefixes;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerNegotiateAdapter" /> class.
        /// </summary>
        internal HttpListenerNegotiateAdapter()
        {
            _listener = new HttpListener
            {
                AuthenticationSchemes = AuthenticationSchemes.Negotiate,
                UnsafeConnectionNtlmAuthentication = false
            };
        }
        #endregion

        #region Function Overrides
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_listener.IsListening)
                _listener.Close();
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Waits for a request to come in to the web server and returns a
        /// <see cref="IHttpListenerContext" /> adapter around it.
        /// </summary>
        /// <param name="abortEvent">A 
        /// <see cref="EventWaitHandle" /> to use for aborting the wait. If this
        /// event becomes set before a request comes in, this method will return 
        /// <c>null</c>.</param>
        /// <returns>
        /// A 
        /// <see cref="IHttpListenerContext" /> adapter object for a request;
        /// or 
        /// <c>null</c> if the wait for a request was aborted due to 
        /// <paramref name="abortEvent" /> being set.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">abortEvent</exception>
        /// <exception cref="ArgumentNullException"><paramref name="abortEvent" /> is <c>null</c>.</exception>
        public IHttpListenerContext GetContext(EventWaitHandle abortEvent)
        {
            if (abortEvent == null)
                throw new ArgumentNullException("abortEvent");

            IAsyncResult ar = _listener.BeginGetContext(null, null);
            int index = WaitHandle.WaitAny(new[] {abortEvent, ar.AsyncWaitHandle});
            if (index != 1) return null;
            HttpListenerContext context = _listener.EndGetContext(ar);
            return new HttpListenerContextAdapter(context);
        }
        
        /// <summary>
        /// Allows the adapted <see cref="HttpListener" /> to receive incoming requests.
        /// </summary>
        public void Start()
        {
            _listener.Start();
        }

        /// <summary>
        /// Causes the adapted <see cref="HttpListener" /> to stop receiving incoming requests.
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
        }

        public IIdentity GetIdentity(IHttpListenerContext context)
        {
            return context.AdaptedInstance.User.Identity;
        }

        #endregion
    }
}
using System;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Security.Principal;
using System.Threading;

namespace WebDAVSharp.Server.Adapters.AuthenticationTypes
{
    /// <summary>
    /// This version uses integrated authentication if present,
    /// but if the auth is not good it can fallback on a basic
    /// authentication scheme.
    /// 
    /// Please check the answer to this post
    /// 
    /// https://social.msdn.microsoft.com/Forums/en-US/c2ba1e1c-3ff8-4c74-874e-2de2bcb1a4c1/httplistener-windows-authentication-fails-for-domain-account?forum=netfxnetcom
    /// 
    /// Negotiate can be used only if the service is running as a 
    /// network service or local system. If this condition is not 
    /// fulfilled, windows or other client that supports kerberos will try
    /// to authenticate with kerberos and the auth will always fail.
    /// 
    /// To diagnose this issue, use fiddler and check auth header
    /// if it starts with YII it is a kerberos auth, and will always fail
    /// until the code run as a service with local system or network service
    /// identity.
    /// Authorization: Negotiate YIIGDQYGKwYBB
    /// <see cref="IHttpListener" /> implementation wraps around a
    /// <see cref="HttpListener" /> instance.
    /// </summary>
    internal sealed class HttpListenerSmartAdapter : WebDavDisposableBase, IHttpListener, IAdapter<HttpListener>
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

        private Boolean _canSupportKerberos;
        private AuthenticationSchemes _supportedAuthScheme;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerNegotiateAdapter" /> class.
        /// </summary>
        internal HttpListenerSmartAdapter()
        {
            var currentIdentity = WindowsIdentity.GetCurrent();
            if (currentIdentity.Name.StartsWith("NT AUTHORITY\\"))
            {
                WebDavServer.Log.Info("WebDav: SmartAdapter is used with Kerberos Support");
                _supportedAuthScheme = AuthenticationSchemes.Negotiate | AuthenticationSchemes.Basic;
                _canSupportKerberos = true;
            }
            else
            {
                WebDavServer.Log.InfoFormat(
@"WebDav: SmartAdapter is used WITHOUT Kerberos Support because user is {0}\n
Kerberos can be only used with Local system or Network Service. NTLM will be used.", currentIdentity.Name);
                _supportedAuthScheme = AuthenticationSchemes.Ntlm | AuthenticationSchemes.Basic;
                
                _canSupportKerberos = false;
            }
            WebDavServer.Log.DebugFormat("_canSupportKerberos is {0}", _canSupportKerberos);
            _listener = new HttpListener
            {
                AuthenticationSchemes = _supportedAuthScheme,
            };

            //***********************************************************************************
            // If you use UnsafeConnectionNtlmAuthentication you are allowing to the client
            // to avoid NTLM handshake for each request, this permits to cyberduck or carotdav
            // to work uploading files. If this is not set to true it is pratically impossible to upload
            // sequence of files.
            //***********************************************************************************
            _listener.UnsafeConnectionNtlmAuthentication = true;



            _listener.AuthenticationSchemeSelectorDelegate = new AuthenticationSchemeSelector(AuthSchemeSelector);
        }

        private AuthenticationSchemes AuthSchemeSelector(HttpListenerRequest httpRequest)
        {
            return _supportedAuthScheme;
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
            int index = WaitHandle.WaitAny(new[] { abortEvent, ar.AsyncWaitHandle });
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

            try
            {
                if (context.AdaptedInstance.User.Identity == null ||
                    !context.AdaptedInstance.User.Identity.IsAuthenticated)
                {
                    return null;
                }

                if (context.AdaptedInstance.User.Identity.AuthenticationType == "Basic")
                {
                    HttpListenerBasicIdentity ident = (HttpListenerBasicIdentity)context.AdaptedInstance.User.Identity;
                    var isDomainUser = ident.Name.Contains("\\");
                    var user = ident.Name;
                    var pwd = ident.Password;
                    if (isDomainUser)
                    {
                        return HttpListenerBasicAdapter.AuthenticateOnSpecificDomain(user, pwd);
                    }
                    else
                    {
                        try
                        {
                            using (PrincipalContext authContext = new PrincipalContext(ContextType.Domain))
                            {
                                if (authContext.ValidateCredentials(user, pwd))
                                {
                                    return new GenericIdentity(user);
                                }
                            }
                        }
                        catch (PrincipalException)
                        {
                            //login failed
                        }
                       
                    }
                    return null;
                }
                else
                {
                    return context.AdaptedInstance.User.Identity;
                }
            }
            catch (Exception)
            {
                return null;
            }

        }

        #endregion
    }
}
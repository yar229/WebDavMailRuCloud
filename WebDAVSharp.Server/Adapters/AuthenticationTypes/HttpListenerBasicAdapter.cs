using System;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.DirectoryServices.AccountManagement;

namespace WebDAVSharp.Server.Adapters.AuthenticationTypes
{
    class HttpListenerBasicAdapter : WebDavDisposableBase, IHttpListener, IAdapter<HttpListener>
    {
        #region Imports
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        #endregion

        public HttpListenerBasicAdapter()
        {
            AdaptedInstance = new HttpListener
            {
                AuthenticationSchemes = AuthenticationSchemes.Basic,
                UnsafeConnectionNtlmAuthentication = false
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (AdaptedInstance.IsListening)
                AdaptedInstance.Close();
        }

        public HttpListener AdaptedInstance
        {
            get;
            private set;
        }

        public IHttpListenerContext GetContext(EventWaitHandle abortEvent)
        {
            if (abortEvent == null)
                throw new ArgumentNullException("abortEvent");

            IAsyncResult ar = AdaptedInstance.BeginGetContext(null, null);
            int index = WaitHandle.WaitAny(new[] { abortEvent, ar.AsyncWaitHandle });
            if (index != 1)
                return null;
            HttpListenerContext context = AdaptedInstance.EndGetContext(ar);
            return new HttpListenerContextAdapter(context);
        }

        public HttpListenerPrefixCollection Prefixes
        {
            get
            {
                return AdaptedInstance.Prefixes;
            }
        }

        public void Start()
        {
            AdaptedInstance.Start();
        }

        public void Stop()
        {
            AdaptedInstance.Stop();
        }

        public IIdentity GetIdentity(IHttpListenerContext context)
        {
            try
            {
                HttpListenerBasicIdentity ident = (HttpListenerBasicIdentity)context.AdaptedInstance.User.Identity;
                var isDomainUser = ident.Name.Contains("\\");
                var user = ident.Name;
                var pwd = ident.Password;
                if (isDomainUser)
                {
                    return AuthenticateOnSpecificDomain(user, pwd);
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
                    }
                   
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }

        }

        internal static IIdentity AuthenticateOnSpecificDomain(string user, string pwd)
        {
            var splitted = user.Split('\\');
            string domain = splitted[0];
            string username = splitted[1];
            var token = GetToken(domain, username, pwd);
            return new WindowsIdentity(token.DangerousGetHandle());
        }


        internal static SafeTokenHandle GetToken(string domainName,
            string userName, string password)
        {
            SafeTokenHandle safeTokenHandle;

            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token.
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // Call LogonUser to obtain a handle to an access token.
            bool returnValue = LogonUser(userName, domainName, password,
                LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                out safeTokenHandle);

            if (returnValue) return safeTokenHandle;
            int ret = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(ret);
        }

        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            {
            }

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }
    }
}

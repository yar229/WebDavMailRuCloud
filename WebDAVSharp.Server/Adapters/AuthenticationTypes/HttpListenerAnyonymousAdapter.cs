using System;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace WebDAVSharp.Server.Adapters.AuthenticationTypes
{
    class HttpListenerAnyonymousAdapter : WebDavDisposableBase, IHttpListener, IAdapter<HttpListener>
    {
        public HttpListenerAnyonymousAdapter()
        {
            AdaptedInstance = new HttpListener
            {
                AuthenticationSchemes = AuthenticationSchemes.Anonymous,
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
            return WindowsIdentity.GetCurrent();
        }



    }
}

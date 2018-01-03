using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Logging;
using NWebDav.Server.Stores;

namespace NWebDav.Server
{
    /// <summary>
    /// Default implementation of the <see cref="IWebDavDispatcher"/>
    /// interface to dispatch WebDAV requests.
    /// </summary>
    /// <remarks>
    /// The default implementation uses <see cref="IRequestHandlerFactory"/>
    /// to create request handlers and invokes the handler for each request. It
    /// also adds some logging to each call and it takes care of closing the
    /// HTTP context after the request has been processed.
    /// </remarks>
    /// <seealso cref="IWebDavDispatcher"/>
    public class WebDavDispatcher : IWebDavDispatcher
    {
        private static readonly ILogger s_log = LoggerFactory.CreateLogger(typeof(WebDavDispatcher));
        private static readonly string s_serverName;

        private readonly IStore _store;
        private readonly IRequestHandlerFactory _requestHandlerFactory;

        static WebDavDispatcher()
        {
            // Determine the server name for the Server header. The format of
            // this header should have a fixed layout.
            // (see https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.38)
            var assemblyVersion = typeof(WebDavDispatcher).GetTypeInfo().Assembly.GetName().Version;
            s_serverName = $"NWebDav/{assemblyVersion}";
        }

        /// <summary>
        /// Create an instance of the default WebDavDispatcher implementation.
        /// </summary>
        /// <param name="store">
        /// Store that should be used to obtain the collections and/or
        /// documents.
        /// </param>
        /// <param name="requestHandlerFactory">
        /// Optional request handler factory that is used to find the proper
        /// <see cref="IRequestHandler"/> for the current WebDAV request. This
        /// is an optional parameter (default <see langword="null"/>). If no
        /// value is specified (or <see langword="null"/>) then the default
        /// implementation (<see cref="RequestHandlerFactory"/>) is used.
        /// </param>
        public WebDavDispatcher(IStore store, IRequestHandlerFactory requestHandlerFactory = null)
        {
            // Make sure a store resolver is specified
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            // Save store resolver and request handler factory
            _store = store;
            _requestHandlerFactory = requestHandlerFactory ?? new RequestHandlerFactory();
        }

        /// <summary>
        /// Dispatch the WebDAV request based on the given HTTP context.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP context for this request.
        /// </param>
        /// <returns>
        /// A task that represents the request dispatching operation.
        /// </returns>
        public async Task DispatchRequestAsync(IHttpContext httpContext)
        {
            // Make sure a HTTP context is specified
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            // Make sure the HTTP context has a request
            var request = httpContext.Request;
            if (request == null)
                throw new ArgumentException("The HTTP context doesn't have a request.", nameof(httpContext));

            // Make sure the HTTP context has a response
            var response = httpContext.Response;
            if (response == null)
                throw new ArgumentException("The HTTP context doesn't have a response.", nameof(httpContext));

            // Determine the request log-string
            var logRequest = $"{request.HttpMethod}:{request.Url}:{request.RemoteEndPoint}";
            var range = request.GetRange();
            if (null != range)
                logRequest += $" ({range.Start?.ToString() ?? string.Empty}-{range.End?.ToString() ?? string.Empty})";


            // Log the request
            s_log.Log(LogLevel.Info, () => $"{logRequest} - Processing");

            try
            {
                // Set the Server header of the response message. This has no
                // functional use, but it can be used to diagnose problems by
                // determining the actual WebDAV server and version.
                response.SetHeaderValue("Server", s_serverName);

                // Start the stopwatch
                var sw = Stopwatch.StartNew();

                IRequestHandler requestHandler;
                try
                {
                    // Obtain the request handler for this message
                    requestHandler = _requestHandlerFactory.GetRequestHandler(httpContext);

                    // Make sure we got a request handler
                    if (requestHandler == null)
                    {
                        // Log warning
                        s_log.Log(LogLevel.Warning, () => $"{logRequest} - Not implemented.");

                        // This request is not implemented
                        httpContext.Response.SetStatus(DavStatusCode.NotImplemented);
                        return;
                    }
                }
                catch (Exception exc)
                {
                    // Log error
                    s_log.Log(LogLevel.Error, $"Unexpected exception while trying to obtain the request handler (method={request.HttpMethod}, url={request.Url}, source={request.RemoteEndPoint}", exc);

                    // Abort
                    return;
                }

                try
                {
                    // Handle the request
                    if (await requestHandler.HandleRequestAsync(httpContext, _store).ConfigureAwait(false))
                    {
                        s_log.Log(LogLevel.Info, () => $"{logRequest} - Finished ({sw.ElapsedMilliseconds}ms, HTTP {httpContext.Response.Status})");
                    }
                    else
                    {
                        // Log warning
                        s_log.Log(LogLevel.Warning, () => $"{logRequest} - Not processed.");

                        // Set status code to bad request
                        httpContext.Response.SetStatus(DavStatusCode.NotImplemented);
                    }
                }
                catch (HttpListenerException hle) when (hle.ErrorCode == ERROR_OPERATION_ABORTED)
                {
                    s_log.Log(LogLevel.Error, $"Operation aborted at (method={request.HttpMethod}, url={request.Url}, source={request.RemoteEndPoint}");
                }
                // happens when client cancel operation, usially nothing to scare
                catch (HttpListenerException hle) when (hle.ErrorCode == ERROR_CONNECTION_INVALID)
                {
                    s_log.Log(LogLevel.Error, $"An operation was attempted on a nonexistent network connection at (method={request.HttpMethod}, url={request.Url}, source={request.RemoteEndPoint}");
                }
                // happens when client cancel operation, usially nothing to scare
                catch (HttpListenerException hle) when (hle.ErrorCode == ERROR_NETNAME_DELETED)
                {
                    s_log.Log(LogLevel.Error, $"The specified network name is no longer available at (method={request.HttpMethod}, url={request.Url}, source={request.RemoteEndPoint}");
                }
                catch (HttpListenerException excListener)
                {
                    if (excListener.ErrorCode != ERROR_OPERATION_ABORTED)
                        throw;
                }

                catch (Exception exc)
                {
                    s_log.Log(LogLevel.Error, $"Unexpected exception while handling request (method={request.HttpMethod}, url={request.Url}", exc);
                    //, source={request.RemoteEndPoint}", exc); // request.RemoteEndPoint may be disposed

                    try
                    {
                        // Attempt to return 'InternalServerError' (if still possible)
                        httpContext.Response.SetStatus(DavStatusCode.InternalServerError);
                    }
                    catch
                    {
                        // We might not be able to send the response, because a response
                        // was already initiated by the the request handler.
                    }
                }
                finally
                {
                    // Check if we need to dispose the request handler
                    (requestHandler as IDisposable)?.Dispose();
                }
            }
            finally
            {
                // Always close the context
                await httpContext.CloseAsync().ConfigureAwait(false);
            }
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms681383(v=vs.85).aspx
        // An operation was attempted on a nonexistent network connection.
        private const int ERROR_CONNECTION_INVALID = 1229;
        // The specified network name is no longer available.
        private const int ERROR_NETNAME_DELETED = 64;
        private const int ERROR_OPERATION_ABORTED = 995;
    }
}


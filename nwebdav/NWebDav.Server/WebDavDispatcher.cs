using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

using NWebDav.Server.Handlers;
using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Logging;
using NWebDav.Server.Stores;

namespace NWebDav.Server
{
    public class WebDavDispatcher : IWebDavDispatcher
    {
        private static readonly ILogger s_log = LoggerFactory.CreateLogger(typeof(WebDavDispatcher));
        private static readonly string s_serverName;

        private readonly IStore _store;
        private readonly IRequestHandlerFactory _requestHandlerFactory;

        static WebDavDispatcher()
        {
            var assemblyVersion = typeof(WebDavDispatcher).GetTypeInfo().Assembly.GetName().Version;
            s_serverName = $"NWebDav/{assemblyVersion}";
        }

        public WebDavDispatcher(IStore store, IRequestHandlerFactory requestHandlerFactory = null)
        {
            // Make sure a store resolver is specified
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            // Save store resolver and request handler factory
            _store = store;
            _requestHandlerFactory = requestHandlerFactory ?? new RequestHandlerFactory();
        }

        public async Task DispatchRequestAsync(IHttpContext httpContext)
        {
            // Determine the request log-string
            var request = httpContext.Request;
            var response = httpContext.Response;
            var logRequest = $"{request.HttpMethod}:{request.Url}:{request.RemoteEndPoint?.Address}";

            // Log the request
            s_log.Log(LogLevel.Info, $"{logRequest} - Start processing");

            try
            {
                // Set the Server header of the response
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
                        s_log.Log(LogLevel.Warning, $"{logRequest} - Not implemented.");

                        // This request is not implemented
                        httpContext.Response.SendResponse(DavStatusCode.NotImplemented);
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
                        // Log processing duration
                        s_log.Log(LogLevel.Info, $"{logRequest} - Finished processing ({sw.ElapsedMilliseconds}ms, HTTP result: {httpContext.Response.Status})");
                    }
                    else
                    {
                        // Log warning
                        s_log.Log(LogLevel.Warning, $"{logRequest} - Not processed.");

                        // Set status code to bad request
                        httpContext.Response.SendResponse(DavStatusCode.NotImplemented);
                    }
                }
                catch (Exception exc)
                {
                    // Log what's going wrong
                    s_log.Log(LogLevel.Error, $"Unexpected exception while handling request (method={request.HttpMethod}, url={request.Url}, source={request.RemoteEndPoint}", exc);

                    try
                    {
                        // Attempt to return 'InternalServerError' (if still possible)
                        httpContext.Response.SendResponse(DavStatusCode.InternalServerError);
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
                httpContext.Close();
            }
        }
    }
}


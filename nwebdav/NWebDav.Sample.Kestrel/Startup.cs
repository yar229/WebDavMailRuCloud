using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Logging;

using NWebDav.Server;
using NWebDav.Server.Handlers;
using NWebDav.Server.Stores;
using NWebDav.Server.AspNetCore;

namespace NWebDav.Sample.Kestrel
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            // Add logging
            // TODO: Migrate this logging with NWebDav logging
            loggerFactory.AddConsole(LogLevel.Debug);

            // Create the request handler factory
            var requestHandlerFactory = new RequestHandlerFactory();

            // Create WebDAV dispatcher
            var homeFolder = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetEnvironmentVariable("USERPROFILE");
            var webDavDispatcher = new WebDavDispatcher(new DiskStore(homeFolder), requestHandlerFactory);

            app.Run(async context =>
            {
                // Create the proper HTTP context
                var httpContext = new AspNetCoreContext(context);

                // Dispatch request
                await webDavDispatcher.DispatchRequestAsync(httpContext).ConfigureAwait(false);
            });
        }
    }
}
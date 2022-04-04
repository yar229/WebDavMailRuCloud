using System.Collections.Generic;
using System.Net;

namespace YaR.Clouds.Console
{
    class HttpListenerOptions
    {
        public HttpListenerOptions(CommandLineOptions options)
        {
            foreach (int port in options.Port)
            {
                const string webdavProtocol = "http";
                const string webdavIp = "127.0.0.1";
                var webdavHost = string.IsNullOrWhiteSpace(options.Host)
                    ? $"{webdavProtocol}://{webdavIp}"
                    : options.Host.TrimEnd('/');
                if (webdavHost.EndsWith("//0.0.0.0")) webdavHost = webdavHost.Replace("//0.0.0.0", "//*");

                string prefix = $"{webdavHost}:{port}/";
                Prefixes.Add(prefix);
            }
        }
        
        public List<string> Prefixes { get; } = new();
        public AuthenticationSchemes AuthenticationScheme { get; } = AuthenticationSchemes.Basic;
    }
}
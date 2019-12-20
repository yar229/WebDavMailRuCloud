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
                var webdavProtocol = "http";
                var webdavIp = "127.0.0.1";
                var webdavHost = string.IsNullOrWhiteSpace(options.Host)
                    ? $"{webdavProtocol}://{webdavIp}"
                    : options.Host.TrimEnd('/');
                if (webdavHost.EndsWith("//0.0.0.0")) webdavHost = webdavHost.Replace("//0.0.0.0", "//*");

                string prefix = $"{webdavHost}:{port}/";
                Prefixes.Add(prefix);
            }
        }
        
        public List<string> Prefixes { get; } = new List<string>();
        public AuthenticationSchemes AuthenticationScheme { get; } = AuthenticationSchemes.Basic;
    }
}
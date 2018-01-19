using System.Net;
using YaR.WebDavMailRu;

namespace YaR.CloudMailRu.Console
{
    class HttpListenerOptions
    {
        public HttpListenerOptions(CommandLineOptions options)
        {
            var webdavProtocol = "http";
            var webdavIp = "127.0.0.1";
            var webdavPort = options.Port;
            var webdavHost = string.IsNullOrWhiteSpace(options.Host)
                ? $"{webdavProtocol}://{webdavIp}"
                : options.Host.TrimEnd('/');
            if (webdavHost.EndsWith("//0.0.0.0")) webdavHost = webdavHost.Replace("//0.0.0.0", "//*");

            Prefix = $"{webdavHost}:{webdavPort}/";
        }
        
        public string Prefix { get; }
        public AuthenticationSchemes AuthenticationScheme { get; } = AuthenticationSchemes.Basic;
    }
}
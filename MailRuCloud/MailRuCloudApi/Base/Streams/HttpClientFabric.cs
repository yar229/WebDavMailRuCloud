using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;

namespace YaR.Clouds.Base.Streams
{
    class HttpClientFabric
    {
        public static HttpClientFabric Instance => _instance ??= new HttpClientFabric();
        private static HttpClientFabric _instance;

        public HttpClient this[Account account]
        {
            get
            {
                var cli =  _lockDict.GetOrAdd(account, new HttpClient(new HttpClientHandler
                    {
                        UseProxy = true,
                        Proxy = account.RequestRepo.HttpSettings.Proxy,
                        CookieContainer = account.RequestRepo.Authent.Cookies,
                        UseCookies = true,
                        AllowAutoRedirect = true,
                        MaxConnectionsPerServer = int.MaxValue,
                        
                    })
                    {Timeout = Timeout.InfiniteTimeSpan});

                return cli;
            }
        }

        private readonly ConcurrentDictionary<Account, HttpClient> _lockDict = new ConcurrentDictionary<Account, HttpClient>();
    }
}
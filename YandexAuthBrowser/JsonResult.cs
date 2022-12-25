using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YandexAuthBrowser
{
    public class BrowserAppResponse
    {
        [JsonProperty( "Login" )]
        public string? Login { get; set; }

        [JsonProperty( "Uuid" )]
        public string? Uuid { get; set; }

        [JsonProperty( "Sk" )]
        public string? Sk { get; set; }

        [JsonProperty( "Cookies" )]
        public List<BrowserAppCookieResponse>? Cookies { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject( this );
        }
    }
    public class BrowserAppCookieResponse
    {
        [JsonProperty( "name" )]
        public string? Name { get; set; }

        [JsonProperty( "Value" )]
        public string? Value { get; set; }

        [JsonProperty( "Path" )]
        public string? Path { get; set; }

        [JsonProperty( "Domain" )]
        public string? Domain { get; set; }
    }
}

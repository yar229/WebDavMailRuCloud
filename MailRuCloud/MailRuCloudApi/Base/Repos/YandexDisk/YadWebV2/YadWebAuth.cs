using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2
{
    class YadWebAuth : IAuth
    {
        public YadWebAuth(HttpCommonSettings settings, IBasicCredentials creds)
        {
            _settings = settings;
            _creds = creds;
            Cookies = new CookieContainer();

            MakeLogin().Wait();
        }

        public YadWebAuth(HttpCommonSettings settings, IBasicCredentials creds, string path)
        {
            _settings = settings;
            _creds = creds;
            Cookies = new CookieContainer();

            string content = System.IO.File.ReadAllText(path);
            BrowserAppResponse response = JsonConvert.DeserializeObject<BrowserAppResponse>(content);

            DiskSk = /*YadAuth.DiskSk*/ response.Sk;
            Uuid = /*YadAuth.Uuid*/response.Uuid; //yandexuid

            foreach(var item in response.Cookies)
            {
                Cookie cookie = new Cookie(item.Name, item.Value, item.Path, item.Domain);
                Cookies.Add(cookie);
            }
        }

        public static string GetCache(HttpCommonSettings settings, IBasicCredentials creds)
        {
            // Проверяем кеш в файле и читаем, если он есть
            if(string.IsNullOrEmpty(settings.CloudSettings.BrowserAuthenticatorstringCacheDir))
                return null;

            string path = System.IO.Path.Combine(
                settings.CloudSettings.BrowserAuthenticatorstringCacheDir,
                creds.Login
                );
            if(!System.IO.File.Exists(path))
                return null;

            return path;
        }

        private readonly IBasicCredentials _creds;
        private readonly HttpCommonSettings _settings;

        public async Task MakeLogin()
        {
            (BrowserAppResponse response, string responseHtml) = await ConnectToBrowserApp();

            if(response!=null &&
                !string.IsNullOrEmpty(response.Sk) &&
                !string.IsNullOrEmpty(response.Uuid) &&
                !string.IsNullOrEmpty(response.Login) &&
                YadWebAuth.GetNameOnly(response.Login)
                    .Equals(YadWebAuth.GetNameOnly(Login), StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrEmpty(response.ErrorMessage)
                )
            {
                DiskSk = /*YadAuth.DiskSk*/ response.Sk;
                Uuid = /*YadAuth.Uuid*/response.Uuid; //yandexuid

                foreach(var item in response.Cookies)
                {
                    Cookie cookie = new Cookie(item.Name, item.Value, item.Path, item.Domain);
                    Cookies.Add(cookie);
                }

                // Если аутентификация прошла успешно, сохраняем результат в кеш в файл
                if(!string.IsNullOrEmpty(_settings.CloudSettings.BrowserAuthenticatorstringCacheDir))
                {
                    string path = System.IO.Path.Combine(
                        _settings.CloudSettings.BrowserAuthenticatorstringCacheDir,
                        _creds.Login
                        );

                    try
                    {
                        string dir = System.IO.Path.GetDirectoryName(path);
                        if(!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                    }
                    catch(Exception)
                    {
                        throw new AuthenticationException("Directory for cache can not be created, " +
                            "remove attribute CacheDir in BrowserAuthenticator tag in configuration file!");
                    }
                    try
                    {
                        System.IO.File.WriteAllText(path, responseHtml);
                    }
                    catch(Exception) { }
                }
            }
            else
            {
                if(string.IsNullOrEmpty(response?.ErrorMessage))
                    throw new AuthenticationException("OAuth: Authentication using YandexAuthBrowser is failed!");
                else
                    throw new AuthenticationException(
                        string.Concat(
                            "OAuth: Authentication using YandexAuthBrowser is failed! ",
                            response.ErrorMessage)
                        );
            }
        }

        public string Login => _creds.Login;
        public string Password => _creds.Password;
        public string DiskSk { get; set; }
        public string Uuid { get; set; }
        //public string Csrf { get; set; }



        public bool IsAnonymous => false;
        public string AccessToken { get; }
        public string DownloadToken { get; }
        public CookieContainer Cookies { get; private set; }
        public void ExpireDownloadToken()
        {
            throw new NotImplementedException();
        }

        public class BrowserAppResponse
        {
            [JsonProperty("ErrorMessage")]
            public string ErrorMessage { get; set; }

            [JsonProperty("Login")]
            public string Login { get; set; }

            [JsonProperty("Uuid")]
            public string Uuid { get; set; }

            [JsonProperty("Sk")]
            public string Sk { get; set; }

            [JsonProperty("Cookies")]
            public List<BrowserAppCookieResponse> Cookies { get; set; }
        }
        public class BrowserAppCookieResponse
        {
            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("Value")]
            public string Value { get; set; }

            [JsonProperty("Path")]
            public string Path { get; set; }

            [JsonProperty("Domain")]
            public string Domain { get; set; }
        }

        private static string GetNameOnly(string value)
        {
            if(string.IsNullOrEmpty(value))
                return value;
            int pos = value.IndexOf('@');
            if(pos==0)
                return "";
            if(pos>0)
                return value.Substring(0, pos);
            return value;
        }

        private async Task<(BrowserAppResponse, string)> ConnectToBrowserApp()
        {
            // Login для подключения содержит название логина Диска, не email, затем | и номер порта программы с браузером.
            // Password - пароль в программе с браузером.

            string url = _settings.CloudSettings.BrowserAuthenticatorUrl;
            string password = string.IsNullOrWhiteSpace(Password)
                ? _settings.CloudSettings.BrowserAuthenticatorstringPassword
                : Password;

            if(string.IsNullOrEmpty(url))
            {
                throw new Exception("Ошибка! " +
                    "Для работы с Яндекс.Диском запустите сервер аутентификации и задайте в параметре YandexAuthenticationUrl его настройки!");
            }

            using var client = new HttpClient { BaseAddress = new Uri(url) };
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"/{Uri.EscapeDataString(Login)}/{Uri.EscapeDataString(password)}/", UriKind.Relative),
                Headers = {
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                    { HttpRequestHeader.ContentType.ToString(), "application/json" },
                },
                //Content = new StringContent(JsonConvert.SerializeObject(""))
            };

            client.Timeout = new TimeSpan(0, 5, 0);
            using var response = await client.SendAsync(httpRequestMessage);
            var responseText = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            BrowserAppResponse data = JsonConvert.DeserializeObject<BrowserAppResponse>(responseText);
            return (data, responseText);
        }
    }
}

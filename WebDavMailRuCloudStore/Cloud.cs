using MailRuCloudApi;

namespace YaR.WebDavMailRu.CloudStore
{
    public static class Cloud
    {
        public static void Init(string login, string password, string userAgent = "")
        {
            if (!string.IsNullOrEmpty(userAgent))
                ConstSettings.UserAgent = userAgent;

            Instance = new MailRuCloud(login, password);
        }

        public static  MailRuCloud Instance;

    }
}

namespace YaR.MailRuCloud.Api
{
    public interface ITwoFaHandler
    {
        string Get(string login, bool isAutoRelogin);
    }
}

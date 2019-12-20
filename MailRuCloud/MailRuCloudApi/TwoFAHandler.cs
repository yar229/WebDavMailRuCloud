namespace YaR.Clouds
{
    public interface ITwoFaHandler
    {
        string Get(string login, bool isAutoRelogin);
    }
}

namespace YaR.Clouds.Base
{
    internal interface IBasicCredentials
    {
        string Login { get; }
        string Password { get; }

        bool IsAnonymous { get; }
    }
}
using System;
using System.Security.Authentication;

namespace YaR.MailRuCloud.Api.Base
{
    internal class Credentials : IBasicCredentials
    {
        public Credentials(string login, string password)
        {
            if (string.IsNullOrEmpty(login))
                throw new InvalidCredentialException("Login is null or empty.");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password is null or empty.");


            int slashpos = login.IndexOf(@"#", StringComparison.InvariantCulture);
            if (slashpos == login.Length - 1)
                throw new InvalidCredentialException("Invalid credential format.");

            if (slashpos < 0)
            {
                Login = login;
                Password = password;
                PasswordCrypt = string.Empty;
                return;
            }

            Login = login.Substring(0, slashpos);

            string separator = login.Substring(slashpos + 1);

            int seppos = password.IndexOf(separator, StringComparison.InvariantCulture);
            if (seppos < 0)
                throw new InvalidCredentialException("Invalid credential format.");

            Password = password.Substring(0, seppos);
            if (seppos + separator.Length >= password.Length)
                throw new InvalidCredentialException("Invalid credential format.");

            PasswordCrypt = password.Substring(seppos + separator.Length);
        }


        public string Login { get; }
        public string Password { get; }

        public string PasswordCrypt { get; }

        public bool CanCrypt => !string.IsNullOrEmpty(PasswordCrypt);

    }
}
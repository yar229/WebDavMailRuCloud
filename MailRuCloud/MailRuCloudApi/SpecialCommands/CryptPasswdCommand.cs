using System.Collections.Generic;
using System.Threading.Tasks;

namespace YaR.Clouds.SpecialCommands
{
    /// <summary>
    /// Пароль для (де)шифрования
    /// </summary>
    public class CryptPasswdCommand : SpecialCommand
    {
        public CryptPasswdCommand(Cloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(1);

        public override async Task<SpecialCommandResult> Execute()
        {
            var newPasswd = Parames[0];
            if (string.IsNullOrEmpty(newPasswd))
                return await Task.FromResult(new SpecialCommandResult(false, "Crypt password is empty"));

            Cloud.Account.Credentials.PasswordCrypt = newPasswd;

            return await Task.FromResult(SpecialCommandResult.Success);
        }
    }
}
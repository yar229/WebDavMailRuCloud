namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class SpecialCommandResult
    {
        public SpecialCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get;}

        public static SpecialCommandResult Success => new SpecialCommandResult(true);
        public static SpecialCommandResult Fail => new SpecialCommandResult(false);

    }
}
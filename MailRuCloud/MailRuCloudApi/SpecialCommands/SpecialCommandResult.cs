namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class SpecialCommandResult
    {
        public SpecialCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public SpecialCommandResult(bool isSuccess, string message) : this(isSuccess)
        {
            Message = message;
        }

        public bool IsSuccess { get;}
        public string Message { get; }

        public static SpecialCommandResult Success => new SpecialCommandResult(true);
        public static SpecialCommandResult Fail => new SpecialCommandResult(false);

    }
}
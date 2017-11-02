namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class SpecialCommandResult
    {
        public bool IsSuccess { get; set; }

        public static SpecialCommandResult Success => new SpecialCommandResult {IsSuccess = true};
        public static SpecialCommandResult Fail => new SpecialCommandResult { IsSuccess = false };

    }
}
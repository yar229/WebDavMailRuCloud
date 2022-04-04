namespace YaR.Clouds.SpecialCommands
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

        public static SpecialCommandResult Success => new(true);
        public static SpecialCommandResult Fail => new(false);

    }
}
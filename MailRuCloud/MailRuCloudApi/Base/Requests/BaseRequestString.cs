namespace YaR.MailRuCloud.Api.Base.Requests
{
    public abstract class BaseRequestString : BaseRequestString<string>
    {
        protected BaseRequestString(RequestInit init) : base(init)
        {
        }

        protected override RequestResponse<string> DeserializeMessage(string data)
        {
            var msg = new RequestResponse<string>
            {
                Ok = true,
                Result = data
            };
            return msg;
        }
    }
}
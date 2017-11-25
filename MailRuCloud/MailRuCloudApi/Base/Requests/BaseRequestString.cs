namespace YaR.MailRuCloud.Api.Base.Requests
{
    public abstract class BaseRequestString : BaseRequestString<string>
    {
        protected BaseRequestString(CloudApi cloudApi) : base(cloudApi)
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
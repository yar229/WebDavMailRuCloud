using System;
using System.Threading.Tasks;
using log4net.Appender;

namespace YaR.Clouds
{
    public class SmtpAsyncAppender : SmtpAppender
    {
        protected override void SendEmail(string messageBody)
        {
            Task.Run(() =>
            {
                try
                {
                    base.SendEmail(messageBody);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            });
        }
    }
}

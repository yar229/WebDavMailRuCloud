using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YaR.MailRuCloud.Api;

namespace YaR.MailRuCloud.TwoFA.UI
{
    public class AuthCodeWindow : ITwoFaHandler
    {
        private readonly IEnumerable<KeyValuePair<string, string>> _parames;

        public AuthCodeWindow(IEnumerable<KeyValuePair<string, string>> parames)
        {
            _parames = parames;
        }

        public string Get(string login, bool isAutoRelogin)
        {
            Application.EnableVisualStyles();

            using (NotifyIcon notify = new NotifyIcon())
            using (AuthCodeForm prompt = new AuthCodeForm())
            {
                var prompt1 = prompt;

                notify.Visible = true;
                notify.Icon = SystemIcons.Exclamation;
                notify.BalloonTipTitle = "WebDAV Mail.Ru 2 Factor Auth";
                notify.BalloonTipText = $"Auth code required for {login}";
                notify.BalloonTipIcon = ToolTipIcon.Info;

                notify.Click += (sender, args) =>
                    prompt1.Activate();
                notify.BalloonTipClicked += (sender, args) =>
                    prompt1.Activate();

                notify.ShowBalloonTip(30000);

                var res = prompt.ShowAuthDialog(login, isAutoRelogin);

                return res.AuthCode;
            }
        }
    }
}

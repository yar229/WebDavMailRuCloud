using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MailRuCloudApi.TwoFA
{
    public class AuthCodeWindow : ITwoFaHandler
    {
        public AuthCodeWindow()
        { }

        public string Get(string login, bool isAutoRelogin)
        {
            string result = string.Empty;

            Application.EnableVisualStyles();

            using (NotifyIcon notify = new NotifyIcon())
            using (AuthCodeForm prompt = new AuthCodeForm())
            {
                notify.Visible = true;
                notify.Icon = SystemIcons.Exclamation;
                notify.BalloonTipTitle = "WebDAV Mail.Ru 2 Factor Auth";
                notify.BalloonTipText = $"Auth code required for {login}";
                notify.BalloonTipIcon = ToolTipIcon.Info;

                notify.Click += (sender, args) => 
                    prompt?.Activate();
                notify.BalloonTipClicked += (sender, args) => 
                    prompt?.Activate();

                notify.ShowBalloonTip(30000);

                var res = prompt.ShowAuthDialog(login, isAutoRelogin);

                return res.AuthCode;
            }
        }
    }
}

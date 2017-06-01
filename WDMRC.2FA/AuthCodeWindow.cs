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

                notify.ShowBalloonTip(30000);

                var res = prompt.ShowAuthDialog(login, isAutoRelogin);

                return res.AuthCode;

                //CancellationTokenSource cancelWait = new CancellationTokenSource();
                //notify.Click += (o, eventArgs) =>
                //{
                //    result = prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
                //    cancelWait.Cancel();
                //};

                //notify.ShowBalloonTip(30000);

                //var t =  new Task(() =>
                //{
                //    while (!cancelWait.IsCancellationRequested)
                //    {
                //        Thread.Sleep(100);
                //    }

                //});
                //t.Start();
                //t.Wait(cancelWait.Token);
            }
            
            //return result;
        }

        private async Task Handle(CancellationToken cancellationToken)
        {
        }
    }
}

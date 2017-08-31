using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MailRuCloudApi.TwoFA
{
    public partial class AuthCodeForm : Form
    {
        public AuthCodeForm()
        {
            InitializeComponent();
        }

        public AuthDialogResult ShowAuthDialog(string login, bool isRelogin)
        {
            txtLogin.Text = login;
            lblInfo.Text = isRelogin
                ? "Auto relogin request"
                : "Login request";
            txtAuthCode.Focus();

            var res = ShowDialog();

            return new AuthDialogResult
            {
                DialogResult = res,
                AuthCode = txtAuthCode.Text
            };
        }

    }

    public class AuthDialogResult
    {
        public DialogResult DialogResult { get; set; }
        public string AuthCode { get; set; }
    }
}

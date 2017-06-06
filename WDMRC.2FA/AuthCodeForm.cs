using System.Windows.Forms;

namespace MailRuCloudApi.TwoFA
{
    public partial class AuthCodeForm : Form
    {
        public AuthCodeForm()
        {
            InitializeComponent();
        }

        public AuthDialogResult ShowAuthDialog(string login, string phone, bool isRelogin)
        {
            txtLogin.Text = login;
            txtPhone.Text = phone;
            lblInfo.Text = isRelogin
                ? "Auto relogin request"
                : "Login request";
            txtAuthCode.Focus();

            var res = ShowDialog();

            return new AuthDialogResult
            {
                DialogResult = res,
                AuthCode = txtAuthCode.Text,
                DoNotAskAgainForThisDevice = chkDoNotAsk.Checked
            };
        }
    }

    public class AuthDialogResult
    {
        public DialogResult DialogResult { get; set; }
        public string AuthCode { get; set; }
        public bool DoNotAskAgainForThisDevice { get; set; }
    }
}

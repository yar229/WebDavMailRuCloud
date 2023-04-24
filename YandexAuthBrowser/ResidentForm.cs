using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

/*
 * Частично код взят отсюда:
 * https://gist.github.com/define-private-public/d05bc52dd0bed1c4699d49e2737e80e7
 */

namespace YandexAuthBrowser
{
    public partial class ResidentForm : Form
    {
        [GeneratedRegex("http://[^/]*/(?<login>.*?)/(?<password>.*?)/", RegexOptions.Compiled)]
        private static partial Regex UrlRegex();


        private HttpListener? Listener;
        private bool RunServer = false;
        private string PreviousPort;
        public delegate void Execute(string desiredLogin, BrowserAppResponse response);
        public Execute AuthExecuteDelegate;
        private readonly int? SavedTop = null;
        private readonly int? SavedLeft = null;

        private int AuthenticationOkCounter = 0;
        private int AuthenticationFailCounter = 0;


        public ResidentForm()
        {
            InitializeComponent();

            var screen = Screen.GetWorkingArea(this);
            Top = screen.Height + 100;
            ShowInTaskbar = false;

            NotifyIcon.Visible = true;

            // Get the current configuration file.
            System.Configuration.Configuration config =
                    ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.None);

            string? value = config.AppSettings?.Settings?["port"]?.Value;
            if( !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out _) )
                Port.Text = value;
            value = config.AppSettings?.Settings?["password"]?.Value;
            if( !string.IsNullOrWhiteSpace(value) )
                Password.Text = value;

            value = config.AppSettings?.Settings?["Top"]?.Value;
            if( !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out int top) )
                SavedTop = top;
            value = config.AppSettings?.Settings?["Left"]?.Value;
            if( !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out int left) )
                SavedLeft = left;


            PreviousPort = Port.Text;

            NotifyIcon.ContextMenuStrip = new ContextMenuStrip();
            NotifyIcon.ContextMenuStrip.Items.Add("Показать окно", null, NotifyIcon_ShowClick);
            NotifyIcon.ContextMenuStrip.Items.Add("Выход", null, NotifyIcon_ExitClick);

            AuthExecuteDelegate = OpenDialog;

            Counter.Text = "";

            RunServer = true;
            StartServer();
        }

        private void ResidentForm_Load(object sender, EventArgs e)
        {
            Lock.Checked = true;
            Lock.Focus();
            HideTimer.Interval = 100;
            HideTimer.Enabled = true;
        }
        private void HideTimer_Tick(object sender, EventArgs e)
        {
            HideTimer.Enabled = false;

            HideShow(false);
        }

        private void HideShow(bool show)
        {
            if( show )
            {
                if( !ShowInTaskbar )
                {
                    var screen = Screen.GetWorkingArea(this);

                    if( SavedTop.HasValue && SavedLeft.HasValue &&
                        SavedTop.Value >= 0 && SavedTop.Value + Height < screen.Height &&
                        SavedLeft.Value >= 0 && SavedLeft.Value + Width < screen.Width )

                    {
                        Top = SavedTop.Value;
                        Left = SavedLeft.Value;
                    }
                    else
                    {
                        Left = screen.Width - Width - 10;
                        Top = screen.Height - Height - 100;
                    }
                    ShowInTaskbar = true;
                }
                Visible = true;
            }
            else
            {
                Visible = false;
            }
        }
        private void ResidentForm_Move(object sender, EventArgs e)
        {
            if( Visible )
            {
                SaveConfigTimer.Interval = 1000;
                SaveConfigTimer.Enabled = true;
            }

        }
        private void SaveConfigTimer_Tick(object sender, EventArgs e)
        {
            SaveConfigTimer.Enabled = false;

            System.Configuration.Configuration config =
                    ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.None);

            config.AppSettings.Settings.Remove("Top");
            config.AppSettings.Settings.Remove("Left");

            config.AppSettings.Settings.Add("Top", Top.ToString());
            config.AppSettings.Settings.Add("Left", Left.ToString());

            // Save the configuration file.
            config.Save(ConfigurationSaveMode.Modified);
        }
        private void NotifyIcon_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            HideShow(!Visible);
        }

        /*
		 * Метод
		 * ResidentForm_FormClosed( object? sender, FormClosingEventArgs e )
		 * здесь не использовать, т.к. событие перекрывается и обрабатывается
		 * в HiddenContent. См. там.
		 */

        private void ResidentForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            HideShow(false);
            e.Cancel = RunServer ? /*просто закрывается окно*/ true : /*Выход в меню TrayIcon*/ false;
        }
        private void NotifyIcon_ExitClick(object? sender, EventArgs e)
        {
            NotifyIcon.Visible = false;
            StopServer();

            // При вызове Close дальше будет обработка в HiddenContext, см. там.
            Close();
        }
        private void NotifyIcon_ShowClick(object? sender, EventArgs e)
        {
            HideShow(true);
        }
        private void HideButton_Click(object sender, EventArgs e)
        {
            HideShow(false);
        }
        private void SaveConfig()
        {
            // Get the current configuration file.
            System.Configuration.Configuration config =
                    ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.None);

            config.AppSettings.Settings.Remove("port");
            config.AppSettings.Settings.Remove("password");

            config.AppSettings.Settings.Add("port", Port.Text);
            config.AppSettings.Settings.Add("password", Password.Text);

            // Save the configuration file.
            config.Save(ConfigurationSaveMode.Modified);
        }
        private void Port_TextChanged(object sender, EventArgs e)
        {
            SaveConfig();
            if( RunServer )
            {
                StopServer();
                StartServer();
            }
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            SaveConfig();
            if( RunServer )
            {
                StopServer();
                StartServer();
            }
        }
        private void GeneratePassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if( Lock.Checked )
                return;
            Password.Text = Guid.NewGuid().ToString();
        }
        private void Port_Validating(object sender, CancelEventArgs e)
        {
            if( !int.TryParse(Port.Text, out int value) || value < 1 || value > ushort.MaxValue )
            {
                e.Cancel = true;
                Port.Text = PreviousPort;
            }
            else
            {
                PreviousPort = Port.Text;

            }
        }

        private void CopyPortPic_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Port.Text);
        }

        private void CopyPasswordPic_Click(object sender, EventArgs e)
        {
            if( string.IsNullOrEmpty(Password.Text) )
                Password.Text = Guid.NewGuid().ToString();

            Clipboard.SetText(Password.Text);
        }

        private void Lock_CheckedChanged(object sender, EventArgs e)
        {
            Port.Enabled = !Lock.Checked;
            Password.Enabled = !Lock.Checked;
            GeneratePassword.Visible = !Lock.Checked;
        }

        private void StartServer()
        {
#if DEBUG
            Port.Text = "54322";
            Password.Text = "adb4bcd5-b4b6-45b7-bb7d-b38470917448";
#endif
            if( !int.TryParse(Port.Text, out int port) )
            {
                Port.Text = "54321";
                port = 54321;
            }

            try
            {
                Listener = new HttpListener();
                // Create a http server and start listening for incoming connections
                Listener?.Prefixes.Add($"http://localhost:{port}/");
                Listener?.Start();

                // Handle requests
                _ = Task.Run(HandleIncomingConnections);
            }
            catch( Exception ex )
            {
                MessageBox.Show(ex.Message,
                    "Ошибка инициализации сервера аутентификации Яндекс.Диска", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
        private void StopServer()
        {
            RunServer = false;
            Listener?.Abort();
            Listener?.Close();
            Listener = null;
        }

        private void AuthButton_Click(object sender, EventArgs e)
        {
            new AuthForm("default", new BrowserAppResponse()).ShowDialog();

        }
        private void OpenDialog(string desiredLogin, BrowserAppResponse response)
        {
            // Переключение на поток, обрабатывающий UI.
            //System.Threading.SynchronizationContext.Current?.Post( ( _ ) =>
            //{
            //	new AuthForm( desiredLogin, response ).ShowDialog();
            //}, null );
            new AuthForm(desiredLogin, response).ShowDialog();

            if( response.Cookies != null )
                AuthenticationOkCounter++;
            else
                AuthenticationFailCounter++;

            Counter.Text = $"Входов успешных / не успешных : {AuthenticationOkCounter} / {AuthenticationFailCounter}";
        }

        public async Task HandleIncomingConnections()
        {
            string passwordToCompre = Password.Text;

            while( RunServer )
            {
                try
                {
                    if( Listener == null )
                        throw new NullReferenceException("Listener is null");

                    // Will wait here until we hear from a connection
                    HttpListenerContext ctx = await Listener.GetContextAsync();

                    // Peel out the requests and response objects
                    HttpListenerRequest req = ctx.Request;
                    using HttpListenerResponse resp = ctx.Response;

                    var match = UrlRegex().Match(req.Url?.AbsoluteUri ?? "");

                    var login = match.Success ? match.Groups["login"].Value : string.Empty;
                    var password = match.Success ? match.Groups["password"].Value : string.Empty;

                    BrowserAppResponse response = new BrowserAppResponse();

                    if( string.IsNullOrEmpty(login) )
                        response.ErrorMessage = "Login is not provided";
                    else
                    if( string.IsNullOrEmpty(password) )
                        response.ErrorMessage = "Password is not provided";
                    else
                    if( password != passwordToCompre )
                        response.ErrorMessage = "Password is wrong";
                    else
                    {

                        // Окно с браузером нужно открыть в потоке, обрабатывающем UI
                        if( AuthButton.InvokeRequired )
                            AuthButton.Invoke(AuthExecuteDelegate, login, response);
                        else
                            AuthExecuteDelegate(login, response);
                    }

                    string text = response.Serialize();
                    byte[] data = Encoding.UTF8.GetBytes(text);
                    resp.ContentType = "application/json";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.Length;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data);
                    resp.Close();
                }
                catch( ObjectDisposedException )
                {
                    // Такое исключение при Listener.Abort(), значит работа закончена
                    return;
                }
                catch( HttpListenerException )
                {
                    if( !RunServer )
                        return;
                }
            }
        }
    }
}

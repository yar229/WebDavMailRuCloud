namespace YandexAuthBrowser
{
    partial class ResidentForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResidentForm));
            this.label1 = new System.Windows.Forms.Label();
            this.Port = new System.Windows.Forms.TextBox();
            this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.Password = new System.Windows.Forms.TextBox();
            this.GeneratePassword = new System.Windows.Forms.LinkLabel();
            this.HideButton = new System.Windows.Forms.Button();
            this.Lock = new System.Windows.Forms.CheckBox();
            this.HideTimer = new System.Windows.Forms.Timer(this.components);
            this.AuthButton = new System.Windows.Forms.Button();
            this.CopyPortPic = new System.Windows.Forms.PictureBox();
            this.CopyPasswordPic = new System.Windows.Forms.PictureBox();
            this.SaveConfigTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.CopyPortPic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CopyPasswordPic)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(329, 30);
            this.label1.TabIndex = 1;
            this.label1.Text = "Порт для входящего соединения";
            // 
            // Port
            // 
            this.Port.Location = new System.Drawing.Point(22, 54);
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(90, 35);
            this.Port.TabIndex = 2;
            this.Port.Text = "54321";
            this.Port.TextChanged += new System.EventHandler(this.Port_TextChanged);
            this.Port.Validating += new System.ComponentModel.CancelEventHandler(this.Port_Validating);
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.NotifyIcon.BalloonTipText = "WebDAVCloud для Яндекс.Диск";
            this.NotifyIcon.BalloonTipTitle = "Резидентная часть для отображения на десктопе окна браузера для входа на Яндекс.Д" +
    "иск";
            this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
            this.NotifyIcon.Text = "WebDAVCloud";
            this.NotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseDoubleClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(353, 30);
            this.label2.TabIndex = 3;
            this.label2.Text = "Пароль для входящего соединения";
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(22, 133);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(449, 35);
            this.Password.TabIndex = 4;
            this.Password.TextChanged += new System.EventHandler(this.Password_TextChanged);
            // 
            // GeneratePassword
            // 
            this.GeneratePassword.AutoSize = true;
            this.GeneratePassword.Location = new System.Drawing.Point(22, 171);
            this.GeneratePassword.Name = "GeneratePassword";
            this.GeneratePassword.Size = new System.Drawing.Size(429, 30);
            this.GeneratePassword.TabIndex = 5;
            this.GeneratePassword.TabStop = true;
            this.GeneratePassword.Text = "Сгенерировать пароль в виде нового GUID";
            this.GeneratePassword.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GeneratePassword_LinkClicked);
            // 
            // HideButton
            // 
            this.HideButton.Location = new System.Drawing.Point(22, 214);
            this.HideButton.Name = "HideButton";
            this.HideButton.Size = new System.Drawing.Size(490, 40);
            this.HideButton.TabIndex = 6;
            this.HideButton.Text = "Свернуть программу в System Tray";
            this.HideButton.UseVisualStyleBackColor = true;
            this.HideButton.Click += new System.EventHandler(this.HideButton_Click);
            // 
            // Lock
            // 
            this.Lock.AutoSize = true;
            this.Lock.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Lock.Location = new System.Drawing.Point(431, 21);
            this.Lock.Name = "Lock";
            this.Lock.Size = new System.Drawing.Size(81, 34);
            this.Lock.TabIndex = 0;
            this.Lock.Text = "Lock";
            this.Lock.UseVisualStyleBackColor = true;
            this.Lock.CheckedChanged += new System.EventHandler(this.Lock_CheckedChanged);
            // 
            // HideTimer
            // 
            this.HideTimer.Tick += new System.EventHandler(this.HideTimer_Tick);
            // 
            // AuthButton
            // 
            this.AuthButton.Location = new System.Drawing.Point(431, 61);
            this.AuthButton.Name = "AuthButton";
            this.AuthButton.Size = new System.Drawing.Size(81, 40);
            this.AuthButton.TabIndex = 7;
            this.AuthButton.Text = "Test";
            this.AuthButton.UseVisualStyleBackColor = true;
            this.AuthButton.Click += new System.EventHandler(this.AuthButton_Click);
            // 
            // CopyPortPic
            // 
            this.CopyPortPic.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CopyPortPic.Image = ((System.Drawing.Image)(resources.GetObject("CopyPortPic.Image")));
            this.CopyPortPic.InitialImage = ((System.Drawing.Image)(resources.GetObject("CopyPortPic.InitialImage")));
            this.CopyPortPic.Location = new System.Drawing.Point(118, 54);
            this.CopyPortPic.Name = "CopyPortPic";
            this.CopyPortPic.Size = new System.Drawing.Size(35, 35);
            this.CopyPortPic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.CopyPortPic.TabIndex = 9;
            this.CopyPortPic.TabStop = false;
            this.CopyPortPic.Click += new System.EventHandler(this.CopyPortPic_Click);
            // 
            // CopyPasswordPic
            // 
            this.CopyPasswordPic.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CopyPasswordPic.Image = ((System.Drawing.Image)(resources.GetObject("CopyPasswordPic.Image")));
            this.CopyPasswordPic.InitialImage = ((System.Drawing.Image)(resources.GetObject("CopyPasswordPic.InitialImage")));
            this.CopyPasswordPic.Location = new System.Drawing.Point(477, 133);
            this.CopyPasswordPic.Name = "CopyPasswordPic";
            this.CopyPasswordPic.Size = new System.Drawing.Size(35, 35);
            this.CopyPasswordPic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.CopyPasswordPic.TabIndex = 9;
            this.CopyPasswordPic.TabStop = false;
            this.CopyPasswordPic.Click += new System.EventHandler(this.CopyPasswordPic_Click);
            // 
            // SaveConfigTimer
            // 
            this.SaveConfigTimer.Tick += new System.EventHandler(this.SaveConfigTimer_Tick);
            // 
            // ResidentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 272);
            this.Controls.Add(this.CopyPasswordPic);
            this.Controls.Add(this.CopyPortPic);
            this.Controls.Add(this.AuthButton);
            this.Controls.Add(this.Lock);
            this.Controls.Add(this.HideButton);
            this.Controls.Add(this.GeneratePassword);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.Port);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResidentForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WebDAVCloud browser authentication";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ResidentForm_FormClosing);
            this.Load += new System.EventHandler(this.ResidentForm_Load);
            this.Move += new System.EventHandler(this.ResidentForm_Move);
            ((System.ComponentModel.ISupportInitialize)(this.CopyPortPic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CopyPasswordPic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private TextBox Port;
        private NotifyIcon NotifyIcon;
        private Label label2;
        private TextBox Password;
        private LinkLabel GeneratePassword;
        private Button HideButton;
        private CheckBox Lock;
        private System.Windows.Forms.Timer HideTimer;
        private Button AuthButton;
        private PictureBox CopyPortPic;
        private PictureBox CopyPasswordPic;
        private System.Windows.Forms.Timer SaveConfigTimer;
    }
}
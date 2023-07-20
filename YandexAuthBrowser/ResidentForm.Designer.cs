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
        protected override void Dispose(bool disposing)
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components= new System.ComponentModel.Container() ;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResidentForm));
            label1= new Label() ;
            Port= new TextBox() ;
            NotifyIcon= new NotifyIcon(components) ;
            label2= new Label() ;
            Password= new TextBox() ;
            GeneratePassword= new LinkLabel() ;
            HideButton= new Button() ;
            Lock= new CheckBox() ;
            HideTimer= new System.Windows.Forms.Timer(components) ;
            AuthButton= new Button() ;
            CopyPortPic= new PictureBox() ;
            CopyPasswordPic= new PictureBox() ;
            SaveConfigTimer= new System.Windows.Forms.Timer(components) ;
            label3= new Label() ;
            Counter= new Label() ;
            ( (System.ComponentModel.ISupportInitialize) CopyPortPic  ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize) CopyPasswordPic  ).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize= true ;
            label1.Location= new Point(22, 21) ;
            label1.Name= "label1" ;
            label1.Size= new Size(329, 30) ;
            label1.TabIndex= 1 ;
            label1.Text= "Порт для входящего соединения" ;
            // 
            // Port
            // 
            Port.Location= new Point(22, 54) ;
            Port.Name= "Port" ;
            Port.Size= new Size(90, 35) ;
            Port.TabIndex= 2 ;
            Port.Text= "54321" ;
            Port.TextChanged+= Port_TextChanged ;
            Port.Validating+= Port_Validating ;
            // 
            // NotifyIcon
            // 
            NotifyIcon.BalloonTipIcon= ToolTipIcon.Info ;
            NotifyIcon.BalloonTipText= "WebDAVCloud для Яндекс.Диск" ;
            NotifyIcon.BalloonTipTitle= "Резидентная часть для отображения на десктопе окна браузера для входа на Яндекс.Диск" ;
            NotifyIcon.Icon= (Icon) resources.GetObject("NotifyIcon.Icon")  ;
            NotifyIcon.Text= "WebDAVCloud" ;
            NotifyIcon.MouseDoubleClick+= NotifyIcon_MouseDoubleClick ;
            // 
            // label2
            // 
            label2.AutoSize= true ;
            label2.Location= new Point(22, 100) ;
            label2.Name= "label2" ;
            label2.Size= new Size(353, 30) ;
            label2.TabIndex= 3 ;
            label2.Text= "Пароль для входящего соединения" ;
            // 
            // Password
            // 
            Password.Location= new Point(22, 133) ;
            Password.Name= "Password" ;
            Password.Size= new Size(523, 35) ;
            Password.TabIndex= 4 ;
            Password.TextChanged+= Password_TextChanged ;
            // 
            // GeneratePassword
            // 
            GeneratePassword.AutoSize= true ;
            GeneratePassword.Location= new Point(22, 171) ;
            GeneratePassword.Name= "GeneratePassword" ;
            GeneratePassword.Size= new Size(429, 30) ;
            GeneratePassword.TabIndex= 5 ;
            GeneratePassword.TabStop= true ;
            GeneratePassword.Text= "Сгенерировать пароль в виде нового GUID" ;
            GeneratePassword.LinkClicked+= GeneratePassword_LinkClicked ;
            // 
            // HideButton
            // 
            HideButton.Location= new Point(22, 260) ;
            HideButton.Name= "HideButton" ;
            HideButton.Size= new Size(564, 40) ;
            HideButton.TabIndex= 6 ;
            HideButton.Text= "Свернуть программу в System Tray" ;
            HideButton.UseVisualStyleBackColor= true ;
            HideButton.Click+= HideButton_Click ;
            // 
            // Lock
            // 
            Lock.AutoSize= true ;
            Lock.Cursor= Cursors.Hand ;
            Lock.Location= new Point(505, 14) ;
            Lock.Name= "Lock" ;
            Lock.Size= new Size(81, 34) ;
            Lock.TabIndex= 0 ;
            Lock.Text= "Lock" ;
            Lock.UseVisualStyleBackColor= true ;
            Lock.CheckedChanged+= Lock_CheckedChanged ;
            // 
            // HideTimer
            // 
            HideTimer.Tick+= HideTimer_Tick ;
            // 
            // AuthButton
            // 
            AuthButton.Location= new Point(505, 54) ;
            AuthButton.Name= "AuthButton" ;
            AuthButton.Size= new Size(81, 40) ;
            AuthButton.TabIndex= 7 ;
            AuthButton.Text= "Test" ;
            AuthButton.UseVisualStyleBackColor= true ;
            AuthButton.Click+= AuthButton_Click ;
            // 
            // CopyPortPic
            // 
            CopyPortPic.Cursor= Cursors.Hand ;
            CopyPortPic.Image= (Image) resources.GetObject("CopyPortPic.Image")  ;
            CopyPortPic.InitialImage= (Image) resources.GetObject("CopyPortPic.InitialImage")  ;
            CopyPortPic.Location= new Point(118, 54) ;
            CopyPortPic.Name= "CopyPortPic" ;
            CopyPortPic.Size= new Size(35, 35) ;
            CopyPortPic.SizeMode= PictureBoxSizeMode.Zoom ;
            CopyPortPic.TabIndex= 9 ;
            CopyPortPic.TabStop= false ;
            CopyPortPic.Click+= CopyPortPic_Click ;
            // 
            // CopyPasswordPic
            // 
            CopyPasswordPic.Cursor= Cursors.Hand ;
            CopyPasswordPic.Image= (Image) resources.GetObject("CopyPasswordPic.Image")  ;
            CopyPasswordPic.InitialImage= (Image) resources.GetObject("CopyPasswordPic.InitialImage")  ;
            CopyPasswordPic.Location= new Point(551, 133) ;
            CopyPasswordPic.Name= "CopyPasswordPic" ;
            CopyPasswordPic.Size= new Size(35, 35) ;
            CopyPasswordPic.SizeMode= PictureBoxSizeMode.Zoom ;
            CopyPasswordPic.TabIndex= 9 ;
            CopyPasswordPic.TabStop= false ;
            CopyPasswordPic.Click+= CopyPasswordPic_Click ;
            // 
            // SaveConfigTimer
            // 
            SaveConfigTimer.Tick+= SaveConfigTimer_Tick ;
            // 
            // label3
            // 
            label3.AutoSize= true ;
            label3.Location= new Point(22, 312) ;
            label3.Name= "label3" ;
            label3.Size= new Size(564, 30) ;
            label3.TabIndex= 1 ;
            label3.Text= "Для выхода используйте меню иконки в системном трее" ;
            // 
            // Counter
            // 
            Counter.AutoSize= true ;
            Counter.Location= new Point(22, 211) ;
            Counter.Name= "Counter" ;
            Counter.Size= new Size(29, 30) ;
            Counter.TabIndex= 1 ;
            Counter.Text= "--" ;
            // 
            // ResidentForm
            // 
            AutoScaleDimensions= new SizeF(12F, 30F) ;
            AutoScaleMode= AutoScaleMode.Font ;
            ClientSize= new Size(612, 354) ;
            Controls.Add(CopyPasswordPic);
            Controls.Add(CopyPortPic);
            Controls.Add(AuthButton);
            Controls.Add(Lock);
            Controls.Add(HideButton);
            Controls.Add(GeneratePassword);
            Controls.Add(Password);
            Controls.Add(Port);
            Controls.Add(label2);
            Controls.Add(Counter);
            Controls.Add(label3);
            Controls.Add(label1);
            FormBorderStyle= FormBorderStyle.FixedDialog ;
            Icon= (Icon) resources.GetObject("$this.Icon")  ;
            MaximizeBox= false ;
            MinimizeBox= false ;
            Name= "ResidentForm" ;
            StartPosition= FormStartPosition.CenterScreen ;
            Text= "WebDAVCloud browser authentication" ;
            FormClosing+= ResidentForm_FormClosing ;
            Load+= ResidentForm_Load ;
            Move+= ResidentForm_Move ;
            ( (System.ComponentModel.ISupportInitialize) CopyPortPic  ).EndInit();
            ( (System.ComponentModel.ISupportInitialize) CopyPasswordPic  ).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
        private Label label3;
        private Label Counter;
    }
}
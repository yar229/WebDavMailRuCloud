namespace YandexAuthBrowser
{
    partial class AuthForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components= new System.ComponentModel.Container() ;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AuthForm));
            WebView= new Microsoft.Web.WebView2.WinForms.WebView2() ;
            DelayTimer= new System.Windows.Forms.Timer(components) ;
            NobodyHomeTimer= new System.Windows.Forms.Timer(components) ;
            ( (System.ComponentModel.ISupportInitialize) WebView  ).BeginInit();
            SuspendLayout();
            // 
            // WebView
            // 
            WebView.AllowExternalDrop= true ;
            WebView.CreationProperties= null ;
            WebView.DefaultBackgroundColor= Color.White ;
            WebView.Dock= DockStyle.Fill ;
            WebView.Location= new Point(0, 0) ;
            WebView.Name= "WebView" ;
            WebView.Size= new Size(1165, 895) ;
            WebView.TabIndex= 1 ;
            WebView.ZoomFactor= 1D ;
            WebView.NavigationCompleted+= WebView_NavigationCompleted ;
            // 
            // DelayTimer
            // 
            DelayTimer.Interval= 3000 ;
            DelayTimer.Tick+= DelayTimer_Tick ;
            // 
            // NobodyHomeTimer
            // 
            NobodyHomeTimer.Interval= 3000 ;
            NobodyHomeTimer.Tick+= NobodyHomeTimer_Tick ;
            // 
            // AuthForm
            // 
            AutoScaleDimensions= new SizeF(12F, 30F) ;
            AutoScaleMode= AutoScaleMode.Font ;
            ClientSize= new Size(1165, 895) ;
            Controls.Add(WebView);
            Icon= (Icon) resources.GetObject("$this.Icon")  ;
            Name= "AuthForm" ;
            Text= "Авторизация на Яндекс.Диск" ;
            FormClosed+= AuthForm_FormClosed ;
            Load+= AuthForm_Load ;
            ( (System.ComponentModel.ISupportInitialize) WebView  ).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Microsoft.Web.WebView2.WinForms.WebView2 WebView;
        private System.Windows.Forms.Timer DelayTimer;
        private System.Windows.Forms.Timer NobodyHomeTimer;
    }
}
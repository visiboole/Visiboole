namespace VisiBoole.Views
{
    partial class TutorialWindow
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
            if (disposing && (components != null))
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
            this.tutorialBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // tutorialBrowser
            // 
            this.tutorialBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tutorialBrowser.Location = new System.Drawing.Point(0, 0);
            this.tutorialBrowser.Margin = new System.Windows.Forms.Padding(10);
            this.tutorialBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.tutorialBrowser.Name = "tutorialBrowser";
            this.tutorialBrowser.Size = new System.Drawing.Size(588, 373);
            this.tutorialBrowser.TabIndex = 0;
            //string currentURL = C
            string tutorialURL = "..\\..\\Resources\\Help Documentation\\help.html";
            this.tutorialBrowser.Url = new System.Uri(tutorialURL, System.UriKind.Relative);
            // 
            // TutorialWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
            this.ClientSize = new System.Drawing.Size(588, 373);
            this.Controls.Add(this.tutorialBrowser);
            this.Name = "TutorialWindow";
            this.ShowIcon = false;
            this.Text = "Tutorial";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser tutorialBrowser;
    }
}
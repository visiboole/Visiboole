using System;
using System.IO;
using System.Reflection;

namespace VisiBoole.Views
{
    partial class DocumentWindow
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
            this.Browser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // Browser
            // 
            this.Browser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Browser.Location = new System.Drawing.Point(0, 0);
            this.Browser.Margin = new System.Windows.Forms.Padding(13, 12, 13, 12);
            this.Browser.MinimumSize = new System.Drawing.Size(27, 25);
            this.Browser.Name = "Browser";
            this.Browser.Size = new System.Drawing.Size(784, 459);
            this.Browser.TabIndex = 0;
            this.Browser.Url = new System.Uri("C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\Resourc" +
        "es\\Help Documentation\\Syntax.html", System.UriKind.Absolute);
            // 
            // DocumentWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
            this.ClientSize = new System.Drawing.Size(784, 459);
            this.Controls.Add(this.Browser);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "DocumentWindow";
            this.ShowIcon = false;
            this.Text = "Document";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser Browser;
    }
}
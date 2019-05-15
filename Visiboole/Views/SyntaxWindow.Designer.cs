using System;
using System.IO;
using System.Reflection;

namespace VisiBoole.Views
{
    partial class SyntaxWindow
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
            this.syntaxBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // syntaxBrowser
            // 
            this.syntaxBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.syntaxBrowser.Location = new System.Drawing.Point(0, 0);
            this.syntaxBrowser.Margin = new System.Windows.Forms.Padding(10);
            this.syntaxBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.syntaxBrowser.Name = "syntaxBrowser";
            this.syntaxBrowser.Size = new System.Drawing.Size(588, 373);
            this.syntaxBrowser.TabIndex = 0;
            this.syntaxBrowser.Url = new System.Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Help Documentation", "Syntax.html"), System.UriKind.Absolute);
            // 
            // SyntaxWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
            this.ClientSize = new System.Drawing.Size(588, 373);
            this.Controls.Add(this.syntaxBrowser);
            this.Name = "SyntaxWindow";
            this.ShowIcon = false;
            this.Text = "Syntax";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser syntaxBrowser;
    }
}
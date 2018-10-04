namespace VisiBoole.Views
{
    partial class DisplayDefault
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OpenFileLinkLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // OpenFileLinkLabel
            // 
            this.OpenFileLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenFileLinkLabel.AutoSize = true;
            this.OpenFileLinkLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.OpenFileLinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(226)))), ((int)(((byte)(85)))));
            this.OpenFileLinkLabel.Location = new System.Drawing.Point(291, 293);
            this.OpenFileLinkLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.OpenFileLinkLabel.Name = "OpenFileLinkLabel";
            this.OpenFileLinkLabel.Size = new System.Drawing.Size(218, 15);
            this.OpenFileLinkLabel.TabIndex = 3;
            this.OpenFileLinkLabel.TabStop = true;
            this.OpenFileLinkLabel.Text = "Open a VisiBoole project or file to get started";
            this.OpenFileLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DisplayDefault
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.OpenFileLinkLabel);
            this.Name = "DisplayDefault";
            this.Size = new System.Drawing.Size(800, 600);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel OpenFileLinkLabel;
    }
}

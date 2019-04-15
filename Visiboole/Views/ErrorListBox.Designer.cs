namespace VisiBoole.Views
{
    partial class ErrorListBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorListBox));
            this.uxButtonExit = new System.Windows.Forms.Button();
            this.uxPanelTop = new System.Windows.Forms.Panel();
            this.uxLabelTitle = new System.Windows.Forms.Label();
            this.uxRichTextBoxLog = new System.Windows.Forms.RichTextBox();
            this.uxPanelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // uxButtonExit
            // 
            this.uxButtonExit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.uxButtonExit.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.uxButtonExit.FlatAppearance.BorderSize = 0;
            this.uxButtonExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.uxButtonExit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.uxButtonExit.ForeColor = System.Drawing.Color.White;
            this.uxButtonExit.Location = new System.Drawing.Point(497, 3);
            this.uxButtonExit.Name = "uxButtonExit";
            this.uxButtonExit.Size = new System.Drawing.Size(25, 23);
            this.uxButtonExit.TabIndex = 2;
            this.uxButtonExit.Text = "X";
            this.uxButtonExit.UseVisualStyleBackColor = true;
            // 
            // uxPanelTop
            // 
            this.uxPanelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
            this.uxPanelTop.Controls.Add(this.uxLabelTitle);
            this.uxPanelTop.Controls.Add(this.uxButtonExit);
            this.uxPanelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.uxPanelTop.Location = new System.Drawing.Point(0, 0);
            this.uxPanelTop.Name = "uxPanelTop";
            this.uxPanelTop.Size = new System.Drawing.Size(525, 30);
            this.uxPanelTop.TabIndex = 1;
            // 
            // uxLabelTitle
            // 
            this.uxLabelTitle.AutoSize = true;
            this.uxLabelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.uxLabelTitle.ForeColor = System.Drawing.Color.White;
            this.uxLabelTitle.Location = new System.Drawing.Point(3, 7);
            this.uxLabelTitle.Name = "uxLabelTitle";
            this.uxLabelTitle.Size = new System.Drawing.Size(49, 19);
            this.uxLabelTitle.TabIndex = 4;
            this.uxLabelTitle.Text = "Errors";
            // 
            // uxRichTextBoxLog
            // 
            this.uxRichTextBoxLog.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.uxRichTextBoxLog.HideSelection = false;
            this.uxRichTextBoxLog.Location = new System.Drawing.Point(13, 37);
            this.uxRichTextBoxLog.Name = "uxRichTextBoxLog";
            this.uxRichTextBoxLog.ReadOnly = true;
            this.uxRichTextBoxLog.Size = new System.Drawing.Size(500, 201);
            this.uxRichTextBoxLog.TabIndex = 2;
            this.uxRichTextBoxLog.Text = "";
            this.uxRichTextBoxLog.WordWrap = false;
            // 
            // ErrorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(525, 250);
            this.Controls.Add(this.uxRichTextBoxLog);
            this.Controls.Add(this.uxPanelTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ErrorListBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Errors";
            this.TopMost = true;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ErrorListBoxPaint);
            this.uxPanelTop.ResumeLayout(false);
            this.uxPanelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button uxButtonExit;
        private System.Windows.Forms.Panel uxPanelTop;
        private System.Windows.Forms.Label uxLabelTitle;
        private System.Windows.Forms.RichTextBox uxRichTextBoxLog;
    }
}
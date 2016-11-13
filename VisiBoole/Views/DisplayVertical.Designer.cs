﻿namespace VisiBoole
{
    partial class DisplayVertical
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
            this.pnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlEditorControls = new System.Windows.Forms.Panel();
            this.btnRun = new System.Windows.Forms.Button();
            this.pnlOutputControls = new System.Windows.Forms.Panel();
            this.IndependentVars = new System.Windows.Forms.RichTextBox();
            this.updTickCount = new System.Windows.Forms.NumericUpDown();
            this.btnTick = new System.Windows.Forms.Button();
            this.tabEditor = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.outputBrowser = new System.Windows.Forms.WebBrowser();
            this.pnlMain.SuspendLayout();
            this.pnlEditorControls.SuspendLayout();
            this.pnlOutputControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updTickCount)).BeginInit();
            this.tabEditor.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlMain.ColumnCount = 2;
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlMain.Controls.Add(this.pnlEditorControls, 0, 1);
            this.pnlMain.Controls.Add(this.pnlOutputControls, 1, 1);
            this.pnlMain.Controls.Add(this.tabEditor, 0, 0);
            this.pnlMain.Controls.Add(this.outputBrowser, 1, 0);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.RowCount = 2;
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 94F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6F));
            this.pnlMain.Size = new System.Drawing.Size(800, 600);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlEditorControls
            // 
            this.pnlEditorControls.Controls.Add(this.btnRun);
            this.pnlEditorControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditorControls.Location = new System.Drawing.Point(3, 567);
            this.pnlEditorControls.Name = "pnlEditorControls";
            this.pnlEditorControls.Size = new System.Drawing.Size(394, 30);
            this.pnlEditorControls.TabIndex = 0;
            // 
            // btnRun
            // 
            this.btnRun.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnRun.Location = new System.Drawing.Point(315, 4);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // pnlOutputControls
            // 
            this.pnlOutputControls.Controls.Add(this.IndependentVars);
            this.pnlOutputControls.Controls.Add(this.updTickCount);
            this.pnlOutputControls.Controls.Add(this.btnTick);
            this.pnlOutputControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOutputControls.Location = new System.Drawing.Point(403, 567);
            this.pnlOutputControls.Name = "pnlOutputControls";
            this.pnlOutputControls.Size = new System.Drawing.Size(394, 30);
            this.pnlOutputControls.TabIndex = 1;
            // 
            // IndependentVars
            // 
            this.IndependentVars.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.IndependentVars.Location = new System.Drawing.Point(12, 4);
            this.IndependentVars.Multiline = false;
            this.IndependentVars.Name = "IndependentVars";
            this.IndependentVars.Size = new System.Drawing.Size(225, 22);
            this.IndependentVars.TabIndex = 3;
            this.IndependentVars.Text = "";
            // 
            // updTickCount
            // 
            this.updTickCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.updTickCount.Location = new System.Drawing.Point(261, 7);
            this.updTickCount.Name = "updTickCount";
            this.updTickCount.Size = new System.Drawing.Size(49, 20);
            this.updTickCount.TabIndex = 1;
            // 
            // btnTick
            // 
            this.btnTick.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnTick.Location = new System.Drawing.Point(316, 4);
            this.btnTick.Name = "btnTick";
            this.btnTick.Size = new System.Drawing.Size(75, 23);
            this.btnTick.TabIndex = 0;
            this.btnTick.Text = "Tick";
            this.btnTick.UseVisualStyleBackColor = true;
            // 
            // tabEditor
            // 
            this.tabEditor.Controls.Add(this.tabPage1);
            this.tabEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabEditor.Location = new System.Drawing.Point(3, 3);
            this.tabEditor.Name = "tabEditor";
            this.tabEditor.SelectedIndex = 0;
            this.tabEditor.Size = new System.Drawing.Size(394, 558);
            this.tabEditor.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(386, 532);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // outputBrowser
            // 
            this.outputBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputBrowser.Location = new System.Drawing.Point(403, 3);
            this.outputBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.outputBrowser.Name = "outputBrowser";
            this.outputBrowser.Size = new System.Drawing.Size(394, 558);
            this.outputBrowser.TabIndex = 3;
            // 
            // DisplayVertical
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.pnlMain);
            this.Name = "DisplayVertical";
            this.Size = new System.Drawing.Size(800, 600);
            this.pnlMain.ResumeLayout(false);
            this.pnlEditorControls.ResumeLayout(false);
            this.pnlOutputControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updTickCount)).EndInit();
            this.tabEditor.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel pnlMain;
        private System.Windows.Forms.Panel pnlEditorControls;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Panel pnlOutputControls;
        private System.Windows.Forms.NumericUpDown updTickCount;
        private System.Windows.Forms.Button btnTick;
        public System.Windows.Forms.TabControl tabEditor;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox IndependentVars;
        public System.Windows.Forms.WebBrowser outputBrowser;
    }
}
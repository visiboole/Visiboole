using System.Windows.Forms;

namespace VisiBoole.Views
{
	partial class DisplayRun
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
            this.pnlOutputControls = new System.Windows.Forms.Panel();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.btnMultiTick = new System.Windows.Forms.Button();
            this.btnTick = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.pnlMain.SuspendLayout();
            this.pnlOutputControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlMain.ColumnCount = 1;
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlMain.Controls.Add(this.pnlOutputControls, 0, 0);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.RowCount = 3;
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.pnlMain.Size = new System.Drawing.Size(800, 600);
            this.pnlMain.TabIndex = 1;
            // 
            // pnlOutputControls
            // 
            this.pnlOutputControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlOutputControls.Controls.Add(this.numericUpDown1);
            this.pnlOutputControls.Controls.Add(this.btnMultiTick);
            this.pnlOutputControls.Controls.Add(this.btnTick);
            this.pnlOutputControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOutputControls.Location = new System.Drawing.Point(0, 0);
            this.pnlOutputControls.Margin = new System.Windows.Forms.Padding(0);
            this.pnlOutputControls.Name = "pnlOutputControls";
            this.pnlOutputControls.Size = new System.Drawing.Size(800, 27);
            this.pnlOutputControls.TabIndex = 0;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numericUpDown1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numericUpDown1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericUpDown1.Location = new System.Drawing.Point(129, 3);
            this.numericUpDown1.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDown1.MaximumSize = new System.Drawing.Size(60, 0);
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(60, 31);
            this.numericUpDown1.TabIndex = 2;
            this.numericUpDown1.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // btnMultiTick
            // 
            this.btnMultiTick.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnMultiTick.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnMultiTick.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnMultiTick.Location = new System.Drawing.Point(65, 2);
            this.btnMultiTick.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.btnMultiTick.MaximumSize = new System.Drawing.Size(60, 22);
            this.btnMultiTick.Name = "btnMultiTick";
            this.btnMultiTick.Size = new System.Drawing.Size(60, 22);
            this.btnMultiTick.TabIndex = 1;
            this.btnMultiTick.Text = "X Ticks";
            this.btnMultiTick.UseVisualStyleBackColor = true;
            this.btnMultiTick.Click += new System.EventHandler(this.btnMultiTick_Click);
            // 
            // btnTick
            // 
            this.btnTick.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnTick.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnTick.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnTick.Location = new System.Drawing.Point(2, 2);
            this.btnTick.Margin = new System.Windows.Forms.Padding(0);
            this.btnTick.MaximumSize = new System.Drawing.Size(60, 22);
            this.btnTick.Name = "btnTick";
            this.btnTick.Size = new System.Drawing.Size(60, 22);
            this.btnTick.TabIndex = 0;
            this.btnTick.Text = "Tick";
            this.btnTick.UseVisualStyleBackColor = true;
            this.btnTick.Click += new System.EventHandler(this.btnTick_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(786, 539);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // DisplayRun
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "DisplayRun";
            this.Size = new System.Drawing.Size(800, 600);
            this.pnlMain.ResumeLayout(false);
            this.pnlOutputControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel pnlMain;
        private Panel pnlOutputControls;
        private NumericUpDown numericUpDown1;
        private Button btnMultiTick;
        private Button btnTick;
        private TabPage tabPage1;
    }
}

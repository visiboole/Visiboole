using System.Windows.Forms;

namespace VisiBoole.Views
{
	partial class MainWindow
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Designs:");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.closeDesignToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.increaseFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decreaseFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.lightThemeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkThemeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.colorBlindModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simulationCommentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syntaxDocumentationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.NavTree = new System.Windows.Forms.TreeView();
            this.OpenFileLinkLabel = new System.Windows.Forms.LinkLabel();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.newIcon = new System.Windows.Forms.ToolStripMenuItem();
            this.openIcon = new System.Windows.Forms.ToolStripMenuItem();
            this.saveIcon = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAllIcon = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.runModeToggle = new System.Windows.Forms.ToolStripMenuItem();
            this.editModeToggle = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.MainLayoutPanel.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(959, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "MainMenu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator2,
            this.closeDesignToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            this.fileToolStripMenuItem.ToolTipText = "New File (Ctrl+N)";
            this.fileToolStripMenuItem.DropDownClosed += new System.EventHandler(this.MenuDropDownClosedEvent);
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.MenuDropDownOpeningEvent);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewFileMenuClick);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenFileMenuClick);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(143, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveFileMenuClick);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveAsToolStripMenuItem.Text = "Save &As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsFileMenuClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(143, 6);
            // 
            // closeDesignToolStripMenuItem
            // 
            this.closeDesignToolStripMenuItem.Enabled = false;
            this.closeDesignToolStripMenuItem.Name = "closeDesignToolStripMenuItem";
            this.closeDesignToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.closeDesignToolStripMenuItem.Text = "&Close";
            this.closeDesignToolStripMenuItem.Click += new System.EventHandler(this.CloseFileMenuClick);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitApplicationMenuClick);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator3,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator4,
            this.selectAllToolStripMenuItem});
            this.editToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.editToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            this.editToolStripMenuItem.DropDownClosed += new System.EventHandler(this.MenuDropDownClosedEvent);
            this.editToolStripMenuItem.DropDownOpening += new System.EventHandler(this.MenuDropDownOpeningEvent);
            this.editToolStripMenuItem.Click += new System.EventHandler(this.EditMenuClick);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Enabled = false;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Z";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.undoToolStripMenuItem.Text = "&Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.UndoTextMenuClick);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Enabled = false;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Y";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.redoToolStripMenuItem.Text = "&Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.RedoTextMenuClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(161, 6);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Enabled = false;
            this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+X";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.cutToolStripMenuItem.Text = "Cu&t";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.CutTextMenuClick);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Enabled = false;
            this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.copyToolStripMenuItem.Text = "&Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CopyTextMenuClick);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Enabled = false;
            this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+V";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.pasteToolStripMenuItem.Text = "&Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.PasteTextEvent);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(161, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Enabled = false;
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+A";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.selectAllToolStripMenuItem.Text = "Select &All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.SelectAllTextEvent);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.increaseFontToolStripMenuItem,
            this.decreaseFontToolStripMenuItem,
            this.toolStripSeparator6,
            this.lightThemeToolStripMenuItem,
            this.darkThemeToolStripMenuItem,
            this.toolStripSeparator5,
            this.colorBlindModeToolStripMenuItem,
            this.simulationCommentsToolStripMenuItem});
            this.viewToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.viewToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            this.viewToolStripMenuItem.DropDownClosed += new System.EventHandler(this.MenuDropDownClosedEvent);
            this.viewToolStripMenuItem.DropDownOpening += new System.EventHandler(this.MenuDropDownOpeningEvent);
            // 
            // increaseFontToolStripMenuItem
            // 
            this.increaseFontToolStripMenuItem.Enabled = false;
            this.increaseFontToolStripMenuItem.Name = "increaseFontToolStripMenuItem";
            this.increaseFontToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+";
            this.increaseFontToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Oemplus)));
            this.increaseFontToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.increaseFontToolStripMenuItem.Text = "Increase Font";
            this.increaseFontToolStripMenuItem.Click += new System.EventHandler(this.IncreaseFontMenuClick);
            // 
            // decreaseFontToolStripMenuItem
            // 
            this.decreaseFontToolStripMenuItem.Enabled = false;
            this.decreaseFontToolStripMenuItem.Name = "decreaseFontToolStripMenuItem";
            this.decreaseFontToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl-";
            this.decreaseFontToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemMinus)));
            this.decreaseFontToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.decreaseFontToolStripMenuItem.Text = "Decrease Font";
            this.decreaseFontToolStripMenuItem.Click += new System.EventHandler(this.DecreaseFontMenuClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(223, 6);
            // 
            // lightThemeToolStripMenuItem
            // 
            this.lightThemeToolStripMenuItem.Name = "lightThemeToolStripMenuItem";
            this.lightThemeToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.lightThemeToolStripMenuItem.Text = "Light Theme";
            this.lightThemeToolStripMenuItem.Click += new System.EventHandler(this.LightThemeMenuClick);
            // 
            // darkThemeToolStripMenuItem
            // 
            this.darkThemeToolStripMenuItem.Name = "darkThemeToolStripMenuItem";
            this.darkThemeToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.darkThemeToolStripMenuItem.Text = "Dark Theme";
            this.darkThemeToolStripMenuItem.Click += new System.EventHandler(this.DarkThemeMenuClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(223, 6);
            // 
            // colorBlindModeToolStripMenuItem
            // 
            this.colorBlindModeToolStripMenuItem.CheckOnClick = true;
            this.colorBlindModeToolStripMenuItem.Name = "colorBlindModeToolStripMenuItem";
            this.colorBlindModeToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.colorBlindModeToolStripMenuItem.Text = "Toggle Colorblind Mode";
            this.colorBlindModeToolStripMenuItem.Click += new System.EventHandler(this.ColorblindModeMenuClick);
            // 
            // simulationCommentsToolStripMenuItem
            // 
            this.simulationCommentsToolStripMenuItem.CheckOnClick = true;
            this.simulationCommentsToolStripMenuItem.Name = "simulationCommentsToolStripMenuItem";
            this.simulationCommentsToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.simulationCommentsToolStripMenuItem.Text = "Toggle Simulator Comments";
            this.simulationCommentsToolStripMenuItem.Click += new System.EventHandler(this.SimulatorCommentsMenuClick);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.syntaxDocumentationToolStripMenuItem});
            this.helpToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            this.helpToolStripMenuItem.DropDownClosed += new System.EventHandler(this.MenuDropDownClosedEvent);
            this.helpToolStripMenuItem.DropDownOpening += new System.EventHandler(this.MenuDropDownOpeningEvent);
            // 
            // syntaxDocumentationToolStripMenuItem
            // 
            this.syntaxDocumentationToolStripMenuItem.Name = "syntaxDocumentationToolStripMenuItem";
            this.syntaxDocumentationToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.syntaxDocumentationToolStripMenuItem.Text = "VisiBoole Syntax";
            this.syntaxDocumentationToolStripMenuItem.Click += new System.EventHandler(this.SyntaxDocumentationMenuClick);
            // 
            // MainLayoutPanel
            // 
            this.MainLayoutPanel.BackColor = System.Drawing.Color.Transparent;
            this.MainLayoutPanel.ColumnCount = 2;
            this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 153F));
            this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayoutPanel.Controls.Add(this.NavTree, 0, 0);
            this.MainLayoutPanel.Controls.Add(this.OpenFileLinkLabel, 1, 0);
            this.MainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayoutPanel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainLayoutPanel.Location = new System.Drawing.Point(0, 48);
            this.MainLayoutPanel.Margin = new System.Windows.Forms.Padding(1);
            this.MainLayoutPanel.Name = "MainLayoutPanel";
            this.MainLayoutPanel.RowCount = 1;
            this.MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayoutPanel.Size = new System.Drawing.Size(959, 523);
            this.MainLayoutPanel.TabIndex = 2;
            // 
            // NavTree
            // 
            this.NavTree.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.NavTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NavTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NavTree.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NavTree.ForeColor = System.Drawing.Color.DodgerBlue;
            this.NavTree.FullRowSelect = true;
            this.NavTree.HideSelection = false;
            this.NavTree.Indent = 5;
            this.NavTree.ItemHeight = 18;
            this.NavTree.Location = new System.Drawing.Point(1, 1);
            this.NavTree.Margin = new System.Windows.Forms.Padding(1);
            this.NavTree.Name = "NavTree";
            treeNode1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            treeNode1.ForeColor = System.Drawing.Color.DodgerBlue;
            treeNode1.Name = "Explorer";
            treeNode1.NodeFont = new System.Drawing.Font("Tahoma", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            treeNode1.Text = "Designs:";
            this.NavTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.NavTree.ShowLines = false;
            this.NavTree.ShowPlusMinus = false;
            this.NavTree.ShowRootLines = false;
            this.NavTree.Size = new System.Drawing.Size(151, 521);
            this.NavTree.TabIndex = 0;
            this.NavTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.NavTree_NodeMouseDoubleClick);
            // 
            // OpenFileLinkLabel
            // 
            this.OpenFileLinkLabel.ActiveLinkColor = System.Drawing.Color.RoyalBlue;
            this.OpenFileLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenFileLinkLabel.AutoSize = true;
            this.OpenFileLinkLabel.Font = new System.Drawing.Font("Tahoma", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenFileLinkLabel.LinkColor = System.Drawing.Color.DodgerBlue;
            this.OpenFileLinkLabel.Location = new System.Drawing.Point(154, 0);
            this.OpenFileLinkLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.OpenFileLinkLabel.Name = "OpenFileLinkLabel";
            this.OpenFileLinkLabel.Size = new System.Drawing.Size(804, 523);
            this.OpenFileLinkLabel.TabIndex = 2;
            this.OpenFileLinkLabel.TabStop = true;
            this.OpenFileLinkLabel.Text = "Open File";
            this.OpenFileLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.OpenFileLinkLabel.VisitedLinkColor = System.Drawing.Color.DodgerBlue;
            this.OpenFileLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenFileLinkClick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "VisiBoole (*.vbi) File|*.vbi";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.FileName = "newFile1.vbi";
            this.saveFileDialog1.Filter = "VisiBoole (*.vbi) File|*.vbi";
            // 
            // menuStrip2
            // 
            this.menuStrip2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newIcon,
            this.openIcon,
            this.saveIcon,
            this.saveAllIcon,
            this.undoToolStripMenuItem1,
            this.redoToolStripMenuItem1,
            this.runModeToggle,
            this.editModeToggle});
            this.menuStrip2.Location = new System.Drawing.Point(0, 24);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.ShowItemToolTips = true;
            this.menuStrip2.Size = new System.Drawing.Size(959, 24);
            this.menuStrip2.TabIndex = 3;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // newIcon
            // 
            this.newIcon.Image = ((System.Drawing.Image)(resources.GetObject("newIcon.Image")));
            this.newIcon.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.newIcon.Name = "newIcon";
            this.newIcon.Size = new System.Drawing.Size(28, 20);
            this.newIcon.ToolTipText = "New (Ctrl+N)";
            this.newIcon.Click += new System.EventHandler(this.NewFileMenuClick);
            // 
            // openIcon
            // 
            this.openIcon.Image = ((System.Drawing.Image)(resources.GetObject("openIcon.Image")));
            this.openIcon.Name = "openIcon";
            this.openIcon.Size = new System.Drawing.Size(28, 20);
            this.openIcon.ToolTipText = "Open (Ctrl+O)";
            this.openIcon.Click += new System.EventHandler(this.OpenFileMenuClick);
            // 
            // saveIcon
            // 
            this.saveIcon.Enabled = false;
            this.saveIcon.Image = ((System.Drawing.Image)(resources.GetObject("saveIcon.Image")));
            this.saveIcon.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.saveIcon.Name = "saveIcon";
            this.saveIcon.Size = new System.Drawing.Size(28, 20);
            this.saveIcon.ToolTipText = "Save (Ctrl+S)";
            this.saveIcon.Click += new System.EventHandler(this.SaveFileMenuClick);
            // 
            // saveAllIcon
            // 
            this.saveAllIcon.Enabled = false;
            this.saveAllIcon.Image = ((System.Drawing.Image)(resources.GetObject("saveAllIcon.Image")));
            this.saveAllIcon.Name = "saveAllIcon";
            this.saveAllIcon.Size = new System.Drawing.Size(28, 20);
            this.saveAllIcon.ToolTipText = "Save All";
            this.saveAllIcon.Click += new System.EventHandler(this.SaveAllFileMenuClick);
            // 
            // undoToolStripMenuItem1
            // 
            this.undoToolStripMenuItem1.Enabled = false;
            this.undoToolStripMenuItem1.Image = global::VisiBoole.Properties.Resources.Undo;
            this.undoToolStripMenuItem1.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.undoToolStripMenuItem1.Name = "undoToolStripMenuItem1";
            this.undoToolStripMenuItem1.Size = new System.Drawing.Size(28, 20);
            this.undoToolStripMenuItem1.ToolTipText = "Undo (Ctrl+Z)";
            this.undoToolStripMenuItem1.Click += new System.EventHandler(this.UndoTextMenuClick);
            // 
            // redoToolStripMenuItem1
            // 
            this.redoToolStripMenuItem1.Enabled = false;
            this.redoToolStripMenuItem1.Image = global::VisiBoole.Properties.Resources.Redo;
            this.redoToolStripMenuItem1.Name = "redoToolStripMenuItem1";
            this.redoToolStripMenuItem1.Size = new System.Drawing.Size(28, 20);
            this.redoToolStripMenuItem1.ToolTipText = "Redo (Ctrl+Y)";
            this.redoToolStripMenuItem1.Click += new System.EventHandler(this.RedoTextMenuClick);
            // 
            // runModeToggle
            // 
            this.runModeToggle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.runModeToggle.Enabled = false;
            this.runModeToggle.ForeColor = System.Drawing.Color.White;
            this.runModeToggle.Image = ((System.Drawing.Image)(resources.GetObject("runModeToggle.Image")));
            this.runModeToggle.Margin = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.runModeToggle.Name = "runModeToggle";
            this.runModeToggle.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.runModeToggle.Size = new System.Drawing.Size(56, 20);
            this.runModeToggle.Text = "Run";
            this.runModeToggle.ToolTipText = "Run Design";
            this.runModeToggle.Click += new System.EventHandler(this.RunButtonClick);
            // 
            // editModeToggle
            // 
            this.editModeToggle.Enabled = false;
            this.editModeToggle.ForeColor = System.Drawing.Color.White;
            this.editModeToggle.Image = global::VisiBoole.Properties.Resources.Stop;
            this.editModeToggle.Name = "editModeToggle";
            this.editModeToggle.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.editModeToggle.Size = new System.Drawing.Size(55, 20);
            this.editModeToggle.Text = "Edit";
            this.editModeToggle.ToolTipText = "Edit Design";
            this.editModeToggle.Click += new System.EventHandler(this.EditButtonClick);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
            this.ClientSize = new System.Drawing.Size(959, 571);
            this.Controls.Add(this.MainLayoutPanel);
            this.Controls.Add(this.menuStrip2);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip2;
            this.MinimumSize = new System.Drawing.Size(560, 350);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VisiBoole - Visualizing HDL";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindowFormClosingEvent);
            this.Load += new System.EventHandler(this.MainWindowFormLoadEvent);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.MainLayoutPanel.ResumeLayout(false);
            this.MainLayoutPanel.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.TableLayoutPanel MainLayoutPanel;
		private System.Windows.Forms.LinkLabel OpenFileLinkLabel;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem increaseFontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decreaseFontToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem lightThemeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkThemeToolStripMenuItem;
        private MenuStrip menuStrip2;
        private ToolStripMenuItem editModeToggle;
        private ToolStripMenuItem runModeToggle;
        private ToolStripMenuItem openIcon;
        private ToolStripMenuItem saveIcon;
        private ToolStripMenuItem saveAllIcon;
        private ToolStripMenuItem newIcon;
        private ToolStripMenuItem closeDesignToolStripMenuItem;
        private ToolStripMenuItem undoToolStripMenuItem1;
        private ToolStripMenuItem redoToolStripMenuItem1;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem syntaxDocumentationToolStripMenuItem;
        private TreeView NavTree;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem colorBlindModeToolStripMenuItem;
        private ToolStripMenuItem simulationCommentsToolStripMenuItem;
    }
}
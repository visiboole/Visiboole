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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("My SubDesigns");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.variablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syntaxDocumentationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.OpenFileLinkLabel = new System.Windows.Forms.LinkLabel();
            this.NavTree = new System.Windows.Forms.TreeView();
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
            this.menuStrip1.BackColor = System.Drawing.Color.DarkGray;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1120, 24);
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
            this.toolStripSeparator1,
            this.printToolStripMenuItem,
            this.printPreviewToolStripMenuItem,
            this.toolStripSeparator2,
            this.closeDesignToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            this.fileToolStripMenuItem.ToolTipText = "New File (Ctrl+N)";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewFileEvent);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenFileEvent);
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
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveFileEvent);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveAsToolStripMenuItem.Text = "Save &As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsFileEvent);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(143, 6);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Enabled = false;
            this.printToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.printToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.printToolStripMenuItem.Text = "&Print";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.PrintFileEvent);
            // 
            // printPreviewToolStripMenuItem
            // 
            this.printPreviewToolStripMenuItem.Enabled = false;
            this.printPreviewToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            this.printPreviewToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.printPreviewToolStripMenuItem.Text = "Print Pre&view";
            this.printPreviewToolStripMenuItem.Click += new System.EventHandler(this.PrintPreviewFileEvent);
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
            this.closeDesignToolStripMenuItem.Text = "Close";
            this.closeDesignToolStripMenuItem.Click += new System.EventHandler(this.CloseFileEvent);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitApplicationEvent);
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
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Enabled = false;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Z";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.undoToolStripMenuItem.Text = "&Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.UndoTextEvent);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Enabled = false;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Y";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.redoToolStripMenuItem.Text = "&Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.RedoTextEvent);
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
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.CutTextEvent);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Enabled = false;
            this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.copyToolStripMenuItem.Text = "&Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CopyTextEvent);
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
            this.darkThemeToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // increaseFontToolStripMenuItem
            // 
            this.increaseFontToolStripMenuItem.Enabled = false;
            this.increaseFontToolStripMenuItem.Name = "increaseFontToolStripMenuItem";
            this.increaseFontToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+";
            this.increaseFontToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Oemplus)));
            this.increaseFontToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.increaseFontToolStripMenuItem.Text = "Increase Font";
            this.increaseFontToolStripMenuItem.Click += new System.EventHandler(this.IncreaseFontEvent);
            // 
            // decreaseFontToolStripMenuItem
            // 
            this.decreaseFontToolStripMenuItem.Enabled = false;
            this.decreaseFontToolStripMenuItem.Name = "decreaseFontToolStripMenuItem";
            this.decreaseFontToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl-";
            this.decreaseFontToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemMinus)));
            this.decreaseFontToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.decreaseFontToolStripMenuItem.Text = "Decrease Font";
            this.decreaseFontToolStripMenuItem.Click += new System.EventHandler(this.DecreaseFontEvent);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(176, 6);
            // 
            // lightThemeToolStripMenuItem
            // 
            this.lightThemeToolStripMenuItem.Name = "lightThemeToolStripMenuItem";
            this.lightThemeToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.lightThemeToolStripMenuItem.Text = "Light Theme";
            this.lightThemeToolStripMenuItem.Click += new System.EventHandler(this.LightThemeEvent);
            // 
            // darkThemeToolStripMenuItem
            // 
            this.darkThemeToolStripMenuItem.Name = "darkThemeToolStripMenuItem";
            this.darkThemeToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.darkThemeToolStripMenuItem.Text = "Dark Theme";
            this.darkThemeToolStripMenuItem.Click += new System.EventHandler(this.DarkThemeEvent);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.variablesToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // variablesToolStripMenuItem
            // 
            this.variablesToolStripMenuItem.Enabled = false;
            this.variablesToolStripMenuItem.Name = "variablesToolStripMenuItem";
            this.variablesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.variablesToolStripMenuItem.Text = "Variables";
            this.variablesToolStripMenuItem.Click += new System.EventHandler(this.variablesToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.syntaxDocumentationToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // syntaxDocumentationToolStripMenuItem
            // 
            this.syntaxDocumentationToolStripMenuItem.Name = "syntaxDocumentationToolStripMenuItem";
            this.syntaxDocumentationToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.syntaxDocumentationToolStripMenuItem.Text = "VisiBoole Syntax";
            this.syntaxDocumentationToolStripMenuItem.Click += new System.EventHandler(this.syntaxDocumentationToolStripMenuItem_Click);
            // 
            // MainLayoutPanel
            // 
            this.MainLayoutPanel.BackColor = System.Drawing.Color.Transparent;
            this.MainLayoutPanel.ColumnCount = 2;
            this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.5F));
            this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 82.5F));
            this.MainLayoutPanel.Controls.Add(this.OpenFileLinkLabel, 0, 0);
            this.MainLayoutPanel.Controls.Add(this.NavTree, 0, 0);
            this.MainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayoutPanel.Font = new System.Drawing.Font("Garamond", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainLayoutPanel.Location = new System.Drawing.Point(0, 48);
            this.MainLayoutPanel.Margin = new System.Windows.Forms.Padding(1);
            this.MainLayoutPanel.Name = "MainLayoutPanel";
            this.MainLayoutPanel.RowCount = 1;
            this.MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayoutPanel.Size = new System.Drawing.Size(1120, 611);
            this.MainLayoutPanel.TabIndex = 2;
            // 
            // OpenFileLinkLabel
            // 
            this.OpenFileLinkLabel.AutoSize = true;
            this.OpenFileLinkLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.OpenFileLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OpenFileLinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(226)))), ((int)(((byte)(85)))));
            this.OpenFileLinkLabel.Location = new System.Drawing.Point(197, 0);
            this.OpenFileLinkLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.OpenFileLinkLabel.Name = "OpenFileLinkLabel";
            this.OpenFileLinkLabel.Size = new System.Drawing.Size(922, 611);
            this.OpenFileLinkLabel.TabIndex = 2;
            this.OpenFileLinkLabel.TabStop = true;
            this.OpenFileLinkLabel.Text = "Open a VisiBoole project or file to get started";
            this.OpenFileLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.OpenFileLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenFileLinkEvent);
            // 
            // NavTree
            // 
            this.NavTree.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(77)))), ((int)(((byte)(81)))));
            this.NavTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NavTree.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(226)))), ((int)(((byte)(85)))));
            this.NavTree.FullRowSelect = true;
            this.NavTree.HideSelection = false;
            this.NavTree.Location = new System.Drawing.Point(1, 1);
            this.NavTree.Margin = new System.Windows.Forms.Padding(1);
            this.NavTree.Name = "NavTree";
            treeNode1.Name = "Node0";
            treeNode1.Text = "My SubDesigns";
            this.NavTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.NavTree.ShowLines = false;
            this.NavTree.Size = new System.Drawing.Size(194, 609);
            this.NavTree.TabIndex = 0;
            this.NavTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.NavTree_NodeMouseDoubleClick);
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
            this.menuStrip2.BackColor = System.Drawing.Color.DarkGray;
            this.menuStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
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
            this.menuStrip2.Size = new System.Drawing.Size(1120, 24);
            this.menuStrip2.TabIndex = 3;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // newIcon
            // 
            this.newIcon.Image = ((System.Drawing.Image)(resources.GetObject("newIcon.Image")));
            this.newIcon.Name = "newIcon";
            this.newIcon.Size = new System.Drawing.Size(28, 20);
            this.newIcon.ToolTipText = "New Design (Ctrl+N)";
            this.newIcon.Click += new System.EventHandler(this.NewFileEvent);
            // 
            // openIcon
            // 
            this.openIcon.Image = ((System.Drawing.Image)(resources.GetObject("openIcon.Image")));
            this.openIcon.Name = "openIcon";
            this.openIcon.Size = new System.Drawing.Size(28, 20);
            this.openIcon.ToolTipText = "Open Design (Ctrl+O)";
            this.openIcon.Click += new System.EventHandler(this.OpenFileEvent);
            // 
            // saveIcon
            // 
            this.saveIcon.Enabled = false;
            this.saveIcon.Image = ((System.Drawing.Image)(resources.GetObject("saveIcon.Image")));
            this.saveIcon.Name = "saveIcon";
            this.saveIcon.Size = new System.Drawing.Size(28, 20);
            this.saveIcon.ToolTipText = "Save Design (Ctrl+S)";
            this.saveIcon.Click += new System.EventHandler(this.SaveFileEvent);
            // 
            // saveAllIcon
            // 
            this.saveAllIcon.Enabled = false;
            this.saveAllIcon.Image = ((System.Drawing.Image)(resources.GetObject("saveAllIcon.Image")));
            this.saveAllIcon.Name = "saveAllIcon";
            this.saveAllIcon.Size = new System.Drawing.Size(28, 20);
            this.saveAllIcon.ToolTipText = "Save All Designs";
            this.saveAllIcon.Click += new System.EventHandler(this.SaveAllFileEvent);
            // 
            // undoToolStripMenuItem1
            // 
            this.undoToolStripMenuItem1.Enabled = false;
            this.undoToolStripMenuItem1.Image = global::VisiBoole.Properties.Resources.Undo;
            this.undoToolStripMenuItem1.Name = "undoToolStripMenuItem1";
            this.undoToolStripMenuItem1.Size = new System.Drawing.Size(28, 20);
            this.undoToolStripMenuItem1.ToolTipText = "Undo (Ctrl+Z)";
            this.undoToolStripMenuItem1.Click += new System.EventHandler(this.UndoTextEvent);
            // 
            // redoToolStripMenuItem1
            // 
            this.redoToolStripMenuItem1.Enabled = false;
            this.redoToolStripMenuItem1.Image = global::VisiBoole.Properties.Resources.Redo;
            this.redoToolStripMenuItem1.Name = "redoToolStripMenuItem1";
            this.redoToolStripMenuItem1.Size = new System.Drawing.Size(28, 20);
            this.redoToolStripMenuItem1.ToolTipText = "Redo (Ctrl+Y)";
            this.redoToolStripMenuItem1.Click += new System.EventHandler(this.RedoTextEvent);
            // 
            // runModeToggle
            // 
            this.runModeToggle.Enabled = false;
            this.runModeToggle.Image = ((System.Drawing.Image)(resources.GetObject("runModeToggle.Image")));
            this.runModeToggle.Name = "runModeToggle";
            this.runModeToggle.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.runModeToggle.Size = new System.Drawing.Size(56, 20);
            this.runModeToggle.Text = "Run";
            this.runModeToggle.ToolTipText = "Run Design";
            this.runModeToggle.Click += new System.EventHandler(this.RunToggleEvent);
            // 
            // editModeToggle
            // 
            this.editModeToggle.Enabled = false;
            this.editModeToggle.Image = global::VisiBoole.Properties.Resources.Stop;
            this.editModeToggle.Name = "editModeToggle";
            this.editModeToggle.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.editModeToggle.Size = new System.Drawing.Size(55, 20);
            this.editModeToggle.Text = "Edit";
            this.editModeToggle.ToolTipText = "Edit Design";
            this.editModeToggle.Click += new System.EventHandler(this.EditToggleEvent);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(77)))), ((int)(((byte)(81)))));
            this.ClientSize = new System.Drawing.Size(1120, 659);
            this.Controls.Add(this.MainLayoutPanel);
            this.Controls.Add(this.menuStrip2);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip2;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VisiBoole - Visualizing HDL";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
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
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
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
		private System.Windows.Forms.TreeView NavTree;
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
        private ToolStripMenuItem debugToolStripMenuItem;
        private ToolStripMenuItem variablesToolStripMenuItem;
    }
}
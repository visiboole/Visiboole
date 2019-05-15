/*
 * Copyright (C) 2019 John Devore
 * Copyright (C) 2019 Chance Henney, Juwan Moore, William Van Cleve
 * Copyright (C) 2017 Matthew Segraves, Zachary Terwort, Zachary Cleary
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
 * along with this program located at "\Visiboole\license.txt".
 * If not, see <http://www.gnu.org/licenses/>
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using VisiBoole.Controllers;
using VisiBoole.Models;

namespace VisiBoole.Views
{
    /// <summary>
    /// The MainWindow of this application
    /// </summary>
    public partial class MainWindow : Form, IMainWindow
    {
        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground { get { return Color.FromArgb(33, 33, 33); } }

            public override Color ImageMarginGradientBegin { get { return Color.FromArgb(33, 33, 33); } }

            public override Color ImageMarginGradientMiddle { get { return Color.FromArgb(33, 33, 33); } }

            public override Color ImageMarginGradientEnd { get { return Color.FromArgb(33, 33, 33); } }

            public override Color MenuBorder { get { return Color.FromArgb(33, 33, 33); } }

            public override Color MenuItemBorder { get { return Color.FromArgb(66, 66, 66); } }

            public override Color MenuItemSelected { get { return Color.FromArgb(99, 99, 99); } }

            public override Color MenuStripGradientBegin { get { return Color.FromArgb(33, 33, 33); } }

            public override Color MenuStripGradientEnd { get { return Color.FromArgb(33, 33, 33); } }

            public override Color MenuItemSelectedGradientBegin { get { return Color.FromArgb(99, 99, 99); } }

            public override Color MenuItemSelectedGradientEnd { get { return Color.FromArgb(99, 99, 99); } }

            public override Color MenuItemPressedGradientBegin { get { return Color.FromArgb(99, 99, 99); } }

            public override Color MenuItemPressedGradientEnd { get { return Color.FromArgb(99, 99, 99); } }

            public override Color SeparatorLight { get { return Color.FromArgb(99, 99, 99); } }

            public override Color SeparatorDark { get { return Color.FromArgb(99, 99, 99); } }

            public override Color ButtonCheckedGradientBegin { get { return Color.Red; } }

            public override Color ButtonCheckedGradientMiddle { get { return Color.Red; } }

            public override Color ButtonCheckedGradientEnd { get { return Color.Red; } }

            public override Color ButtonCheckedHighlight { get { return Color.FromArgb(99, 99, 99); } }

            public override Color CheckBackground { get { return Color.FromArgb(99, 99, 99); } }

            public override Color CheckPressedBackground { get { return Color.FromArgb(99, 99, 99); } }

            public override Color CheckSelectedBackground { get { return Color.FromArgb(99, 99, 99); } }
        }


        /// <summary>
        /// Handle to the MainWindowController for this view
        /// </summary>
        private IMainWindowController MainWindowController;

        #region "Class Initialization"

        /// <summary>
        /// Constructs an instance of MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            NavTree.NodeMouseClick += (sender, args) =>
            {
                NavTree.SelectedNode = args.Node;
                if (args.Button == MouseButtons.Right && args.Node.Level != 0)
                {
                    ContextMenu contextMenu = new ContextMenu();
                    MenuItem item = new MenuItem("Save Design");
                    item.Click += new EventHandler(SaveDesignContextMenuClick);
                    item.Enabled = runModeToggle.Enabled;
                    contextMenu.MenuItems.Add(item);
                    item = new MenuItem("Close Design");
                    item.Click += new EventHandler(CloseDesignContextMenuClick);
                    item.Enabled = runModeToggle.Enabled;
                    contextMenu.MenuItems.Add(item);
                    item = new MenuItem("Close All Except This");
                    item.Click += new EventHandler(CloseAllExceptMenuClick);
                    item.Enabled = runModeToggle.Enabled;
                    contextMenu.MenuItems.Add(item);
                    args.Node.ContextMenu = contextMenu;
                }
            };
            NavTree.HideSelection = true;
            NavTree.SelectedNode = null;
            menuStrip1.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                for (int i = 0; i < item.DropDownItems.Count; i++)
                {
                    item.DropDownItems[i].ForeColor = Color.WhiteSmoke;
                }
            }
            menuStrip2.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
            foreach (ToolStripMenuItem item in menuStrip2.Items)
            {
                for (int i = 0; i < item.DropDownItems.Count; i++)
                {
                    item.DropDownItems[i].ForeColor = Color.WhiteSmoke;
                }
            }
        }

        /// <summary>
        /// Saves the handle to the MainWindowController for this view
        /// </summary>
        /// <param name="MainWindowController">The handle to the MainWindowController for this view</param>
        public void AttachMainWindowController(IMainWindowController controller)
        {
            MainWindowController = controller;
        }

        #endregion

        #region "Utility Methods"

        /// <summary>
        /// Update buttons and icons based on the display
        /// </summary>
        /// <param name="current"></param>
        private void UpdateControls(DisplayType displayType)
        {
            openIcon.Enabled = displayType == DisplayType.EDIT;
            openToolStripMenuItem.Enabled = displayType == DisplayType.EDIT;
            newIcon.Enabled = displayType == DisplayType.EDIT;
            newToolStripMenuItem.Enabled = displayType == DisplayType.EDIT;
            saveIcon.Enabled = displayType == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0;
            saveAllIcon.Enabled = displayType == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0;
            saveToolStripMenuItem.Enabled = displayType == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0;
            saveAsToolStripMenuItem.Enabled = displayType == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0;
            runModeToggle.Enabled = displayType == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0;
            runStateToolStripMenuItem.Enabled = displayType == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0;
            newStateToolStripMenuItem.Enabled = displayType == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0;
            editModeToggle.Enabled = displayType == DisplayType.RUN;
            closeDesignToolStripMenuItem.Enabled = displayType == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0;
            closeAllDesignToolStripMenuItem.Enabled = displayType == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0;
            increaseFontToolStripMenuItem.Enabled = NavTree.Nodes[0].Nodes.Count > 0;
            decreaseFontToolStripMenuItem.Enabled = NavTree.Nodes[0].Nodes.Count > 0;
            selectAllToolStripMenuItem.Enabled = displayType == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0;

            if (displayType == DisplayType.EDIT && DesignController.ActiveDesign != null)
            {
                if (DesignController.ActiveDesign.Font.Size != Properties.Settings.Default.FontSize)
                {
                    MainWindowController.SetFontSize();
                }

                previousStateToolStripMenuItem.Enabled = MainWindowController.DesignHasParser(DesignController.ActiveDesign.FileName);
                undoToolStripMenuItem.Enabled = DesignController.ActiveDesign.EditHistory.Count > 0;
                undoToolStripMenuItem1.Enabled = DesignController.ActiveDesign.EditHistory.Count > 0;
                redoToolStripMenuItem.Enabled = DesignController.ActiveDesign.UndoHistory.Count > 0;
                redoToolStripMenuItem1.Enabled = DesignController.ActiveDesign.UndoHistory.Count > 0;
                cutToolStripMenuItem.Enabled = DesignController.ActiveDesign.SelectedText.Length > 0;
                copyToolStripMenuItem.Enabled = DesignController.ActiveDesign.SelectedText.Length > 0;
                pasteToolStripMenuItem.Enabled = Clipboard.ContainsText();
            }
            else
            {
                previousStateToolStripMenuItem.Enabled = false;
                undoToolStripMenuItem.Enabled = false;
                undoToolStripMenuItem1.Enabled = false;
                redoToolStripMenuItem.Enabled = false;
                redoToolStripMenuItem1.Enabled = false;
                copyToolStripMenuItem.Enabled = false;
                cutToolStripMenuItem.Enabled = false;
                pasteToolStripMenuItem.Enabled = false;
            }
        }

        /// <summary>
        /// Sets the theme of the application
        /// </summary>
        /// <param name="theme">Theme to set</param>
        private void SetTheme(string theme)
        {
            if (theme == "Light" || theme == "light")
            {
                Properties.Settings.Default.Theme = "Light";
                NavTree.Nodes[0].BackColor = Color.DodgerBlue;
                NavTree.Nodes[0].ForeColor = Color.Black;
                NavTree.BackColor = Color.DodgerBlue;
                NavTree.ForeColor = Color.Black;
                NavTree.HideSelection = true;
                NavTree.SelectedNode = null;
                BackColor = Color.AliceBlue;
                OpenFileLinkLabel.LinkColor = Color.DodgerBlue;

                MainWindowController.SetTheme();
            }
            else
            {
                Properties.Settings.Default.Theme = "Dark";
                NavTree.Nodes[0].BackColor = Color.FromArgb(48, 48, 48);
                NavTree.Nodes[0].ForeColor = Color.DodgerBlue;
                NavTree.BackColor = Color.FromArgb(48, 48, 48);
                NavTree.ForeColor = Color.DodgerBlue;
                NavTree.HideSelection = true;
                NavTree.SelectedNode = null;
                BackColor = Color.FromArgb(66, 66, 66);
                OpenFileLinkLabel.LinkColor = Color.DodgerBlue;

                MainWindowController.SetTheme();
            }
        }

        /// <summary>
        /// Adds a new node in the TreeView
        /// </summary>
        /// <param name="path">The filepath string that will be parsed to obtain the name of this treenode</param>
        public void AddNavTreeNode(string path)
        {
            string fileName = path.Substring(path.LastIndexOf("\\") + 1);
            TreeNode fileNode = new TreeNode(fileName);
            fileNode.Name = fileName;
            NavTree.Nodes[0].Nodes.Add(fileNode);
            NavTree.ExpandAll();
        }

        /// <summary>
        /// Removes a node in the TreeView
        /// </summary>
        /// <param name="name">The name of the node to be removed</param>
        public void RemoveNavTreeNode(string name)
        {
            NavTree.Nodes[0].Nodes.RemoveByKey(name);
        }

        /// <summary>
        /// Swaps two indexes of the nav tree.
        /// </summary>
        /// <param name="srcIndex">Source index</param>
        /// <param name="dstIndex">Destination index</param>
        public void SwapNavTreeNodes(int srcIndex, int dstIndex)
        {
            TreeNode srcNode = NavTree.Nodes[0].Nodes[srcIndex];
            TreeNode dstNode = NavTree.Nodes[0].Nodes[dstIndex];

            if (srcIndex > dstIndex)
            {
                NavTree.Nodes[0].Nodes.RemoveAt(srcIndex);
                NavTree.Nodes[0].Nodes.RemoveAt(dstIndex);
                NavTree.Nodes[0].Nodes.Insert(dstIndex, srcNode);
                NavTree.Nodes[0].Nodes.Insert(srcIndex, dstNode);
            }
            else
            {
                NavTree.Nodes[0].Nodes.RemoveAt(dstIndex);
                NavTree.Nodes[0].Nodes.RemoveAt(srcIndex);
                NavTree.Nodes[0].Nodes.Insert(srcIndex, dstNode);
                NavTree.Nodes[0].Nodes.Insert(dstIndex, srcNode);
            }
        }

        /// <summary>
        /// Loads the given IDisplay
        /// </summary>
        /// <param name="previous">The display to replace</param>
        /// <param name="current">The display to be loaded</param>
        public void LoadDisplay(IDisplay previous, IDisplay current)
        {
            if (!MainLayoutPanel.Controls.Contains((Control)previous))
            {
                // No files have been opened
                MainLayoutPanel.Controls.Remove(OpenFileLinkLabel);
            }
            else
            {
                if ((previous == current) ^ (NavTree.Nodes[0].Nodes.Count > 0))
                {
                    // If either display is same or files have been opened
                    MainLayoutPanel.Controls.Remove((Control)previous);

                    if (NavTree.Nodes[0].Nodes.Count == 0)
                    {
                        MainLayoutPanel.Controls.Add(OpenFileLinkLabel, 1, 0);
                    }
                }
            }

            if (!MainLayoutPanel.Controls.Contains(OpenFileLinkLabel) && !MainLayoutPanel.Controls.Contains((Control)current))
            {
                Control currentControls = (Control)current;
                currentControls.Dock = DockStyle.Fill;
                MainLayoutPanel.Controls.Add(currentControls);
            }

            UpdateControls(MainWindowController.GetCurrentDisplayType()); // Change controls to match the new display
        }

        /// <summary>
        /// Focuses this window.
        /// </summary>
        public void RetrieveFocus()
        {
            NavTree.Focus(); // This foucs will allow all shortcut keys to work
            NavTree.SelectedNode = null;
        }

        #endregion

        #region "Event Handlers"

        /// <summary>
        /// Handles the event that occurs when the light theme menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LightThemeMenuClick(object sender, EventArgs e)
        {
            SetTheme("Light");
        }

        /// <summary>
        /// Handles the event that occurs when the dark theme menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DarkThemeMenuClick(object sender, EventArgs e)
        {
            SetTheme("Dark");
        }

        /// <summary>
        /// Handles the event that occrus when the increase font menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IncreaseFontMenuClick(object sender, EventArgs e)
        {
            Properties.Settings.Default.FontSize += 2;
            MainWindowController.SetFontSize();

            if (editModeToggle.Enabled)
            {
                MainWindowController.RefreshOutput();
            }
        }

        /// <summary>
        /// Handles the event that occrus when the decrease font menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecreaseFontMenuClick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.FontSize > 9)
            {
                Properties.Settings.Default.FontSize -= 2;
                MainWindowController.SetFontSize();

                if (editModeToggle.Enabled)
                {
                    MainWindowController.RefreshOutput();
                }
            }
        }

        /// <summary>
        /// Handles the event that occurs when the colorblind mode menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorblindModeMenuClick(object sender, EventArgs e)
        {
            Properties.Settings.Default.Colorblind = !Properties.Settings.Default.Colorblind; // Flip setting
            if (editModeToggle.Enabled)
            {
                MainWindowController.RefreshOutput();
            }
        }

        /// <summary>
        /// Handles the event that occurs when the simulator comments menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimulatorCommentsMenuClick(object sender, EventArgs e)
        {
            Properties.Settings.Default.SimulationComments = !Properties.Settings.Default.SimulationComments; // Flip setting
            if (editModeToggle.Enabled)
            {
                MainWindowController.RefreshOutput();
            }
        }

        /// <summary>
        /// Handles the event that occurs when the open file link is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileLinkClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFiles();
        }

        /// <summary>
        /// Handles the event that occurs when a open file menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileMenuClick(object sender, EventArgs e)
        {
            OpenFiles();
        }

        /// <summary>
        /// Opens the files selected by the open file dialog.
        /// </summary>
        private void OpenFiles()
        {
            DialogResult response = openFileDialog1.ShowDialog();

            if (response == DialogResult.OK)
            {
                string[] files = openFileDialog1.FileNames;
                foreach (string file in files)
                {
                    MainWindowController.ProcessNewFile(file);
                }

                openFileDialog1.FileName = "";
            }
        }

        /// <summary>
        /// Handles the event that occurs when a new file menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewFileMenuClick(object sender, EventArgs e)
        {
            DialogResult response = saveFileDialog1.ShowDialog();
            if (response != DialogResult.OK)
            {
                return;
            }

            MainWindowController.ProcessNewFile(saveFileDialog1.FileName, true);
            saveFileDialog1.FileName = "newFile1.vbi";
        }

        /// <summary>
        /// Handles the event that occurs when a save file menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFileMenuClick(object sender, EventArgs e)
        {
            MainWindowController.SaveFile();
        }

        /// <summary>
        /// Handles the event that occurs when the save as menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAsFileMenuClick(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = DesignController.ActiveDesign.FileName;
            DialogResult response = saveFileDialog1.ShowDialog();

            if (response == DialogResult.OK)
            {
                MainWindowController.SaveFileAs(saveFileDialog1.FileName);
            }
            saveFileDialog1.FileName = "";
        }

        /// <summary>
        /// Handles the event that occurs when the save all menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAllFileMenuClick(object sender, EventArgs e)
        {
            MainWindowController.SaveFiles();
        }

        /// <summary>
        /// Handles the event that occurs when a node on the treeview was double-clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (editModeToggle.Enabled)
            {
                MainWindowController.SuspendRunDisplay();
                MainWindowController.LoadDisplay(DisplayType.EDIT);
            }
            MainWindowController.SelectFile(e.Node.Name, true);
        }

        /// <summary>
        /// Handles the event that occurs when the run button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunButtonClick(object sender, EventArgs e)
        {
            var currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            MainWindowController.Run();
            Cursor.Current = currentCursor;
        }

        /// <summary>
        /// Handles the event that occurs when the edit button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButtonClick(object sender, EventArgs e)
        {
            var currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            MainWindowController.SuspendRunDisplay();
            MainWindowController.LoadDisplay(DisplayType.EDIT);
            Cursor.Current = currentCursor;
        }

        /// <summary>
        /// Handles the event that occurs when the edit menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditMenuClick(object sender, EventArgs e)
        {
            UpdateControls(MainWindowController.GetCurrentDisplayType());
        }

        /// <summary>
        /// Handles the event that occurs when the undo menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoTextMenuClick(object sender, EventArgs e)
        {
            DesignController.ActiveDesign.UndoTextMenuClick(sender, e);
        }

        /// <summary>
        /// Handles the event that occurs when the redo menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RedoTextMenuClick(object sender, EventArgs e)
        {
            DesignController.ActiveDesign.RedoTextMenuClick(sender, e);
        }

        /// <summary>
        /// Handles the event that occurs when the cut menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CutTextMenuClick(object sender, EventArgs e)
        {
            DesignController.ActiveDesign.CutTextMenuClick(sender, e);
        }

        /// <summary>
        /// Handles the event that occurs when the copy menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyTextMenuClick(object sender, EventArgs e)
        {
            DesignController.ActiveDesign.CopyTextMenuClick(sender, e);
        }

        /// <summary>
        /// Handles the event that occurs when the paste menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasteTextEvent(object sender, EventArgs e)
        {
            DesignController.ActiveDesign.PasteTextMenuClick(sender, e);
        }

        /// <summary>
        /// Handles the event that occurs when the select all menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAllTextEvent(object sender, EventArgs e)
        {
            DesignController.ActiveDesign.SelectAllTextMenuClick(sender, e);
        }

        /// <summary>
        /// Handles the event when the syntax documentation menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyntaxDocumentationMenuClick(object sender, EventArgs e)
        {

            //refocuses syntax window if it exists, refocusing or un-minimizing
            //Creates and displays if doesn't exist
            /*
            Form fc = Application.OpenForms["HelpWindow"];
            if (fc != null)
            {
                fc.WindowState = System.Windows.Forms.FormWindowState.Normal;
                fc.BringToFront();
            }

            else
            {
                HelpWindow hw = new HelpWindow("VisiBoole Syntax", File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Help Documentation", "Syntax.txt")));
                hw.Show();
            }
            */

            new SyntaxWindow().Show();
        }

        /// <summary>
        /// Handles the event that occurs when a close file menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseFileMenuClick(object sender, EventArgs e)
        {
            MainWindowController.CloseFile();
        }

        private void CloseDesignContextMenuClick(object sender, EventArgs e)
        {
            MainWindowController.CloseFile(NavTree.SelectedNode.Name);
        }

        private void SaveDesignContextMenuClick(object sender, EventArgs e)
        {
            MainWindowController.SaveFile(NavTree.SelectedNode.Name);
        }

        /// <summary>
        /// Handles the event that occurs when the user wants to close all files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseAllExceptMenuClick(object sender, EventArgs e)
        {
            MainWindowController.CloseFilesExceptFor(NavTree.SelectedNode.Name);
        }

        /// <summary>
        /// Handles the event that occurs when the user wants to close all files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseAllMenuClick(object sender, EventArgs e)
        {
            MainWindowController.CloseFiles();
        }

        /// <summary>
        /// Handles the event that occurs when the exit menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitApplicationMenuClick(object sender, EventArgs e)
        {
            MainWindowController.CloseFiles();

            if (NavTree.Nodes[0].Nodes.Count == 0)
            {
                // When all files are closed
                Properties.Settings.Default.Save();
                Application.Exit();
            }
        }

        /// <summary>
        /// Handles the event that occurs when the user presses a key on the keyboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.N)
                {
                    NewFileMenuClick(sender, e);
                    return;
                }
                else if (e.KeyCode == Keys.O)
                {
                    OpenFileMenuClick(sender, e);
                    return;
                }

                if (NavTree.Nodes[0].Nodes.Count > 0)
                {
                    if (e.KeyCode == Keys.S)
                    {
                        if (!e.Shift)
                        {
                            SaveFileMenuClick(sender, e);
                        }
                        else
                        {
                            SaveAllFileMenuClick(sender, e);
                        }
                    }
                    else if (e.KeyCode == Keys.E && editModeToggle.Enabled)
                    {
                        EditButtonClick(sender, e);
                    }
                    else if (e.KeyCode == Keys.R && runModeToggle.Enabled)
                    {
                        RunButtonClick(sender, e);
                    }
                    else if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
                    {
                        IncreaseFontMenuClick(sender, e);
                    }
                    else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
                    {
                        DecreaseFontMenuClick(sender, e);
                    }
                    else if (e.KeyCode == Keys.A && runModeToggle.Enabled)
                    {
                        SelectAllTextEvent(sender, e);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the event when the form is closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowFormClosingEvent(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                MainWindowController.CloseFiles();

                if (NavTree.Nodes[0].Nodes.Count == 0)
                {
                    // When all files are closed
                    Properties.Settings.Default.Save();
                    Application.Exit();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Handles the event when the Main Window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowFormLoadEvent(object sender, EventArgs e)
        {
            SetTheme(Properties.Settings.Default.Theme);
            colorBlindModeToolStripMenuItem.Checked = Properties.Settings.Default.Colorblind;
            simulationCommentsToolStripMenuItem.Checked = Properties.Settings.Default.SimulationComments;
            toggleSimulatorSemicolonsToolStripMenuItem.Checked = Properties.Settings.Default.OutputSemicolons;
        }

        #endregion

        private void previousStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            MainWindowController.RunPreviousState();
            Cursor.Current = currentCursor;
        }

        private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            TutorialWindow tutorial = new TutorialWindow();

            tutorial.Show();
            */

            new TutorialWindow().Show();

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //refocuses syntax window if it exists, refocusing or un-minimizing
            //Creates and displays if doesn't exist
            Form fc = Application.OpenForms["HelpWindow"];

            
            if (fc != null)
            {
                fc.WindowState = System.Windows.Forms.FormWindowState.Normal;
                fc.BringToFront();
            }

            else
            {
                HelpWindow about = new HelpWindow("About VisiBoole", File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Help Documentation", "About.txt")));
                about.Show();
            }




            //
            //about.Show();

            //HelpWindow hw = new HelpWindow("VisiBoole Syntax", File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Help Documentation", "Syntax.txt")));
            //hw.Show();
        }

        private void toggleSimulatorSemicolonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.OutputSemicolons = !Properties.Settings.Default.OutputSemicolons; // Flip setting
            if (editModeToggle.Enabled)
            {
                MainWindowController.RefreshOutput();
            }
        }
    }
}
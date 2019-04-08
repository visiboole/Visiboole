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

namespace VisiBoole.Views
{
    /// <summary>
    /// The MainWindow of this application
    /// </summary>
    public partial class MainWindow : Form, IMainWindow
    {
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
            NavTree.NodeMouseClick += (sender, args) => NavTree.SelectedNode = args.Node;
            Globals.TabControl.MouseDown += new MouseEventHandler(this.TabMouseDownEvent);
            Globals.TabControl.SelectedIndexChanged += (sender, e) => {
                MainWindowController.SelectFile(Globals.TabControl.SelectedIndex);
                IDisplay display = MainWindowController.GetDisplay();
                if (display != null)
                {
                    UpdateControls(display);
                }
            };
        }

        /// <summary>
        /// Saves the handle to the MainWindowController for this view
        /// </summary>
        /// <param name="MainWindowController">The handle to the MainWindowController for this view</param>
        public void AttachMainWindowController(IMainWindowController controller)
        {
            this.MainWindowController = controller;
        }

        #endregion

        #region "Utility Methods"

        /// <summary>
        /// Update buttons and icons based on the display
        /// </summary>
        /// <param name="current"></param>
        public void UpdateControls(IDisplay display)
        {
            openIcon.Enabled = (display.TypeOfDisplay == DisplayType.EDIT);
            openToolStripMenuItem.Enabled = (display.TypeOfDisplay == DisplayType.EDIT);
            newIcon.Enabled = (display.TypeOfDisplay == DisplayType.EDIT);
            newToolStripMenuItem.Enabled = (display.TypeOfDisplay == DisplayType.EDIT);
            saveIcon.Enabled = (display.TypeOfDisplay == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
            saveAllIcon.Enabled = (display.TypeOfDisplay == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
            saveToolStripMenuItem.Enabled = (display.TypeOfDisplay == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
            saveAsToolStripMenuItem.Enabled = (display.TypeOfDisplay == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
            runModeToggle.Enabled = (display.TypeOfDisplay == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
            editModeToggle.Enabled = (display.TypeOfDisplay == DisplayType.RUN);
            closeDesignToolStripMenuItem.Enabled = (display.TypeOfDisplay == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
            closeAllDesignToolStripMenuItem.Enabled = (display.TypeOfDisplay == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
            increaseFontToolStripMenuItem.Enabled = (NavTree.Nodes[0].Nodes.Count > 0);
            decreaseFontToolStripMenuItem.Enabled = (NavTree.Nodes[0].Nodes.Count > 0);
            selectAllToolStripMenuItem.Enabled = (display.TypeOfDisplay == DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);

            if (NavTree.Nodes[0].Nodes.Count > 0)
            {
                MainWindowController.SetFontSize();
            }

            if (display.TypeOfDisplay == DisplayType.EDIT && Globals.TabControl.SelectedTab != null)
            {
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
                this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
                this.menuStrip2.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
                this.NavTree.BackColor = System.Drawing.Color.DodgerBlue;
                this.NavTree.ForeColor = System.Drawing.Color.Black;
                this.BackColor = System.Drawing.Color.AliceBlue;
                this.OpenFileLinkLabel.LinkColor = System.Drawing.Color.DodgerBlue;

                this.MainWindowController.SetTheme();
                Globals.TabControl.TabPages.Add("!@#$FillTab!@#$");
                Globals.TabControl.TabPages.Remove(Globals.TabControl.TabPages[Globals.TabControl.TabPages.Count - 1]);
            }
            else if (theme == "Dark" || theme == "dark")
            {
                Properties.Settings.Default.Theme = "Dark";
                this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
                this.menuStrip2.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
                this.NavTree.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
                this.NavTree.ForeColor = System.Drawing.Color.DodgerBlue;
                this.BackColor = System.Drawing.Color.FromArgb(66, 66, 66);
                this.OpenFileLinkLabel.LinkColor = System.Drawing.Color.DodgerBlue;

                this.MainWindowController.SetTheme();
                Globals.TabControl.TabPages.Add("!@#$FillTab!@#$");
                Globals.TabControl.TabPages.Remove(Globals.TabControl.TabPages[Globals.TabControl.TabPages.Count - 1]);
            }
            else
            {
                Globals.Dialog.New("Error", "An error has occured while setting the theme.", DialogType.Ok);
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
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Save Design", new EventHandler(SaveFileMenuClick));
            contextMenu.MenuItems.Add("Close Design", new EventHandler(CloseFileMenuClick));
            contextMenu.MenuItems.Add("Close All Except This", new EventHandler(CloseAllExceptMenuClick));
            contextMenu.MenuItems.Add("Close All Designs", new EventHandler(CloseAllMenuClick));
            fileNode.ContextMenu = contextMenu;

            if (NavTree.Nodes.ContainsKey(fileName))
            {
                Globals.Dialog.New("Error", "Node " + fileName + " already exists in Desings.", DialogType.Ok);
            }

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
        /// Loads the given IDisplay
        /// </summary>
        /// <param name="previous">The display to replace</param>
        /// <param name="current">The display to be loaded</param>
        public void LoadDisplay(IDisplay previous, IDisplay current)
        {
            if (!this.MainLayoutPanel.Controls.Contains((Control)previous))
            {
                // No files have been opened
                this.MainLayoutPanel.Controls.Remove(OpenFileLinkLabel);
            }
            else
            {
                if ((previous == current) ^ (NavTree.Nodes[0].Nodes.Count > 0))
                {
                    // If either display is same or files have been opened
                    this.MainLayoutPanel.Controls.Remove((Control)previous);

                    if (NavTree.Nodes[0].Nodes.Count == 0)
                    {
                        this.MainLayoutPanel.Controls.Add(OpenFileLinkLabel, 1, 0);
                    }
                }
            }

            if (!this.MainLayoutPanel.Controls.Contains(OpenFileLinkLabel) && !this.MainLayoutPanel.Controls.Contains((Control)current))
            {
                Control currentControls = (Control)current;
                currentControls.Dock = DockStyle.Fill;
                this.MainLayoutPanel.Controls.Add(currentControls);
            }

            UpdateControls(current); // Change controls to match the new display
        }

        /// <summary>
        /// Displays file-save success message to the user
        /// </summary>
        /// <param name="fileSaved">True if the file was saved successfully</param>
        public void SaveFileSuccess(bool fileSaved)
        {
            if (fileSaved == true)
            {
                Globals.Dialog.New("Success", "File save successful.", DialogType.Ok);
            }
            else
            {
                Globals.Dialog.New("Failure", "File save failed.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Confrims whether the user wants to close the selected Design
        /// </summary>
        /// <param name="isDirty">True if the Design being closed has been modified since last save</param>
        /// <returns>Whether the selected Design will be closed</returns>
        public bool ConfirmClose(bool isDirty)
        {
            if (isDirty == true)
            {
                System.Media.SystemSounds.Asterisk.Play();
                DialogResult response = Globals.Dialog.New("Confirm", "You have made changes to the file you are trying to close - do you wish to continue?", DialogType.YesNo);

                if (response == DialogResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Confirms exit with the user if the application is dirty.
        /// </summary>
        /// <param name="isDirty">True if any open Designs have been modified since last save</param>
        /// <returns>Indicates whether the user wants to close</returns>
        public bool ConfirmExit(bool isDirty)
        {
            if (isDirty == true)
            {
                System.Media.SystemSounds.Asterisk.Play();
                DialogResult response = Globals.Dialog.New("Confirm", "You have made changes that have not been saved - do you wish to continue?", DialogType.YesNo);

                if (response == DialogResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
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
            Properties.Settings.Default.FontSize += 3;
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
            if (Properties.Settings.Default.FontSize > 6)
            {
                Properties.Settings.Default.FontSize -= 3;
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
        }

        /// <summary>
        /// Handles the event that occurs when the open file link is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileLinkClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DialogResult response = openFileDialog1.ShowDialog();

            if (response != DialogResult.OK)
            {
                return;
            }

            MainWindowController.ProcessNewFile(openFileDialog1.FileName);
            openFileDialog1.FileName = string.Empty;
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
        /// Handles the event that occurs when a open file menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileMenuClick(object sender, EventArgs e)
        {
            DialogResult response = openFileDialog1.ShowDialog();

            if (response == DialogResult.OK)
            {
                MainWindowController.ProcessNewFile(openFileDialog1.FileName);
                openFileDialog1.FileName = string.Empty;
            }
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
            DialogResult response = saveFileDialog1.ShowDialog();

            if (response == DialogResult.OK)
            {
                MainWindowController.SaveFileAs(saveFileDialog1.FileName);
                saveFileDialog1.FileName = "newFile1.vbi";
            }
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
            MainWindowController.SelectFile(e.Node.Index);
            MainWindowController.SwitchDisplay();
        }

        /// <summary>
        /// Handles the event that occurs when the run button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunButtonClick(object sender, EventArgs e)
        {
            MainWindowController.Run();
        }

        /// <summary>
        /// Handles the event that occurs when the edit button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButtonClick(object sender, EventArgs e)
        {
            MainWindowController.SwitchDisplay();
        }

        /// <summary>
        /// Handles the event that occurs when the edit menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditMenuClick(object sender, EventArgs e)
        {
            IDisplay display = MainWindowController.GetDisplay();
            if (display != null && display is DisplayEdit)
            {
                UpdateControls(display);
            }
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
            HelpWindow hw = new HelpWindow("VisiBoole Syntax", File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Help Documentation", "Syntax.txt")));
            hw.Show();
        }

        /// <summary>
        /// Checks whether the user is trying to close a tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabMouseDownEvent(object sender, MouseEventArgs e)
        {
            if (Globals.TabControl.SelectedIndex != -1)
            {
                Rectangle current = Globals.TabControl.GetTabRect(Globals.TabControl.SelectedIndex);
                Rectangle close = new Rectangle(current.Left + 7, current.Top + 4, 12, 12);
                if (close.Contains(e.Location))
                {
                    CloseFileMenuClick(sender, e);
                }
            }
        }

        /// <summary>
        /// Handles the event that occurs when a close file menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseFileMenuClick(object sender, EventArgs e)
        {
            string name = MainWindowController.CloseActiveFile();
            if (name != null)
            {
                RemoveNavTreeNode(name);
                if (NavTree.Nodes[0].Nodes.Count == 0)
                {
                    MainWindowController.LoadDisplay(DisplayType.EDIT); // Switches to default view
                }
            }
        }

        /// <summary>
        /// Handles the event that occurs when the user wants to close all files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseAllExceptMenuClick(object sender, EventArgs e)
        {
            List<string> closedFiles = MainWindowController.CloseFilesExceptFor(NavTree.SelectedNode.Name);

            if (closedFiles != null && closedFiles.Count > 0)
            {
                foreach (string file in closedFiles)
                {
                    RemoveNavTreeNode(file);
                }
            }
        }

        /// <summary>
        /// Handles the event that occurs when the user wants to close all files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseAllMenuClick(object sender, EventArgs e)
        {
            List<string> closedFiles = MainWindowController.CloseFiles();

            if (closedFiles != null && closedFiles.Count > 0)
            {
                foreach (string file in closedFiles)
                {
                    RemoveNavTreeNode(file);
                }

                if (NavTree.Nodes[0].Nodes.Count == 0)
                {
                    MainWindowController.LoadDisplay(DisplayType.EDIT); // Switches to default view
                }
            }
        }

        /// <summary>
        /// Handles the event that occurs when the exit menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitApplicationMenuClick(object sender, EventArgs e)
        {
            List<string> closedFiles = MainWindowController.ExitApplication();

            if (closedFiles != null && closedFiles.Count > 0)
            {
                foreach (string file in closedFiles)
                {
                    RemoveNavTreeNode(file);
                }
            }

            if (NavTree.Nodes[0].Nodes.Count == 0)
            {
                // When all files are closed
                Properties.Settings.Default.Save();
                Application.Exit();
            }
        }

        /// <summary>
        /// Handles the event when a menu's drop down is opening. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuDropDownOpeningEvent(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            menu.ForeColor = Color.Black;
        }

        /// <summary>
        /// Handles the event when a menu's drop down closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuDropDownClosedEvent(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            menu.ForeColor = Color.White;
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
                List<string> closedFiles = MainWindowController.ExitApplication();

                if (closedFiles != null && closedFiles.Count > 0)
                {
                    foreach (string file in closedFiles)
                    {
                        RemoveNavTreeNode(file);
                    }
                }

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
        }

        #endregion
    }
}
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
           Globals.TabControl.MouseDown += new MouseEventHandler(this.TabMouseDownEvent);
           Globals.TabControl.SelectedIndexChanged += (sender, e) => {
               UpdateControls(MainWindowController.GetDisplay());
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
           openIcon.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT);
           openToolStripMenuItem.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT);
           newIcon.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT);
           newToolStripMenuItem.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT);
           saveIcon.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
           saveAllIcon.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
           saveToolStripMenuItem.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
           saveAsToolStripMenuItem.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
           printToolStripMenuItem.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
           printPreviewToolStripMenuItem.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT) && NavTree.Nodes[0].Nodes.Count > 0;
           runModeToggle.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
           editModeToggle.Enabled = (display.TypeOfDisplay == Globals.DisplayType.RUN);
           closeDesignToolStripMenuItem.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
           increaseFontToolStripMenuItem.Enabled = (NavTree.Nodes[0].Nodes.Count > 0);
           decreaseFontToolStripMenuItem.Enabled = (NavTree.Nodes[0].Nodes.Count > 0);
           selectAllToolStripMenuItem.Enabled = (display.TypeOfDisplay == Globals.DisplayType.EDIT && NavTree.Nodes[0].Nodes.Count > 0);
           variablesToolStripMenuItem.Enabled = (display.TypeOfDisplay == Globals.DisplayType.RUN);

           if (NavTree.Nodes[0].Nodes.Count > 0)
           {
               MainWindowController.SetFontSize();
           }

           if (display.TypeOfDisplay == Globals.DisplayType.EDIT && Globals.TabControl.SelectedTab != null)
           {
               undoToolStripMenuItem.Enabled = Globals.TabControl.SelectedTab.SubDesign().editHistory.Count > 0;
               undoToolStripMenuItem1.Enabled = Globals.TabControl.SelectedTab.SubDesign().editHistory.Count > 0;
               redoToolStripMenuItem.Enabled = Globals.TabControl.SelectedTab.SubDesign().undoHistory.Count > 0;
               redoToolStripMenuItem1.Enabled = Globals.TabControl.SelectedTab.SubDesign().undoHistory.Count > 0;
               cutToolStripMenuItem.Enabled = Globals.TabControl.SelectedTab.SubDesign().SelectedText.Length > 0;
               copyToolStripMenuItem.Enabled = Globals.TabControl.SelectedTab.SubDesign().SelectedText.Length > 0;
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
           if (theme == "light")
           {
               Globals.Theme = "light";
               this.menuStrip1.BackColor = System.Drawing.Color.LightGray;
               this.menuStrip2.BackColor = System.Drawing.Color.LightGray;
               this.NavTree.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(242)))), ((int)(((byte)(243)))));
               this.NavTree.ForeColor = System.Drawing.Color.Black;
               this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(242)))), ((int)(((byte)(243)))));
               this.OpenFileLinkLabel.LinkColor = System.Drawing.Color.Blue;

               this.MainWindowController.SetTheme();
               Globals.TabControl.TabPages.Add("!@#$FillTab!@#$");
               Globals.TabControl.TabPages.Remove(Globals.TabControl.TabPages[Globals.TabControl.TabPages.Count - 1]);
           }
           else if (theme == "dark")
           {
               Globals.Theme = "dark";
               this.menuStrip1.BackColor = System.Drawing.Color.DarkGray;
               this.menuStrip2.BackColor = System.Drawing.Color.DarkGray;
               this.NavTree.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(77)))), ((int)(((byte)(81)))));
               this.NavTree.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(226)))), ((int)(((byte)(85)))));
               this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(77)))), ((int)(((byte)(81)))));
               this.OpenFileLinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(226)))), ((int)(((byte)(85)))));

               this.MainWindowController.SetTheme();
               Globals.TabControl.TabPages.Add("!@#$FillTab!@#$");
               Globals.TabControl.TabPages.Remove(Globals.TabControl.TabPages[Globals.TabControl.TabPages.Count - 1]);
           }
       }

       /// <summary>
       /// Adds a new node in the TreeView
       /// </summary>
       /// <param name="path">The filepath string that will be parsed to obtain the name of this treenode</param>
       public void AddNavTreeNode(string path)
       {
           string filename = path.Substring(path.LastIndexOf("\\") + 1);
           TreeNode node = new TreeNode(filename);

           node.Name = filename;
           ContextMenu cm = new ContextMenu();
           cm.MenuItems.Add("Save Design", new EventHandler(SaveFileEvent));
           cm.MenuItems.Add("Close Design", new EventHandler(CloseFileEvent));
           node.ContextMenu = cm;

           if (NavTree.Nodes.ContainsKey(filename))
           {
               Globals.DisplayException(new Exception(string.Concat("Node ", filename, " already exists in 'My SubDesings'.")));
           }

           NavTree.Nodes[0].Nodes.Add(node);
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
                   this.MainLayoutPanel.Controls.Remove((Control)previous);

                   if (NavTree.Nodes[0].Nodes.Count == 0)
                       this.MainLayoutPanel.Controls.Add(OpenFileLinkLabel, 1, 0);
               }   
           }

           if (!this.MainLayoutPanel.Controls.Contains(OpenFileLinkLabel) && !this.MainLayoutPanel.Controls.Contains((Control)current))
           {
               Control c = (Control)current;
               c.Dock = DockStyle.Fill;
               this.MainLayoutPanel.Controls.Add(c);
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
               MessageBox.Show("File save successful.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
           }
           else
           {
               MessageBox.Show("File save failed.", "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
           }
       }

       /// <summary>
       /// Confrims whether the user wants to close the selected SubDesign
       /// </summary>
       /// <param name="isDirty">True if the SubDesign being closed has been modified since last save</param>
       /// <returns>Whether the selected SubDesign will be closed</returns>
       public bool ConfirmClose(bool isDirty)
       {
           if (isDirty == true)
           {
               System.Media.SystemSounds.Asterisk.Play();
               DialogResult response = MessageBox.Show("You have made changes to the file you are trying to close - do you wish to continue?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

               if (response == DialogResult.Yes) return true;
               else return false;
           }
           else return true;
       }

       /// <summary>
       /// Confirms exit with the user if the application is dirty
       /// </summary>
       /// <param name="isDirty">True if any open SubDesigns have been modified since last save</param>
       /// <returns>Indicates whether the user wants to close</returns>
       public bool ConfirmExit(bool isDirty)
       {
           if (isDirty == true)
           {
               System.Media.SystemSounds.Asterisk.Play();
               DialogResult response = MessageBox.Show("You have made changes that have not been saved - do you wish to continue?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

               if (response == DialogResult.Yes)
               {
                   return true;
               }
               else return false;
           }
           else
           {
               return true;
           }
       }

       #endregion

       #region "Event Handlers"

       /// <summary>
       /// Handles the event that occurs when the light theme is selected
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void LightThemeEvent(object sender, EventArgs e)
       {
           SetTheme("light");
       }

       /// <summary>
       /// Handles the event that occurs when the light theme is selected
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void DarkThemeEvent(object sender, EventArgs e)
       {
           SetTheme("dark");
       }

       /// <summary>
       /// Increases the font size of all SubDesigns
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void IncreaseFontEvent(object sender, EventArgs e)
       {
           Globals.FontSize += 3;
           MainWindowController.SetFontSize();
           if (editModeToggle.Enabled) MainWindowController.Run();
       }

       /// <summary>
       /// Decreases the font size of all SubDesigns
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void DecreaseFontEvent(object sender, EventArgs e)
       {
           if (Globals.FontSize > 6)
           {
               Globals.FontSize -= 3;
               MainWindowController.SetFontSize();
               if (editModeToggle.Enabled) MainWindowController.Run();
           }
       }

       /// <summary>
       /// Handles the event that occurs when the link-label is clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void OpenFileLinkEvent(object sender, LinkLabelLinkClickedEventArgs e)
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
       /// Handles the event that occurs when New button (on menustrip) was clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void NewFileEvent(object sender, EventArgs e)
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
       /// Handles the event that occurs when Open button (on menustrip) was clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void OpenFileEvent(object sender, EventArgs e)
       {
           DialogResult response = openFileDialog1.ShowDialog();

           if (response == DialogResult.OK)
           {
               MainWindowController.ProcessNewFile(openFileDialog1.FileName);
               openFileDialog1.FileName = string.Empty;
           }
       }

       /// <summary>
       /// Handles the event that occurs when Save button (on menustrip) was clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void SaveFileEvent(object sender, EventArgs e)
       {
           MainWindowController.SaveFile();
       }

       /// <summary>
       /// Handles the event that occurs when SaveAs button (on menustrip) was clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void SaveAsFileEvent(object sender, EventArgs e)
       {
           DialogResult response = saveFileDialog1.ShowDialog();

           if (response == DialogResult.OK)
           {
               MainWindowController.SaveFileAs(saveFileDialog1.FileName);
               saveFileDialog1.FileName = "newFile1.vbi";
           }
       }

       /// <summary>
       /// Handles the event that ocrrus when SaveAll Icon (on menustrip) was clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void SaveAllFileEvent(object sender, EventArgs e)
       {
           MainWindowController.SaveAll();
       }

       /// <summary>
       /// Handles the event that occurs when a node on the treeview was double-clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void NavTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
       {
           MainWindowController.SelectTabPage(e.Node.Name);
           MainWindowController.SwitchDisplay();
       }

       /// <summary>
       /// Handles the event that occurs when the run mode is toggled
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void RunToggleEvent(object sender, EventArgs e)
       {
           MainWindowController.Run();
       }

       private void variablesToolStripMenuItem_Click(object sender, EventArgs e)
       {
           DebugWindow dw = new DebugWindow("Variables", MainWindowController.DebugVariables());
           dw.Show();
       }

       /// <summary>
       /// Handles the event that occurs when the edit mode is toggled
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void EditToggleEvent(object sender, EventArgs e)
       {
           MainWindowController.SwitchDisplay();
       }

       /// <summary>
       /// Handles the event when the edit menu is clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void editToolStripMenuItem_Click(object sender, EventArgs e)
       {
           if (MainWindowController.GetDisplay() is DisplayEdit) UpdateControls(MainWindowController.GetDisplay());
       }

       /// <summary>
       /// Undo text event
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void UndoTextEvent(object sender, EventArgs e)
       {
           Globals.TabControl.SelectedTab.SubDesign().UndoTextEvent(sender, e);
       }

       /// <summary>
       /// Undo text event
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void RedoTextEvent(object sender, EventArgs e)
       {
           Globals.TabControl.SelectedTab.SubDesign().RedoTextEvent(sender, e);
       }

       /// <summary>
       /// Cut text event
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void CutTextEvent(object sender, EventArgs e)
       {
           Globals.TabControl.SelectedTab.SubDesign().CutTextEvent(sender, e);
       }

       /// <summary>
       /// Cut text event
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void CopyTextEvent(object sender, EventArgs e)
       {
           Globals.TabControl.SelectedTab.SubDesign().CopyTextEvent(sender, e);
       }

       /// <summary>
       /// Cut text event
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void PasteTextEvent(object sender, EventArgs e)
       {
           Globals.TabControl.SelectedTab.SubDesign().PasteTextEvent(sender, e);
       }

       /// <summary>
       /// Select all text event
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void SelectAllTextEvent(object sender, EventArgs e)
       {
           Globals.TabControl.SelectedTab.SubDesign().SelectAllTextEvent(sender, e);
       }

       /// <summary>
       /// Handles the event that occurs when Print button (on menustrip) was clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void PrintFileEvent(object sender, EventArgs e)
       {

       }

       /// <summary>
       /// Handles the event that occurs when Print-Preview button (on menustrip) was clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void PrintPreviewFileEvent(object sender, EventArgs e)
       {

       }

       /// <summary>
       /// Syntax Documentation Help Menu Click
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void syntaxDocumentationToolStripMenuItem_Click(object sender, EventArgs e)
       {
           HelpWindow hw = new HelpWindow("VisiBoole Syntax", File.ReadAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Help Documentation", "Syntax.txt")));
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
                   CloseFileEvent(sender, e);
               }
           }
       }

       /// <summary>
       /// Handles the event that occurs when a file is closing
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void CloseFileEvent(object sender, EventArgs e)
       {
           string name = MainWindowController.CloseFile();
           if (name != null)
           {
               RemoveNavTreeNode(name);
               if (NavTree.Nodes[0].Nodes.Count == 0) MainWindowController.LoadDisplay(Globals.DisplayType.EDIT); // Switches to default view
           }
       }

       /// <summary>
       /// Handles the event that occurs when Exit button (on menustrip) was clicked
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void ExitApplicationEvent(object sender, EventArgs e)
       {
           if (MainWindowController.ExitApplication()) Application.Exit();
       }

       /// <summary>
       /// Handles the event when the form is closing
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
       {
           if (e.CloseReason == CloseReason.UserClosing)
           {
               if (MainWindowController.ExitApplication()) Application.Exit();
               else e.Cancel = true;
           }
       }

       #endregion
   }
}
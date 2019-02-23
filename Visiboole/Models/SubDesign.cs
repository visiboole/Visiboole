using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ionic;
using VisiBoole.ParsingEngine;

namespace VisiBoole.Models
{
    public delegate void DisplayLoader(Globals.DisplayType dType); // Delegate for LoadDisplay Method

    /// <summary>
    /// A User-Created SubDesign
    /// </summary>
    public class SubDesign : RichTextBoxEx
    {
        /// <summary>
        /// Database of the SubDesign
        /// </summary>
        public Database Database { get; set; }

        /// <summary>
        /// The index of the TabControl that this occupies
        /// </summary>
        public int TabPageIndex { get; set; }

        /// <summary>
        /// The file location that this SubDesign is saved in
        /// </summary>
        public FileInfo FileSource { get; set; }

        /// <summary>
        /// The short filename of the FileSource
        /// </summary>
        public string FileSourceName { get; set; }

        /// <summary>
        /// Delegate for updating the display
        /// </summary>
        private DisplayLoader UpdateDisplay;

		/// <summary>
		/// Returns True if this SubDesign Text does not match the FileSource contents
		/// </summary>
		public bool isDirty { get; set; }

        /// <summary>
        /// Previous text of the SubDesign
        /// </summary>
        private string lastText = "";

        /// <summary>
        /// Edit history of the SubDesign
        /// </summary>
        public Stack editHistory = new Stack();

        /// <summary>
        /// Undo history of the SubDesign
        /// </summary>
        public Stack undoHistory = new Stack();

        /// <summary>
        /// Constructs a new SubDesign object
        /// </summary>
        /// <param name="filename">The path of the file source for this SubDesign</param>
        public SubDesign(string filename, DisplayLoader update)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("Invalid filename");
            }

            FileSource = new FileInfo(filename);
            this.FileSourceName = FileSource.Name;
            this.UpdateDisplay = update;

            if (!File.Exists(filename))
            {
                FileSource.Create().Close();
            }

            Text = GetFileText();
            lastText = Text;
            isDirty = false;

            editHistory.Clear();
            undoHistory.Clear();

            this.TextChanged += SubDesign_TextChanged;
            this.MouseDown += SubDesign_MouseDown;
            this.KeyDown += SubDesign_KeyDown;

            this.Database = new Database();
            this.AcceptsTab = true;
	        this.ShowLineNumbers = true;
            SetTheme();
            SetFontSize();
        }

        /// <summary>
        /// Sets the theme of the SubDesign
        /// </summary>
        public void SetTheme()
        {
            NumberBorder = Color.DodgerBlue;
            NumberColor = Color.DodgerBlue;

            if (Properties.Settings.Default.Theme == "Light")
            {
                this.BackColor = SystemColors.ControlLightLight;
                this.ForeColor = Color.Black;
                NumberBackground1 = SystemColors.ControlLightLight;
                NumberBackground2 = SystemColors.ControlLightLight;
            }
            else if (Properties.Settings.Default.Theme == "Dark")
            {
                this.BackColor = Color.FromArgb(48, 48, 48);
                this.ForeColor = SystemColors.ControlLightLight;
                NumberBackground1 = Color.FromArgb(48, 48, 48);
                NumberBackground2 = Color.FromArgb(48, 48, 48);
            }
        }

        /// <summary>
        /// Sets the font size of the Sub Design to the global font size
        /// </summary>
        public void SetFontSize()
        {
            this.Font = new Font(DefaultFont.FontFamily, Properties.Settings.Default.FontSize);
        }

        /// <summary>
        /// Gets the text of the file source
        /// </summary>
        /// <returns>Text of the file source</returns>
        private string GetFileText()
        {
            string text = string.Empty;
            
            using (StreamReader reader = this.FileSource.OpenText())
            {
                string nextLine = string.Empty;

                while ((nextLine = reader.ReadLine()) != null)
                {
                    text += nextLine + "\n";
                }
            }
            return text.Replace("\t", new string(' ', 8));
        }

        /// <summary>
        /// Updates dirty and changes file name to indicate unsaved changes
        /// </summary>
        private void UpdateDirty()
        {
            isDirty = true;

            if (Globals.TabControl.TabPages[TabPageIndex].Text == FileSourceName)
            {
                Globals.TabControl.TabPages[TabPageIndex].Text = "*  " + FileSourceName;
            }
                
        }

        /// <summary>
        /// Records edits for undos
        /// </summary>
        private void RecordEdit()
        {
            bool isDel = this.Text.Length < lastText.Length; // Indicates whether the edit was a deletion
            int len = Math.Abs(this.Text.Length - lastText.Length); // The length of the string inserted or deleted
            int loc = isDel ? this.SelectionStart : (this.SelectionStart - len); // The location of the edit
            string edit = isDel ? lastText.Substring(loc, len) : this.Text.Substring(loc, len); // Gets the edit string

            // Check for special edits such as tabs, quotes and grouping characters
            if (edit.Equals("\t"))
            {
                Text = Text.Remove(loc, 1); // Remove tab
                edit = new string(' ', 8);
                Text = Text.Insert(loc, edit); // Insert spaces for tab
                SelectionStart = loc + 8; // Restore cursor location
            }
            else if (edit.Equals("\"") && !isDel)
            {
                Text = Text.Remove(loc, 1);
                edit = new string('\"', 2);
                Text = Text.Insert(loc, edit);
                SelectionStart = loc + 1;
            }
            else if (edit.Equals("[") && !isDel)
            {
                Text = Text.Remove(loc, 1);
                edit = "[]";
                Text = Text.Insert(loc, edit);
                SelectionStart = loc + 1;
            }
            else if (edit.Equals("{") && !isDel)
            {
                Text = Text.Remove(loc, 1);
                edit = "{}";
                Text = Text.Insert(loc, edit);
                SelectionStart = loc + 1;
            }
            else if (edit.Equals("(") && !isDel)
            {
                Text = Text.Remove(loc, 1);
                edit = "()";
                Text = Text.Insert(loc, edit);
                SelectionStart = loc + 1;
            }

            editHistory.Push(isDel);
            editHistory.Push(loc);
            editHistory.Push(edit);
            undoHistory.Clear();
            lastText = this.Text;
        }

        /// <summary>
        /// Does a undo or redo operation
        /// </summary>
        /// <param name="isDel"></param>
        /// <param name="loc"></param>
        /// <param name="edit"></param>
        private void DoEdit(bool isDel, int loc, string edit)
        {
            if (isDel)
            {
                lastText = this.Text.Remove(loc, edit.Length);
                this.Text = lastText;
                this.SelectionStart = loc;
            }
            else
            {
                lastText = this.Text.Insert(loc, edit);
                this.Text = lastText;
                this.SelectionStart = loc + edit.Length;
            }

            if (!isDirty) UpdateDirty();
            UpdateDisplay(Globals.DisplayType.EDIT);
        }

        /// <summary>
        /// Sets the dirty flag when the contents of this SubDesign have changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubDesign_TextChanged(object sender, EventArgs e)
		{
            if (!this.Text.Equals(lastText))
            {
                RecordEdit();
                OnTextChanged(new EventArgs());
                if (!isDirty) UpdateDirty();
                UpdateDisplay(Globals.DisplayType.EDIT);
            }
        }

        /// <summary>
        /// SubDesign mouse down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubDesign_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu cm = new ContextMenu();
                MenuItem item = new MenuItem("Undo");
                item.Click += new EventHandler(UndoTextMenuClick);
                item.Enabled = editHistory.Count > 0;
                cm.MenuItems.Add(item);
                item = new MenuItem("Redo");
                item.Click += new EventHandler(RedoTextMenuClick);
                item.Enabled = undoHistory.Count > 0;
                cm.MenuItems.Add(item);
                cm.MenuItems.Add("-");
                item = new MenuItem("Cut");
                item.Click += new EventHandler(CutTextMenuClick);
                item.Enabled = this.SelectedText.Length > 0;
                cm.MenuItems.Add(item);
                item = new MenuItem("Copy");
                item.Click += new EventHandler(CopyTextMenuClick);
                item.Enabled = this.SelectedText.Length > 0;
                cm.MenuItems.Add(item);
                item = new MenuItem("Paste");
                item.Click += new EventHandler(PasteTextMenuClick);
                item.Enabled = Clipboard.ContainsText();
                cm.MenuItems.Add(item);
                cm.MenuItems.Add("-");
                item = new MenuItem("Select All");
                item.Click += new EventHandler(SelectAllTextMenuClick);
                item.Enabled = true;
                cm.MenuItems.Add(item);
                this.ContextMenu = cm;
            }
        }

        /// <summary>
        /// SubDesign key down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubDesign_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z && e.Control)
            {
                if (editHistory.Count > 0) UndoTextMenuClick(sender, e);
            }
            else if (e.KeyCode == Keys.Y && e.Control)
            {
                if (undoHistory.Count > 0) RedoTextMenuClick(sender, e);
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                SelectAllTextMenuClick(sender, e);
            }
        }

        /// <summary>
        /// Handles the event that occurs when the undo menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UndoTextMenuClick(object sender, EventArgs e)
        {
            string edit = (string)editHistory.Pop(); // Edit string
            int loc = (int)editHistory.Pop(); // Location of edit
            bool isDel = (bool)editHistory.Pop(); // Indicates whether the edit is a deletion
            undoHistory.Push(isDel);
            undoHistory.Push(loc);
            undoHistory.Push(edit);
            DoEdit(!isDel, loc, edit);
        }

        /// <summary>
        /// Handles the event that occurs when the redo menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RedoTextMenuClick(object sender, EventArgs e)
        {
            string edit = (string)undoHistory.Pop(); // Edit string
            int loc = (int)undoHistory.Pop(); // Location of edit
            bool isDel = (bool)undoHistory.Pop(); // Indicates whether the edit is a deletion
            editHistory.Push(isDel);
            editHistory.Push(loc);
            editHistory.Push(edit);
            DoEdit(isDel, loc, edit);
        }

        /// <summary>
        /// Handles the event that occurs when the cut menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CutTextMenuClick(object sender, EventArgs e)
        {
            this.Cut();
        }

        /// <summary>
        /// Handles the event that occurs when the copy menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CopyTextMenuClick(object sender, EventArgs e)
        {
            Clipboard.SetText(this.SelectedText);
        }

        /// <summary>
        /// Handles the event that occurs when the paste menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PasteTextMenuClick(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                this.SelectedText = Clipboard.GetText(TextDataFormat.Text).ToString();
            }
        }

        /// <summary>
        /// Handles the event that occurs when the select all menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectAllTextMenuClick(object sender, EventArgs e)
        {
            this.SelectAll();
        }

        /// <summary>
        /// Saves the contents of this Text property to the FileSource contents
        /// </summary>
        public void SaveTextToFile()
        {
            File.WriteAllText(this.FileSource.FullName, this.Text);
			isDirty = false;
            Globals.TabControl.TabPages[TabPageIndex].Text = FileSourceName;
        }
    }
}
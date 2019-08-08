using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ionic;
using VisiBoole.ParsingEngine;
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.ParsingEngine.Statements;

namespace VisiBoole.Models
{
    /// <summary>
    /// Delegate for design edit events.
    /// </summary>
    /// <param name="designName">Name of the design that was edited</param>
    /// <param name="isDirty">Whether the design has unsaved changes</param>
    public delegate void DesignEditEventHandler(string designName, bool isDirty);

    /// <summary>
    /// A User-Created Design
    /// </summary>
    public class Design : RichTextBoxEx
    {
        /// <summary>
        /// Event that occurs when the design has been edited.
        /// </summary>
        public event DesignEditEventHandler DesignEdit;

        /// <summary>
        /// Database of the Design
        /// </summary>
        public Database Database { get; set; }

        /// <summary>
        /// The file location that this Design is saved in
        /// </summary>
        public FileInfo FileSource { get; private set; }

        /// <summary>
        /// Short filename that doesn't include the extension.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Returns True if this Design Text does not match the FileSource contents
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Previous text of the design.
        /// </summary>
        private string LastText;

        /// <summary>
        /// Edit history of the design.
        /// </summary>
        public Stack EditHistory { get; private set; }

        /// <summary>
        /// Undo history of the design.
        /// </summary>
        public Stack UndoHistory { get; private set; }

        /// <summary>
        /// Regex for identifying a design headers.
        /// </summary>
        private Regex HeaderRegex;

        /// <summary>
        /// Opened instantiation of the design (if any).
        /// </summary>
        public string OpenedInstantiation { get; private set; }

        /// <summary>
        /// Constructs a new Design object
        /// </summary>
        /// <param name="filename">The path of the file source for this Design</param>
        public Design(string filename, string fileText = null, DesignHeader header = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("Invalid filename");
            }

            FileSource = new FileInfo(filename);
            FileName = FileSource.Name.Split('.')[0];
            HeaderRegex = new Regex($@"^\s*{FileName}\({Parser.ModulePattern}\);$", RegexOptions.Compiled);

            if (!File.Exists(filename))
            {
                FileSource.Create().Close();
            }

            Database = new Database();
            Text = fileText == null ? GetFileText() : fileText;
            if (header != null)
            {
                Database.Header = header;
            }
            LastText = Text;
            IsDirty = false;
            EditHistory = new Stack();
            UndoHistory = new Stack();

            TextChanged += Design_TextChanged;
            MouseDown += Design_MouseDown;
            KeyDown += Design_KeyDown;

            AcceptsTab = true;
            ShowLineNumbers = true;
            AutoWordSelection = false;

            SetTheme();
            SetFontSize();
        }

        public Design Clone()
        {
            return new Design(FileSource.FullName, Text, Database.Header);
        }

        /// <summary>
        /// Sets the theme of the Design
        /// </summary>
        public void SetTheme()
        {
            NumberBorder = Color.DodgerBlue;
            NumberColor = Color.DodgerBlue;

            if (Properties.Settings.Default.Theme == "Light")
            {
                BackColor = SystemColors.ControlLightLight;
                ForeColor = Color.Black;
                NumberBackground1 = SystemColors.ControlLightLight;
                NumberBackground2 = SystemColors.ControlLightLight;
            }
            else
            {
                BackColor = Color.FromArgb(48, 48, 48);
                ForeColor = SystemColors.ControlLightLight;
                NumberBackground1 = Color.FromArgb(48, 48, 48);
                NumberBackground2 = Color.FromArgb(48, 48, 48);
            }
        }

        /// <summary>
        /// Sets the font size of the Sub Design to the global font size
        /// </summary>
        public void SetFontSize()
        {
            Font = new Font("Consolas", Properties.Settings.Default.FontSize);
        }

        /// <summary>
        /// Gets the text of the file source
        /// </summary>
        /// <returns>Text of the file source</returns>
        private string GetFileText()
        {
            string text = string.Empty;
            bool lookingForModule = true;

            using (StreamReader reader = FileSource.OpenText())
            {
                // nextLine = nextLine.TrimEnd();
                string currentStatement = string.Empty;
                string nextLine = string.Empty;
                while ((nextLine = reader.ReadLine()) != null)
                {
                    // Clean line
                    nextLine = nextLine.Replace("\t", new string(' ', 4));

                    if (lookingForModule)
                    {
                        string trimmedLine = nextLine.Trim();
                        if (trimmedLine.Length > 0)
                        {
                            if (nextLine[nextLine.Length - 1] != ';')
                            {
                                // If the current statement is an on going statement
                                if (currentStatement.Length > 0)
                                {
                                    // Add a newline seperator
                                    currentStatement += '\n';
                                }
                                // Add line to current statement
                                currentStatement += nextLine;
                            }
                            else
                            {
                                if (currentStatement.Length == 0)
                                {
                                    currentStatement = nextLine;
                                }
                                else
                                {
                                    currentStatement = string.Concat(currentStatement, "\n", nextLine);
                                }

                                if (currentStatement[0] != '#')
                                {
                                    if (HeaderRegex.IsMatch(currentStatement))
                                    {
                                        Parser parser = new Parser();
                                        Database.Header = parser.CreateHeader(currentStatement);
                                    }
                                    else
                                    {
                                        lookingForModule = false;
                                    }
                                }
                                else
                                {
                                    currentStatement = string.Empty;
                                }
                            }
                        }
                    }

                    // Append line to text
                    text += nextLine + "\n";
                }
            }
            return text.TrimEnd('\n');
        }

        /// <summary>
        /// Records edits for undos
        /// </summary>
        private void RecordEdit()
        {
            bool isDel = Text.Length < LastText.Length; // Indicates whether the edit was a deletion
            int len = Math.Abs(Text.Length - LastText.Length); // The length of the string inserted or deleted
            int loc = isDel ? SelectionStart : (SelectionStart - len); // The location of the edit
            string edit = isDel ? LastText.Substring(loc, len) : Text.Substring(loc, len); // Gets the edit string

            if (Text.Length != 0 && edit == "\t")
            {
                Text = Text.Remove(loc, 1); // Remove tab
                edit = new string(' ', 4);
                Text = Text.Insert(loc, edit); // Insert spaces for tab
                SelectionStart = loc + 4; // Restore cursor location
            }

            EditHistory.Push(isDel);
            EditHistory.Push(loc);
            EditHistory.Push(edit);
            UndoHistory.Clear();
            LastText = Text;
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
                LastText = Text.Remove(loc, edit.Length);
                Text = LastText;
                SelectionStart = loc;
            }
            else
            {
                LastText = Text.Insert(loc, edit);
                Text = LastText;
                SelectionStart = loc + edit.Length;
            }

            if (!IsDirty)
            {
                IsDirty = true;
            }
            ProcessDesignEdit();
        }

        /// <summary>
        /// Invokes listeners for the OnDesignEdit event.
        /// </summary>
        private void ProcessDesignEdit()
        {
            DesignEdit?.Invoke(FileName, IsDirty);
        }

        /// <summary>
        /// Sets the dirty flag when the contents of this Design have changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Design_TextChanged(object sender, EventArgs e)
		{
            if (!Text.Equals(LastText))
            {
                RecordEdit();
                OnTextChanged(e);
                if (!IsDirty)
                {
                    IsDirty = true;
                }
                ProcessDesignEdit();
            }
        }

        /// <summary>
        /// Design mouse down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Design_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu cm = new ContextMenu();
                MenuItem item = new MenuItem("Undo");
                item.Click += new EventHandler(UndoTextMenuClick);
                item.Enabled = EditHistory.Count > 0;
                cm.MenuItems.Add(item);
                item = new MenuItem("Redo");
                item.Click += new EventHandler(RedoTextMenuClick);
                item.Enabled = UndoHistory.Count > 0;
                cm.MenuItems.Add(item);
                cm.MenuItems.Add("-");
                item = new MenuItem("Cut");
                item.Click += new EventHandler(CutTextMenuClick);
                item.Enabled = SelectedText.Length > 0;
                cm.MenuItems.Add(item);
                item = new MenuItem("Copy");
                item.Click += new EventHandler(CopyTextMenuClick);
                item.Enabled = SelectedText.Length > 0;
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
                ContextMenu = cm;
            }
        }

        /// <summary>
        /// Design key down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Design_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.Z && EditHistory.Count > 0)
                {
                    UndoTextMenuClick(sender, e);
                }
                else if (e.KeyCode == Keys.Y && UndoHistory.Count > 0)
                {
                    RedoTextMenuClick(sender, e);
                }
                else if (e.KeyCode == Keys.A)
                {
                    SelectAllTextMenuClick(sender, e);
                }
                else if (e.KeyCode == Keys.C && SelectedText.Length > 0)
                {
                    CopyTextMenuClick(sender, e);
                }
                else if (e.KeyCode == Keys.X && SelectedText.Length > 0)
                {
                    CutTextMenuClick(sender, e);
                }
                else if (e.KeyCode == Keys.V && Clipboard.ContainsText())
                {
                    PasteTextMenuClick(sender, e);
                }
                else if (e.KeyCode == Keys.OemQuotes && Text.Length > 0)
                {
                    MakeComments(); // Comment out line(s)
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Adds ""; to selected lines to comment them out.
        /// </summary>
        private void MakeComments()
        {
            int startLine = GetLineFromCharIndex(SelectionStart);
            int endLine = GetLineFromCharIndex(SelectionStart + SelectionLength);
            if (endLine > Lines.Length)
            {
                endLine = Lines.Length - 1;
            }

            for (int i = startLine; i <= endLine; i++)
            {
                if (Lines[i].Length > 0)
                {
                    int start = GetFirstCharIndexFromLine(i);

                    Match commentMatch = Parser.CommentStmtRegex.Match(Lines[i]);
                    if (commentMatch.Success)
                    {
                        SelectionStart = start + Lines[i].IndexOf("\"");
                        SelectionLength = 1;
                        SelectedText = "";

                        SelectionStart = start + Lines[i].LastIndexOf("\"");
                        SelectionLength = 1;
                        SelectedText = "";
                    }
                    else
                    {
                        // Add " to start of current line
                        SelectionLength = 0;
                        SelectionStart = start;
                        SelectedText = "\"";

                        // Add "; to end of current line
                        int end = start + Lines[i].Length;
                        SelectionLength = 0;
                        SelectionStart = end;
                        SelectedText = "\"";
                    }
                }
            }

            // Update dirty and display
            OnTextChanged(new EventArgs());
            if (!IsDirty)
            {
                IsDirty = true;
            }
            ProcessDesignEdit();
        }

        /// <summary>
        /// Handles the event that occurs when the undo menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UndoTextMenuClick(object sender, EventArgs e)
        {
            string edit = (string)EditHistory.Pop(); // Edit string
            int loc = (int)EditHistory.Pop(); // Location of edit
            bool isDel = (bool)EditHistory.Pop(); // Indicates whether the edit is a deletion
            UndoHistory.Push(isDel);
            UndoHistory.Push(loc);
            UndoHistory.Push(edit);
            DoEdit(!isDel, loc, edit);
        }

        /// <summary>
        /// Handles the event that occurs when the redo menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RedoTextMenuClick(object sender, EventArgs e)
        {
            string edit = (string)UndoHistory.Pop(); // Edit string
            int loc = (int)UndoHistory.Pop(); // Location of edit
            bool isDel = (bool)UndoHistory.Pop(); // Indicates whether the edit is a deletion
            EditHistory.Push(isDel);
            EditHistory.Push(loc);
            EditHistory.Push(edit);
            DoEdit(isDel, loc, edit);
        }

        /// <summary>
        /// Handles the event that occurs when the cut menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CutTextMenuClick(object sender, EventArgs e)
        {
            Clipboard.SetText(SelectedText);
            SelectedText = "";
        }

        /// <summary>
        /// Handles the event that occurs when the copy menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CopyTextMenuClick(object sender, EventArgs e)
        {
            Clipboard.SetText(SelectedText);
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
                SelectedText = (string)Clipboard.GetData("Text");
            }
        }

        /// <summary>
        /// Handles the event that occurs when the select all menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectAllTextMenuClick(object sender, EventArgs e)
        {
            if (!Focused)
            {
                Focus();
            }
            SelectAll();
        }

        /// <summary>
        /// Saves the contents of this Text property to the FileSource contents.
        /// </summary>
        /// <param name="closing">Whether the design is closing.</param>
        public void SaveTextToFile(bool closing)
        {
            if (IsDirty)
            {
                File.WriteAllText(FileSource.FullName, Text);
                IsDirty = false;
                if (!closing)
                {
                    ProcessDesignEdit();
                }
            }
        }

        #region Parser Methods

        /// <summary>
        /// Exports the independent variables of the design.
        /// </summary>
        /// <returns></returns>
        public List<Variable> ExportState()
        {
            var variables = new List<Variable>();
            foreach (Variable var in Database.AllVars.Values)
            {
                if (var.GetType() == typeof(IndependentVariable))
                {
                    variables.Add(var);
                }
            }
            return variables;
        }

        /// <summary>
        /// Gets output from the statement list.
        /// </summary>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> GetOutput()
        {
            // Parse statements for output
            var output = new List<IObjectCodeElement>();
            foreach (var statement in Database.Statements)
            {
                output.AddRange(statement.Parse()); // Add output
            }

            return output;
        }

        #endregion

        #region Click Methods

        /// <summary>
        /// Clicks the provided variable(s).
        /// </summary>
        /// <param name="variableName">Variable clicked</param>
        /// <param name="value">Value of the click (formatters).</param>
        /// <returns>Output to be displayed.</returns>
        public void ClickVariables(string variableName, string value = null)
        {
            string[] variables;
            // If there is only one variable to click
            if (variableName[0] != '{')
            {
                // Create variables with the single variable to click
                variables = new string[] { variableName };
                // Flip the value of the single variable
                Database.FlipValue(variableName);
            }
            // If there are multiple variables to click (formatter)
            else
            {
                // Get all variables to set the value of
                variables = Parser.WhitespaceRegex.Split(variableName.Substring(1));
                // Set the values of all the variables to their next value
                Database.SetValues(variables, value);
            }

            // Run all instantiations
            TryRunInstantiations(false);
            // Update all clock statements' next values
            ComputeClocks();
        }

        #endregion

        #region Clock Methods

        public void TickClocks()
        {
            // For all clock statements
            foreach (var clockStatement in Database.ClockStatements)
            {
                // If clock statement is not drived by an alternate clock
                if (clockStatement.Clock == null)
                {
                    // Tick the clock statement
                    clockStatement.Tick();
                }
            }

            // Run all instantiations
            TryRunInstantiations(true);
            // Update all clock statements' next values
            ComputeClocks();
        }

        /// <summary>
        /// Updates all clock statements' next values.
        /// </summary>
        public void ComputeClocks()
        {
            foreach (var clockStatement in Database.ClockStatements)
            {
                clockStatement.Compute();
            }
        }

        #endregion

        #region Instantiation Methods

        /// <summary>
        /// Runs any present submodules. Returns whether there was an error.
        /// </summary>
        /// <returns>Whether there was an error</returns>
        public bool TryRunInstantiations(bool tick)
        {
            // For all instantiation statements
            foreach (var instantationStatement in Database.InstantiationStatements)
            {
                // If instantiation wasn't able to run
                if (!instantationStatement.TryRunInstance(tick))
                {
                    // Return false for error
                    return false;
                }
            }

            bool reset;
            do
            {
                // Start reset at false
                reset = false;
                // For all instantiation statements
                foreach (var instantationStatement in Database.InstantiationStatements)
                {
                    // If instantiation needs to be reran
                    if (instantationStatement.CheckRerun())
                    {
                        // Set reset to true
                        reset = true;
                    }
                }
            } while (reset == true);

            // Return true for no errors
            return true;
        }

        /// <summary>
        /// Closes the active instantiation.
        /// </summary>
        public void CloseActiveInstantiation()
        {
            if (OpenedInstantiation != null)
            {
                Database.Instantiations[OpenedInstantiation].CloseInstantiations();
                OpenedInstantiation = null;
            }
        }

        public void CloseInstantiation(string instantiation)
        {
            if (instantiation.Split('.')[0] == FileName)
            {
                CloseActiveInstantiation();
            }
            else if (OpenedInstantiation != null)
            {
                Database.Instantiations[OpenedInstantiation].CloseInstantiation(instantiation);
            }
        }

        public List<IObjectCodeElement> OpenInstantiation(string instantiation)
        {
            string[] instantParts = instantiation.Split('.');
            if (instantParts[0] == FileName)
            {
                var output = Database.Instantiations[instantParts[1]].OpenInstantiation();
                OpenedInstantiation = instantParts[1];
                return output;
            }
            else
            {
                return Database.Instantiations[OpenedInstantiation].OpenInstantiation(instantiation);
            }
        }

        /// <summary>
        /// Returns the design of the provided instantiation.
        /// </summary>
        /// <param name="instantiation">Instantiation to get the design of.</param>
        /// <returns>Design of the provided instantiation.</returns>
        public Design GetInstantiationDesign(string instantiation)
        {
            string[] instantParts = instantiation.Split('.');
            if (instantParts[0] == FileName)
            {
                return Database.Instantiations[instantParts[1]].GetDesign();
            }
            else
            {
                if (OpenedInstantiation == null)
                {
                    return null;
                }
                else
                {
                    return Database.Instantiations[OpenedInstantiation].GetDesign(instantiation);
                }
            }
        }

        #endregion
    }
}
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
using VisiBoole.Views;
using System.IO;
using VisiBoole.Models;
using System.Windows.Forms;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.Controllers
{
    /// <summary>
    /// Handles the logic and communication with other objects for the actions in the MainWindow
    /// </summary>
    public class MainWindowController : IMainWindowController
	{
		/// <summary>
		/// Handle to the MainWindow which is the view for this controller
		/// </summary>
		private IMainWindow MainWindow;

		/// <summary>
		/// Handle to the controller for the displays that are hosted by the MainWindow
		/// </summary>
		private IDisplayController DisplayController;

        /// <summary>
        /// Handle to the controller for the designs that are viewed by the MainWindow
        /// </summary>
        private IDesignController DesignController;

        /// <summary>
        /// Dialog box for user prompts.
        /// </summary>
        private DialogBox DialogBox;

        /// <summary>
        /// Error dialog box for design errors.
        /// </summary>
        private ErrorDialogBox ErrorDialogBox;

        /// <summary>
        /// Constructs an instance of MainWindowController with handles to its view and the controllers
        /// </summary>
        /// <param name="mainWindow">Handle to the MainWindow which is the view for this controller</param>
        /// <param name="displayController">Handle to the controller for the displays that are hosted by the MainWindow</param>
        /// <param name="designController">Handle to the controller for the designs that are viewed by the MainWindow</param>
        public MainWindowController(IMainWindow mainWindow, IDisplayController displayController, IDesignController designController)
		{
			MainWindow = mainWindow;
			MainWindow.AttachMainWindowController(this);
			DisplayController = displayController;
            DesignController = designController;
            DialogBox = new DialogBox();
            ErrorDialogBox = new ErrorDialogBox();
        }

        /// <summary>
        /// Gets the display of the main window.
        /// </summary>
        /// <returns>The display</returns>
        public IDisplay GetDisplay()
        {
            try
            {
                return DisplayController.CurrentDisplay;
            }
            catch (Exception)
            {
                DialogBox.New("Error", "An unexpected error has occured while retrieving the current display type.", DialogType.Ok);
                return null;
            }
        }

        /// <summary>
        /// Loads into the MainWindow the display of the given type
        /// </summary>
        /// <param name="dType">The type of display that should be loaded</param>
        public void LoadDisplay(DisplayType dType)
        {
            try
            {
                DisplayController.PreviousDisplay = DisplayController.CurrentDisplay;
                DisplayController.CurrentDisplay = DisplayController.GetDisplayOfType(dType);
                MainWindow.LoadDisplay(DisplayController.PreviousDisplay, DisplayController.CurrentDisplay);
            }
            catch (Exception)
            {
                DialogBox.New("Error", "An unexpected error has occured while loading the display.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Used to check if the display is the output, if it is, change it to editor.
        /// </summary>
        public void SwitchDisplay()
        {
            try
            {
                if (DisplayController.CurrentDisplay is DisplayRun)
                {
                    LoadDisplay(DisplayType.EDIT);
                }
            }
            catch (Exception)
            {
                DialogBox.New("Error", "An unexpected error has occured while switching the display type.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Focuses the main window.
        /// </summary>
        public void RetrieveFocus()
        {
            MainWindow.RetrieveFocus();
        }

        /// <summary>
        /// Displays file-save success message to the user
        /// </summary>
        /// <param name="fileSaved">True if the file was saved successfully</param>
        private void SaveFileSuccess(bool fileSaved)
        {
            if (fileSaved == true)
            {
                DialogBox.New("Success", "File save successful.", DialogType.Ok);
            }
            else
            {
                DialogBox.New("Failure", "File save failed.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Displays the provided error log to the user.
        /// </summary>
        /// <param name="errorLog">Error log to display</param>
        public void DisplayErrors(List<string> errorLog)
        {
            ErrorDialogBox.Display(errorLog);
        }

        /// <summary>
        /// Selects the file at the specified index.
        /// </summary>
        /// <param name="index">The index of the file</param>
        public void SelectFile(int index)
        {
            try
            {
                string designName = DisplayController.SelectTabPage(index);
                DesignController.SelectDesign(designName);
            }
            catch (Exception)
            {
                //DialogBox.New("Error", "An unexpected error has occured while selecting a tab page.", DialogType.Ok);
            }
        }

        /// <summary>
		/// Processes a new file that is created or opened by the user
		/// </summary>
		/// <param name="path">The path of the file that was created or opened by the user</param>
		/// <param name="overwriteExisting">True if the file at the given path should be overwritten</param>
		public void ProcessNewFile(string path, bool overwriteExisting = false)
        {
            try
            {
                if (overwriteExisting == true && File.Exists(path))
                {
                    File.Delete(path);
                }

                Design design = DesignController.CreateDesign(path);

                if (DisplayController.CreateNewTab(design) == true)
                {
                    MainWindow.AddNavTreeNode(design.FileSourceName);
                }

                LoadDisplay(DisplayController.CurrentDisplay.TypeOfDisplay);
            }
            catch (Exception)
            {
                DialogBox.New("Error", "An unexpected error has occured while processing a new file.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Saves the file that is currently active in the selected tabpage
        /// </summary>
        public void SaveFile()
        {
            try
            {
                SaveFileSuccess(DesignController.SaveActiveDesign());
            }
            catch (Exception)
            {
                DialogBox.New("Error", "An unexpected error has occured while saving a file.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Saves the file that is currently active in the selected tabpage with the filename chosen by the user
        /// </summary>
        /// <param name="path">The new file path to save the active file to</param>
        public void SaveFileAs(string path)
        {
            try
            {
                // Write the contents of the active tab in a new file at location of the selected path
                string content = DisplayController.GetActiveTabPage().Design().Text;
                File.WriteAllText(Path.ChangeExtension(path, ".vbi"), content);

                // Process the new file as usual
                ProcessNewFile(path);
                SaveFileSuccess(true);
            }
            catch (Exception)
            {
                SaveFileSuccess(false);
                DialogBox.New("Error", "An unexpected error has occured during a save as file operation.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Saves all files opened
        /// </summary>
        public void SaveFiles()
        {
            try
            {
                SaveFileSuccess(DesignController.SaveDesigns());
            }
            catch (Exception)
            {
                DialogBox.New("Error", "An unexpected error has occured while saving all files.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Closes a file with the provided name.
        /// </summary>
        /// <param name="name">Name of file to close</param>
        /// <returns>The name of the file closed</returns>
        private string CloseFile(string name)
        {
            Design design = DesignController.GetDesign(name);

            if (design != null)
            {
                bool save = true;

                if (design.IsDirty)
                {
                    DialogResult result = DialogBox.New("Confirm", $"{name} has unsaved changes. Would you like to save these changes?", DialogType.YesNoCancel);
                    if (result == DialogResult.No)
                    {
                        save = false;
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return null;
                    }
                }

                // Otherwise close file
                DisplayController.CloseTab(design.TabPageIndex);
                DesignController.CloseDesign(name, save);
                MainWindow.RemoveNavTreeNode(design.FileSourceName);
                return design.FileSourceName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Closes the selected open file
        /// </summary>
        public void CloseActiveFile()
        {
            try
            {
                CloseFile(Controllers.DesignController.ActiveDesign.FileSourceName);
            }
            catch (Exception)
            {
                DialogBox.New("Error", "An unexpected error has occured while closing a file.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Closes all files.
        /// </summary>
        public void CloseFiles()
        {
            try
            {
                string[] files = DesignController.GetDesigns();

                // Try to close all files
                for (int i = 0; i < files.Length; i++)
                {
                    if (CloseFile(files[i]) == null)
                    {
                        return; // File wasn't closed
                    }
                }
            }
            catch (Exception)
            {
                DialogBox.New("Error", "An unexpected error has occured while closing all files.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Closes all files except for the provided file name.
        /// </summary>
        /// <param name="name">Name of the file to keep open</param>
        public void CloseFilesExceptFor(string name)
        {
            try
            {
                string[] files = DesignController.GetDesigns();

                for (int i = 0; i < files.Length; i++)
                {
                    if (!files[i].Equals(name))
                    {
                        if (CloseFile(files[i]) == null)
                        {
                            return; // File wasn't closed
                        }
                    }
                }
            }
            catch (Exception)
            {
                DialogBox.New("Error", "An unexpected error has occured while closing all files.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Set theme of Designs
        /// </summary>
        public void SetTheme()
        {
            DesignController.SetThemes();
        }

        /// <summary>
        /// Update font sizes
        /// </summary>
        public void SetFontSize()
        {
            DesignController.SetDesignFontSizes();
        }

        /// <summary>
        /// Handles the event that occurs when the user runs the active design.
        /// </summary>
        public void Run()
        {
            try
            {
                List<string> errorLog;
                List<IObjectCodeElement> output = DesignController.Parse(out errorLog);
                if (output == null)
                {
                    DisplayErrors(errorLog);
                    return;
                }
                DisplayController.DisplayOutput(output);
            }
            catch (Exception exception)
            {
                DialogBox.New("Error", exception.ToString(), DialogType.Ok);
                // Leave this error message for debugging purposes
            }
        }

        /// <summary>
        /// Runs a subdesign from the provided instantiation.
        /// </summary>
        /// <param name="instantiation">Instantiation to run</param>
        /// <param name="errorLog">Log of errors (if any) from parsing</param>
        /// <returns>Output of the parsed instantiation</returns>
        public List<IObjectCodeElement> RunSubdesign(string instantiation, out List<string> errorLog)
        {
            return DesignController.ParseSubdesign(instantiation, out errorLog);
        }

        /// <summary>
        /// Handles the event that occurs when the browser needs to be refreshed.
        /// </summary>
        public void RefreshOutput()
        {
            DisplayController.RefreshOutput();
        }

        /// <summary>
        /// Handles the event that occurs when the user ticks the active design.
        /// </summary>
        /// <returns>Output list of the ticked design</returns>
        public List<IObjectCodeElement> Tick()
        {
            return DesignController.ParseTick();
        }

        /// <summary>
        /// Handles the event that occurs when the user clicks on an independent variable.
        /// </summary>
        /// <param name="variableName">The name of the variable that was clicked by the user</param>
        /// <returns></returns>
        public List<IObjectCodeElement> Variable_Click(string variableName)
        {
            return DesignController.ParseVariableClick(variableName);
        }
	}
}
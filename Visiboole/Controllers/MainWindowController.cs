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
		private IMainWindow view;

		/// <summary>
		/// Handle to the controller for the displays that are hosted by the MainWindow
		/// </summary>
		private IDisplayController displayController;

        /// <summary>
        /// Handle to the controller for the designs that are viewed by the MainWindow
        /// </summary>
        private IDesignController designController;

        /// <summary>
        /// Constructs an instance of MainWindowController with handles to its view and the display controller
        /// </summary>
        /// <param name="view">Handle to the MainWindow which is the view for this controller</param>
        /// <param name="displayController">Handle to the controller for the displays that are hosted by the MainWindow</param>
        /// <param name="designController">Handle to the controller for the designs that are viewed by the MainWindow</param>
        public MainWindowController(IMainWindow view, IDisplayController displayController, IDesignController designController)
		{
			this.view = view;
			view.AttachMainWindowController(this);
			this.displayController = displayController;
            this.designController = designController;
		}

        /// <summary>
        /// Gets the display of the main window.
        /// </summary>
        /// <returns>The display</returns>
        public IDisplay GetDisplay()
        {
            try
            {
                return displayController.CurrentDisplay;
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while retrieving the current display type.", DialogType.Ok);
                return null;
            }
        }

        /// <summary>
        /// Set theme of Designs
        /// </summary>
        public void SetTheme()
        {
            try
            {
                designController.SetThemes();
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while setting the themes of the designs.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Update font sizes
        /// </summary>
        public void SetFontSize()
        {
            try
            {
                designController.SetDesignFontSizes();
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while setting the font sizes of the designs.", DialogType.Ok);
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

                Design sd = designController.CreateDesign(path);

                if (displayController.CreateNewTab(sd) == true)
                {
                    view.AddNavTreeNode(sd.FileSourceName);
                }

                LoadDisplay(displayController.CurrentDisplay.TypeOfDisplay);
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while processing a new file.", DialogType.Ok);
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
                displayController.PreviousDisplay = displayController.CurrentDisplay;
                displayController.CurrentDisplay = displayController.GetDisplayOfType(dType);
                view.LoadDisplay(displayController.PreviousDisplay, displayController.CurrentDisplay);
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while loading the display.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Used to check if the display is the output, if it is, change it to editor.
        /// </summary>
        public void SwitchDisplay()
        {
            try
            {
                if (displayController.CurrentDisplay is DisplayRun)
                {
                    LoadDisplay(DisplayType.EDIT);
                }
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while switching the display type.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Selects the file at the specified index.
        /// </summary>
        /// <param name="index">The index of the file</param>
        public void SelectFile(int index)
        {
            try
            {
                displayController.SelectTabPage(index);
                designController.SelectDesign(index);
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while selecting a tab page.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Saves the file that is currently active in the selected tabpage
        /// </summary>
        public void SaveFile()
        {
            try
            {
                view.SaveFileSuccess(designController.SaveActiveDesign());
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while saving a file.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Focuses the main window.
        /// </summary>
        public void RetrieveFocus()
        {
            view.RetrieveFocus();
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
                string content = displayController.GetActiveTabPage().Design().Text;
                File.WriteAllText(Path.ChangeExtension(path, ".vbi"), content);

                // Process the new file as usual
                ProcessNewFile(path);
                view.SaveFileSuccess(true);
            }
            catch (Exception)
            {
                view.SaveFileSuccess(false);
                Globals.Dialog.New("Error", "An unexpected error has occured during a save as file operation.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Saves all files opened
        /// </summary>
        public void SaveFiles()
        {
            try
            {
                view.SaveFileSuccess(designController.SaveDesigns());
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while saving all files.", DialogType.Ok);
            }
        }

        /// <summary>
        /// Handles the event that occurs when the user runs the active design.
        /// </summary>
        public void Run()
        {
            try
            {
                List<IObjectCodeElement> output = designController.Parse();
                if (output == null)
                {
                    return;
                }
                displayController.DisplayOutput(output);
            }
            catch (Exception exception)
            {
                Globals.Dialog.New("Error", exception.ToString(), DialogType.Ok);
                // Leave this error message for debugging purposes
            }
        }

        /// <summary>
        /// Handles the event that occurs when the browser needs to be refreshed.
        /// </summary>
        public void RefreshOutput()
        {
            displayController.RefreshOutput();
        }

        /// <summary>
        /// Handles the event that occurs when the user ticks the active design.
        /// </summary>
        /// <returns>Output list of the ticked design</returns>
        public List<IObjectCodeElement> Tick()
        {
            return designController.ParseTick();
        }

        /// <summary>
        /// Handles the event that occurs when the user clicks on an independent variable.
        /// </summary>
        /// <param name="variableName">The name of the variable that was clicked by the user</param>
        /// <returns></returns>
        public List<IObjectCodeElement> Variable_Click(string variableName)
        {
            return designController.ParseVariableClick(variableName);
        }

        /// <summary>
        /// Closes a file with the provided name.
        /// </summary>
        /// <param name="name">Name of file to close</param>
        /// <returns>The name of the file closed</returns>
        private string CloseFile(string name)
        {
            Design design = designController.GetDesign(name);

            if (design != null)
            {
                bool save = true;

                if (design.IsDirty)
                {
                    DialogResult result = Globals.Dialog.New("Confirm", $"{name} has unsaved changes. Would you like to save these changes?", DialogType.YesNoCancel);
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
                displayController.CloseTab(design.TabPageIndex);
                designController.CloseDesign(name, save);
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
        /// <returns>The name of the file closed</returns>
        public string CloseActiveFile()
        {
            try
            {
                return CloseFile(DesignController.ActiveDesign.FileSourceName);
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while closing a file.", DialogType.Ok);
                return null;
            }
        }

        /// <summary>
        /// Closes all files.
        /// </summary>
        /// <returns>List of closed files</returns>
        public List<string> CloseFiles()
        {
            try
            {
                List<string> closedFiles = new List<string>();
                string[] files = designController.GetDesigns();

                // Try to close all files
                for (int i = 0; i < files.Length; i++)
                {
                    if (CloseFile(files[i]) == null)
                    {
                        return closedFiles; // File wasn't closed
                    }
                    else
                    {
                        closedFiles.Add(files[i]); // Add to list of closed files
                    }
                }

                return closedFiles;
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while closing all files.", DialogType.Ok);
                return null;
            }
        }

        /// <summary>
        /// Closes all files except for the provided file name.
        /// </summary>
        /// <param name="name">Name of the file to keep open</param>
        /// <returns>List of closed files</returns>
        public List<string> CloseFilesExceptFor(string name)
        {
            try
            {
                //string name = designController.GetActiveDesign().FileSourceName;
                List<string> closedFiles = new List<string>();
                string[] files = designController.GetDesigns();

                for (int i = 0; i < files.Length; i++)
                {
                    if (!files[i].Equals(name))
                    {
                        if (CloseFile(files[i]) == null)
                        {
                            return closedFiles;
                        }
                        else
                        {
                            closedFiles.Add(files[i]);
                        }
                    }
                }

                return closedFiles;
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while closing all files.", DialogType.Ok);
                return null;
            }
        }

        /// <summary>
        /// Attempts to close all files.
        /// </summary>
        /// <returns>List of closed files</returns>
        public List<string> ExitApplication()
		{
            try
            {
                return CloseFiles();
            }
            catch (Exception)
            {
                Globals.Dialog.New("Error", "An unexpected error has occured while closing the application.", DialogType.Ok);
                return null;
            }
		}
	}
}
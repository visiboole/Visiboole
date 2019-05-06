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
using CustomTabControl;

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
        /// Selects the file at the specified index.
        /// </summary>
        /// <param name="index">The index of the file</param>
        public void SelectFile(int index)
        {
            string designName = DisplayController.SelectTabPage(index);
            DesignController.SelectDesign(designName);
        }

        /// <summary>
		/// Processes a new file that is created or opened by the user
		/// </summary>
		/// <param name="path">The path of the file that was created or opened by the user</param>
		/// <param name="overwriteExisting">True if the file at the given path should be overwritten</param>
		public void ProcessNewFile(string path, bool overwriteExisting = false)
        {
            if (overwriteExisting == true && File.Exists(path))
            {
                File.Delete(path);
            }

            Design design = DesignController.CreateDesign(path);
            if (DisplayController.CreateNewTab(design) == true)
            {
                MainWindow.AddNavTreeNode(design.FileName);
            }

            LoadDisplay(DisplayController.CurrentDisplay.TypeOfDisplay);
        }

        /// <summary>
        /// Saves the file that is currently active in the selected tabpage
        /// </summary>
        public void SaveFile()
        {
            SaveFileSuccess(DesignController.SaveActiveDesign());
        }

        /// <summary>
        /// Saves the file that is currently active in the selected tabpage with the filename chosen by the user
        /// </summary>
        /// <param name="path">The new file path to save the active file to</param>
        public void SaveFileAs(string path)
        {
            // Get current design that is being saved as
            Design currentDesign = DesignController.GetActiveDesign();
            // Get current design's name
            string currentDesignName = currentDesign.FileName;
            // Get content of current design
            string content = currentDesign.Text;

            // Write content of current design to new design
            File.WriteAllText(Path.ChangeExtension(path, ".vbi"), content);
            // Create new design
            Design newDesign = DesignController.CreateDesign(path);

            // Close old design
            DesignController.CloseDesign(currentDesignName, false);
            // Update tab with new design
            DisplayController.UpdateTab(currentDesignName, newDesign);
            // Update node with new design
            MainWindow.UpdateNavTreeNode(currentDesignName, newDesign.FileName);
            // Display success
            SaveFileSuccess(true);
        }

        /// <summary>
        /// Saves all files opened
        /// </summary>
        public void SaveFiles()
        {
            SaveFileSuccess(DesignController.SaveDesigns());
        }

        /// <summary>
        /// Closes a file with the provided name.
        /// </summary>
        /// <param name="designName">Name of file to close</param>
        /// <returns>The name of the file closed</returns>
        private string CloseFile(string designName)
        {
            Design design = DesignController.GetDesign(designName);

            if (design != null)
            {
                bool save = true;

                if (design.IsDirty)
                {
                    DialogResult result = DialogBox.New("Confirm", $"{designName} has unsaved changes. Would you like to save these changes?", DialogType.YesNoCancel);
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
                DisplayController.CloseTab(designName);
                DesignController.CloseDesign(designName, save);
                MainWindow.RemoveNavTreeNode(designName);
                return designName;
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
            CloseFile(Controllers.DesignController.ActiveDesign.FileName);
        }

        /// <summary>
        /// Closes all files.
        /// </summary>
        public void CloseFiles()
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

        /// <summary>
        /// Closes all files except for the provided file name.
        /// </summary>
        /// <param name="name">Name of the file to keep open</param>
        public void CloseFilesExceptFor(string name)
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

        /// <summary>
        /// Handles the event that occurs when an edit has been made to a design.
        /// </summary>
        /// <param name="designName">Name of the design that was edited</param>
        /// <param name="isDirty">Whether the design has unsaved changes</param>
        public void OnDesignEdit(string designName, bool isDirty)
        {
            DisplayController.UpdateTabText(designName, isDirty);
            LoadDisplay(DisplayType.EDIT);
        }

        /// <summary>
        /// Swaps two nav tree nodes.
        /// </summary>
        /// <param name="srcIndex">Source index of the swap</param>
        /// <param name="dstIndex">Destination index of the swap</param>
        public void SwapDesignNodes(int srcIndex, int dstIndex)
        {
            MainWindow.SwapNavTreeNodes(srcIndex, dstIndex);
        }

        /// <summary>
        /// Set theme of Designs
        /// </summary>
        public void SetTheme()
        {
            DisplayController.SetTheme();
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
                List<IObjectCodeElement> output = DesignController.Parse();
                if (output == null)
                {
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
        /// Runs the active design with its previous state.
        /// </summary>
        public void RunPreviousState()
        {
            try
            {
                List<IObjectCodeElement> output = DesignController.ParseWithInput(DesignController.GetActiveDesignState());
                if (output == null)
                {
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
        /// <returns>Output of the parsed instantiation</returns>
        public List<IObjectCodeElement> RunSubdesign(string instantiation)
        {
            return DesignController.ParseSubdesign(instantiation);
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
        /// <param name="value">Value for formatter click</param>
        /// <returns></returns>
        public List<IObjectCodeElement> Variable_Click(string variableName, string value = null)
        {
            return DesignController.ParseVariableClick(variableName, value);
        }
	}
}
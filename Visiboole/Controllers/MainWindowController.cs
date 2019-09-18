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
        /// Error message for unfound errors while parsing.
        /// </summary>
        private string UnfoundErrorMessage =
            "An unforeseen error has occured while parsing this vbi file."
            + " It is most likely a strange syntax error or stray special character in the file."
            + " Please email a copy to jdevore@ksu.edu before trying to isolate the offending line.";

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
        public DisplayType GetCurrentDisplayType()
        {
            return DisplayController.CurrentDisplay.DisplayType;
        }

        /// <summary>
        /// Loads into the MainWindow the display of the given type
        /// </summary>
        /// <param name="dType">The type of display that should be loaded</param>
        public void LoadDisplay(DisplayType dType)
        {
            DisplayController.PreviousDisplay = DisplayController.CurrentDisplay;
            DisplayController.CurrentDisplay = DisplayController.GetDisplayOfType(dType);
            MainWindow.LoadDisplay(DisplayController.PreviousDisplay, DisplayController.CurrentDisplay);
            SetTheme();
        }

        /// <summary>
        /// Focuses the main window.
        /// </summary>
        public void RetrieveFocus()
        {
            MainWindow.RetrieveFocus();
        }

        /// <summary>
        /// Returns whether the active design has a state.
        /// </summary>
        /// <returns>Whether the active design has a state.</returns>
        public bool ActiveDesignHasState()
        {
            return DesignController.ActiveDesignHasState();
        }

        /// <summary>
        /// Selects the provided file name.
        /// </summary>
        /// <param name="name">Name of file to select</param>
        /// <param name="updateTabControl">Indicates whether to update the tab control selection</param>
        public void SelectFile(string name, bool updateDesignControl = false)
        {
            if (updateDesignControl)
            {
                DisplayController.SelectDesignTab(name);
            }
            else
            {
                DesignController.SelectFile(name);
            }
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

            var newDesign = DesignController.CreateDesign(path);
            DisplayController.CreateDesignTab(newDesign);
            MainWindow.AddNavTreeNode(newDesign.FileName);
            LoadDisplay(DisplayType.EDIT);
        }

        /// <summary>
        /// Saves the provided file or the active file if none provided.
        /// </summary>
        public void SaveFile(string name = null)
        {
            DesignController.SaveDesign(name);
        }

        /// <summary>
        /// Saves the file that is currently active in the selected tabpage with the filename chosen by the user
        /// </summary>
        /// <param name="path">The new file path to save the active file to</param>
        public void SaveFileAs(string path)
        {
            // Get current design that is being saved as
            var currentDesign = DesignController.GetActiveDesign();
            // Get current design's name
            string currentDesignName = currentDesign.FileName;

            // If design names are the same
            if (currentDesignName == Path.GetFileNameWithoutExtension(path))
            {
                // Save current design
                SaveFile(currentDesignName);
            }
            // If design names are different
            else
            {
                // Write content of current design to new design
                File.WriteAllText(Path.ChangeExtension(path, ".vbi"), currentDesign.Text);
                // Create new design
                var newDesign = DesignController.CreateDesign(path);
                // Create design tab with new design
                DisplayController.CreateDesignTab(newDesign);
                // Add node with new design
                MainWindow.AddNavTreeNode(newDesign.FileName);
            }
        }

        /// <summary>
        /// Saves all files opened.
        /// </summary>
        public void SaveFiles()
        {
            // Save designs
            DesignController.SaveDesigns();
        }

        /// <summary>
        /// Closes a specific file or the opened design and optionally updates the design tab control.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="updateDesignControl"></param>
        /// <returns></returns>
        public string CloseFile(string name = null, bool updateDesignControl = true)
        {
            // Get active design
            Design activeDesign = DesignController.GetActiveDesign();
            string designName = name ?? activeDesign.FileName;
            Design design = designName == null ? activeDesign : DesignController.GetDesign(designName);

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

            // Remove nav tree node of the design
            MainWindow.RemoveNavTreeNode(designName);
            // If design tab control needs to be updated
            if (updateDesignControl)
            {
                // Close design tab and design
                DisplayController.CloseDesignTab(designName);
            }
            // Close design
            DesignController.CloseDesign(designName, save);


            return designName;
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
                var output = DesignController.Parse();
                if (output == null)
                {
                    return;
                }
                DisplayController.DisplayOutput(output);
            }
            catch (Exception)
            {
                DialogBox.New("Error", UnfoundErrorMessage, DialogType.Ok);
            }
        }

        /// <summary>
        /// Runs the active design with its previous state.
        /// </summary>
        public void RunPreviousState()
        {
            try
            {
                var output = DesignController.Parse(DesignController.GetActiveDesignState());
                if (output == null)
                {
                    return;
                }
                DisplayController.DisplayOutput(output);
            }
            catch (Exception)
            {
                DialogBox.New("Error", UnfoundErrorMessage, DialogType.Ok);
            }
        }

        /// <summary>
        /// Opens the provided instantiation.
        /// </summary>
        /// <param name="instantiation">Instantiation to open</param>
        /// <returns>Output of the parsed instantiation</returns>
        public List<IObjectCodeElement> OpenInstantiation(string instantiation)
        {
            return DesignController.OpenInstantiation(instantiation);
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

        /// <summary>
        /// Closes all parser tabs and instantiations.
        /// </summary>
        public void SuspendRunDisplay()
        {
            DisplayController.CloseParserTabs();
        }

        /// <summary>
        /// Closes a specific instantiation from the active design.
        /// </summary>
        /// <param name="name">Name of instantiation to close.</param>
        public void CloseInstantiation(string name)
        {
            DesignController.CloseInstantiation(name);
        }
    }
}
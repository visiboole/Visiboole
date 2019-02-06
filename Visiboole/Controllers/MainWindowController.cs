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
            return displayController.CurrentDisplay;
        }

        /// <summary>
        /// Set theme of SubDesigns
        /// </summary>
        public void SetTheme()
        {
            designController.SetThemes();
        }

        /// <summary>
        /// Update font sizes
        /// </summary>
        public void SetFontSize()
        {
            designController.SetSubDesignFontSizes();
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

                SubDesign sd = designController.CreateSubDesign(path);

                if (displayController.CreateNewTab(sd) == true)
                {
                    view.AddNavTreeNode(sd.FileSourceName);
                }

                LoadDisplay(displayController.CurrentDisplay.TypeOfDisplay);
            }
            catch (Exception ex)
            {
                Globals.DisplayException(ex);
            }
        }

        /// <summary>
        /// Loads into the MainWindow the display of the given type
        /// </summary>
        /// <param name="dType">The type of display that should be loaded</param>
        public void LoadDisplay(Globals.DisplayType dType)
        {
            displayController.PreviousDisplay = displayController.CurrentDisplay;
            displayController.CurrentDisplay = displayController.GetDisplayOfType(dType);
            view.LoadDisplay(displayController.PreviousDisplay, displayController.CurrentDisplay);
        }

        /// <summary>
        /// Used to check if the display is the output, if it is, change it to editor.
        /// </summary>
        public void SwitchDisplay()
        {
            if (displayController.CurrentDisplay is DisplayRun)
            {
                LoadDisplay(Globals.DisplayType.EDIT);
            }
        }

        /// <summary>
        /// Selects the tabpage in the tabcontrol with name matching the given string
        /// </summary>
        /// <param name="fileName">The name of the tabpage to select</param>
        public void SelectTabPage(string fileName)
        {
            try
            {
                displayController.SelectTabPage(fileName);
            }
            catch (Exception ex)
            {
                Globals.DisplayException(ex);
            }
        }

        /// <summary>
        /// Saves the file that is currently active in the selected tabpage
        /// </summary>
        public void SaveFile()
        {
            try
            {
                view.SaveFileSuccess(displayController.SaveActiveTab());
            }
            catch (Exception ex)
            {
                Globals.DisplayException(ex);
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
                string content = displayController.GetActiveTabPage().SubDesign().Text;
                File.WriteAllText(Path.ChangeExtension(path, ".vbi"), content);

                // Process the new file as usual
                ProcessNewFile(path);
                view.SaveFileSuccess(true);
            }
            catch (Exception ex)
            {
                view.SaveFileSuccess(false);
                Globals.DisplayException(ex);
            }
        }

        /// <summary>
        /// Saves all files opened
        /// </summary>
        public void SaveAll()
        {
            try
            {
                view.SaveFileSuccess(displayController.SaveAllTabs());
            }
            catch (Exception ex)
            {
                Globals.DisplayException(ex);
            }
        }

        /// <summary>
        /// Run mode.
        /// </summary>
        public void Run()
        {
            displayController.Run();
        }

        /// <summary>
        /// Closes the selected open file
        /// </summary>
        /// <returns>The name of the file closed</returns>
        public string CloseFile()
        {
            SubDesign sd = displayController.GetActiveTabPage().SubDesign();

            try
            {
                if (view.ConfirmClose(sd.isDirty))
                {
                    if (displayController.CloseActiveTab())
                    {
                        designController.CloseSubDesign(sd.FileSourceName);
                        return sd.FileSourceName;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Globals.DisplayException(ex);
                return null;
            }
        }

        /// <summary>
        /// Performs a dirty check and confirms application exit with the user
        /// </summary>
        public bool ExitApplication()
		{
			return view.ConfirmExit(designController.CheckUnsavedChanges());
		}
	}
}
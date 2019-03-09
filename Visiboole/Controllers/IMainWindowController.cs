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

using System.Collections.Generic;
using VisiBoole.Views;

namespace VisiBoole.Controllers
{
	/// <summary>
	/// Exposes methods on the controller for the MainWindow
	/// </summary>
	public interface IMainWindowController
	{
        /// <summary>
        /// Gets the display of the main window.
        /// </summary>
        /// <returns>The display</returns>
        IDisplay GetDisplay();

        /// <summary>
        /// Set theme of Designs
        /// </summary>
        void SetTheme();

        /// <summary>
        /// Update font sizes
        /// </summary>
        void SetFontSize();

        /// <summary>
		/// Processes a new file that is created or opened by the user
		/// </summary>
		/// <param name="path">The path of the file that was created or opened by the user</param>
		/// <param name="overwriteExisting">True if the file at the given path should be overwritten</param>
		void ProcessNewFile(string path, bool overwriteExisting = false);

        /// <summary>
        /// Loads into the MainWindow the display of the given type
        /// </summary>
        /// <param name="dType">The type of display that should be loaded</param>
        void LoadDisplay(Globals.DisplayType dType);

        /// <summary>
        /// Switch display mode
        /// </summary>
        void SwitchDisplay();

        /// <summary>
        /// Selects the file at the specified index.
        /// </summary>
        /// <param name="index">The index of the file</param>
        void SelectFile(int index);

        /// <summary>
        /// Saves the file that is currently active in the selected tabpage
        /// </summary>
        void SaveFile();

		/// <summary>
		/// Saves the file that is currently active in the selected tabpage with the filename chosen by the user
		/// </summary>
		/// <param name="path">The new file path to save the active file to</param>
		void SaveFileAs(string filePath);

        /// <summary>
		/// Saves all files opened
		/// </summary>
		void SaveFiles();

        /// <summary>
        /// Run mode.
        /// </summary>
        void Run();

        /// <summary>
        /// Closes the selected open file
        /// </summary>
        /// <returns>The name of the file closed</returns>
        string CloseActiveFile();

        /// <summary>
        /// Closes all files.
        /// </summary>
        /// <returns>List of closed files</returns>
        List<string> CloseFiles();

        /// <summary>
        /// Closes all files except for the provided file name.
        /// </summary>
        /// <param name="name">Name of the file to keep open</param>
        /// <returns>List of closed files</returns>
        List<string> CloseFilesExceptFor(string name);

        /// <summary>
        /// Attempts to close all files.
        /// </summary>
        /// <returns>List of closed files</returns>
        List<string> ExitApplication();
    }
}
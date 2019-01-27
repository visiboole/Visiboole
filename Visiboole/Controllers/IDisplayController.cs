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

using System.Windows.Forms;
using VisiBoole.Models;
using VisiBoole.Views;

namespace VisiBoole.Controllers
{
	/// <summary>
	/// Exposes methods in the controller for the four displays
	/// </summary>
	public interface IDisplayController
	{
		/// <summary>
		/// The display that was hosted by the MainWindow before the current one
		/// </summary>
		IDisplay PreviousDisplay { get; set; }

		/// <summary>
		/// The display that is currently hosted by the MainWindow
		/// </summary>
		IDisplay CurrentDisplay { get; set; }

		/// <summary>
		/// Returns a handle to the display of the matching type
		/// </summary>
		/// <param name="dType">The type of the display to return</param>
		/// <returns>Returns a handle to the display of the matching type</returns>
		IDisplay GetDisplayOfType(Globals.DisplayType dType);

		/// <summary>
		/// Saves the handle to the controller for the MainWindow
		/// </summary>
		void AttachMainWindowController(IMainWindowController mwController);

		/// <summary>
		/// Creates a new tab on the TabControl
		/// </summary>
		/// <param name="sd">The SubDesign that is displayed in the new tab</param>
		/// <returns>Returns true if a new tab was successfully created</returns>
		bool CreateNewTab(SubDesign sd);

		/// <summary>
		/// Saves the file that is associated with the currently selected tabpage
		/// </summary>
		/// <returns>Indicates whether the file was saved</returns>
		bool SaveActiveTab();

        /// <summary>
		/// Saves the files associated to all tabpages
		/// </summary>
		/// <returns>Indicates whether the files were saved</returns>
		bool SaveAllTabs();

        /// <summary>
        /// Returns the TabPage that is currently selected
        /// </summary>
        /// <returns>Returns the TabPage that is currently selected</returns>
        TabPage GetActiveTabPage();

        /// <summary>
		/// Selects the tabpage with matching name
		/// </summary>
		/// <param name="fileName">The name of the tabpage to select</param>
		/// <returns>Returns the tabpage that matches the given string</returns>
		bool SelectTabPage(string fileName);

        /// <summary>
        /// Handles the event that occurs when the user runs the parser
        /// </summary>
        void Run();

        /// <summary>
        /// Handles the event that occurs when the user ticks
        /// </summary>
        void Tick();

        /// <summary>
		/// Closes the current tab
		/// </summary>
		/// <returns>Indicates whether the tab was closed</returns>
        bool CloseActiveTab();
	}
}
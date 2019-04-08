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
using System.Windows.Forms;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;
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
		IDisplay GetDisplayOfType(DisplayType dType);

		/// <summary>
		/// Saves the handle to the controller for the MainWindow
		/// </summary>
		void AttachMainWindowController(IMainWindowController mwController);

		/// <summary>
		/// Creates a new tab on the TabControl
		/// </summary>
		/// <param name="design">The Design that is displayed in the new tab</param>
		/// <returns>Returns true if a new tab was successfully created</returns>
		bool CreateNewTab(Design design);

        /// <summary>
        /// Returns the TabPage that is currently selected
        /// </summary>
        /// <returns>Returns the TabPage that is currently selected</returns>
        TabPage GetActiveTabPage();

        /// <summary>
		/// Selects the tab page with the given index.
		/// </summary>
		/// <param name="index">Index of tabpage to select</param>
		void SelectTabPage(int index);

        /// <summary>
        /// Displays the provided output to the browser.
        /// </summary>
        /// <param name="output">Output of the parsed design</param>
        /// <param name="position">Scroll position of the browser</param>
        void DisplayOutput(List<IObjectCodeElement> output, int position = 0);

        /// <summary>
        /// Handles the event that occurs when the browser needs to be refreshed.
        /// </summary>
        void RefreshOutput();

        /// <summary>
        /// Handles the event that occurs when the user ticks.
        /// </summary>
        /// <param name="count">Number of times to tick</param>
        void Tick(int count);

        /// <summary>
        /// Closes a specific tab in the tab control
        /// </summary>
        /// <param name="index">Index to close</param>
        /// <returns>Whether the operation was successful</returns>
        bool CloseTab(int index);

        /// <summary>
		/// Closes the current tab
		/// </summary>
		/// <returns>Indicates whether the tab was closed</returns>
        bool CloseActiveTab();
	}
}
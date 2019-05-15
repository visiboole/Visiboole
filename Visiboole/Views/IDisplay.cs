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

using CustomTabControl;
using System.Drawing;
using System.Windows.Forms;
using VisiBoole.Controllers;

namespace VisiBoole.Views
{
    /// <summary>
    /// Delegate for display tab changing events.
    /// </summary>
    /// <param name="name">Name of display tab that was changed</param>
    public delegate void DisplayTabChangeEventHandler(string name);

    /// <summary>
    /// Delegate for display tab closing events.
    /// </summary>
    /// <param name="name">Name of display tab that is closing</param>
    public delegate void DisplayTabClosingEventHandler(string name);

    /// <summary>
    /// Delegate for display tab closed events.
    /// </summary>
    /// <param name="name">Name of display tab that was closed</param>
    /// <param name="count">Number of display tabs currently opened</param>
    public delegate void DisplayTabClosedEventHandler(string name, int count);

    /// <summary>
    /// Exposes methods for the four displays hosted by the MainWindow
    /// </summary>
    public interface IDisplay
    {
        /// <summary>
        /// Event that occurs when the display tab is changed.
        /// </summary>
        event DisplayTabChangeEventHandler DisplayTabChanged;

        /// <summary>
        /// Event that occurs when the display tab is closing.
        /// </summary>
        event DisplayTabClosingEventHandler DisplayTabClosing;

        /// <summary>
        /// Event that occurs when the display tab is closed.
        /// </summary>
        event DisplayTabClosedEventHandler DisplayTabClosed;

        /// <summary>
        /// Returns the type of this display
        /// </summary>
        DisplayType DisplayType { get; }

        /// <summary>
        /// Saves the handle to the controller for this display
        /// </summary>
        /// <param name="controller">The handle to the controller to save</param>
        void AttachController(IDisplayController controller);

        /// <summary>
		/// Loads the given tab control into this display.
		/// </summary>
		/// <param name="tabControl">The tabcontrol that will be loaded by this display</param>
		void AttachTabControl(NewTabControl tabControl);

        /// <summary>
        /// Adds/updates a tab page with the provided name and the provided component.
        /// </summary>
        /// <param name="name">Name of the tab page to add or update</param>
        /// <param name="component">Component to add or update</param>
        /// <param name="swap">Whether to swap to the new component</param>
        void AddTabComponent(string name, object component, bool swap = true);

        /// <summary>
        /// Selects the tab with the provided name if present.
        /// </summary>
        /// <param name="name">Name of tab to select</param>
        void SelectTab(string name);

        /// <summary>
        /// Closes the tab with the provided name if present.
        /// </summary>
        /// <param name="name"></param>
        void CloseTab(string name);

        /// <summary>
        /// Closes all tabs.
        /// </summary>
        void CloseTabs();

        /// <summary>
        /// Sets the theme of the control.
        /// </summary>
        void SetTheme();
    }
}
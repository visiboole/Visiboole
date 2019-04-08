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

using VisiBoole.Controllers;

namespace VisiBoole.Views
{
	/// <summary>
	/// Exposes the methods for the MainWindow
	/// </summary>
	public interface IMainWindow
	{
        /// <summary>
        /// Saves the handle to the controller for this view
        /// </summary>
        /// <param name="controller">The handle to the controller for this view</param>
        void AttachMainWindowController(IMainWindowController controller);

        /// <summary>
        /// Adds a new node in the TreeView
        /// </summary>
        /// <param name="path">The filepath string that will be parsed to obtain the name of this treenode</param>
        void AddNavTreeNode(string path);

        /// <summary>
		/// Removes a node in the TreeView
		/// </summary>
		/// <param name="name">The name of the node to be removed</param>
		void RemoveNavTreeNode(string name);

		/// <summary>
		/// Loads the given IDisplay
		/// </summary>
		/// <param name="previous">The display to replace</param>
		/// <param name="current">The display to be loaded</param>
		void LoadDisplay(IDisplay previous, IDisplay current);

		/// <summary>
		/// Displays file-save success message to the user
		/// </summary>
		/// <param name="fileSaved">True if the file was saved successfully</param>
		void SaveFileSuccess(bool fileSaved);

        /// <summary>
        /// Focuses this window.
        /// </summary>
        void RetrieveFocus();

        /// <summary>
        /// Confrims whether the user wants to close the selected Design
        /// </summary>
        /// <param name="isDirty">True if the Design being closed has been modified since last save</param>
        /// <returns>Whether the selected Design will be closed</returns>
		bool ConfirmClose(bool isDirty);

        /// <summary>
        /// Confirms exit with the user if the application is dirty
        /// </summary>
        /// <param name="isDirty">True if any open Designs have been modified since last save</param>
        /// <returns>Indicates whether the user wants to close</returns>
        bool ConfirmExit(bool isDirty);
    }
}
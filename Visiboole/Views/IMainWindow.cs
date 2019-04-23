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
using VisiBoole.Models;

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
        /// Updates the provided nav tree node with a new name.
        /// </summary>
        /// <param name="oldName">Name of node</param>
        /// <param name="newName">New name of node</param>
        void UpdateNavTreeNode(string oldName, string newName);

        /// <summary>
        /// Swaps two indexes of the nav tree.
        /// </summary>
        /// <param name="srcIndex">Source index</param>
        /// <param name="dstIndex">Destination index</param>
        void SwapNavTreeNodes(int srcIndex, int dstIndex);

        /// <summary>
        /// Loads the given IDisplay
        /// </summary>
        /// <param name="previous">The display to replace</param>
        /// <param name="current">The display to be loaded</param>
        void LoadDisplay(IDisplay previous, IDisplay current);

        /// <summary>
        /// Focuses this window.
        /// </summary>
        void RetrieveFocus();
    }
}
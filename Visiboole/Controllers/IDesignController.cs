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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiBoole.Models;

namespace VisiBoole.Controllers
{
    public interface IDesignController
    {
        /// <summary>
		/// Saves the handle to the controller for the MainWindow
		/// </summary>
		void AttachMainWindowController(IMainWindowController mwController);

        /// <summary>
        /// Creates a SubDesign with the given name.
        /// </summary>
        /// <param name="path">Name of SubDesign</param>
        /// <returns>The SubDesign created</returns>
        SubDesign CreateSubDesign(string name);

        /// <summary>
        /// Closes a given SubDesign.
        /// </summary>
        /// <param name="path">Name of SubDesign</param>
        bool CloseSubDesign(string name);

        /// <summary>
        /// Update the font sizes of all SubDesigns.
        /// </summary>
        void SetSubDesignFontSizes();

        /// <summary>
        /// Set the themes of all SubDesigns
        /// </summary>
        void SetThemes();

        /// <summary>
        /// Checks all SubDesigns for unsaved changes
        /// </summary>
        /// <returns>Indicates whether there are unsaved changes</returns>
        bool CheckUnsavedChanges();
    }
}
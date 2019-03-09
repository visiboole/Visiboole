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
        /// Selects a Design with the provided index
        /// </summary>
        /// <param name="index">Index of the design to select</param>
        void SelectDesign(int index);

        /// <summary>
        /// Returns the names of all Designs.
        /// </summary>
        /// <returns>Names of all Designs.</returns>
        string[] GetDesigns();

        /// <summary>
        /// Returns the active Design.
        /// </summary>
        /// <returns>Active Design</returns>
        Design GetActiveDesign();

        /// <summary>
        /// Gets a design by name.
        /// </summary>
        /// <param name="name">Name of design</param>
        /// <returns>Design with the provided name</returns>
        Design GetDesign(string name);

        /// <summary>
        /// Creates a Design with the given name.
        /// </summary>
        /// <param name="path">Name of Design</param>
        /// <returns>The Design created</returns>
        Design CreateDesign(string name);

        /// <summary>
        /// Saves the active Design.
        /// </summary>
        /// <returns>Whether the save was successful</returns>
        bool SaveActiveDesign();

        /// <summary>
        /// Closes a given Design.
        /// </summary>
        /// <param name="name">Name of Design</param>
        /// <param name="save">Indicates whether the user wants the design saved</param>
        /// <returns>Indicates whether the Design was closed</returns>
        bool CloseDesign(string name, bool save);

        /// <summary>
        /// Saves all Designs
        /// </summary>
        /// <returns>Whether the save was successful</returns>
        bool SaveDesigns();

        /// <summary>
        /// Update the font sizes of all Designs.
        /// </summary>
        void SetDesignFontSizes();

        /// <summary>
        /// Set the themes of all Designs
        /// </summary>
        void SetThemes();
    }
}
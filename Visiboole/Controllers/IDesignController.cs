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
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.Controllers
{
    public interface IDesignController
    {
        /// <summary>
        /// Saves the handle to the controller for the MainWindow
        /// </summary>
        void AttachMainWindowController(IMainWindowController mainWindowController);

        /// <summary>
        /// Gets an array of design names that are currently opened.
        /// </summary>
        /// <returns>Array of design names that are currently opened.</returns>
        string[] GetDesigns();

        /// <summary>
        /// Gets the active design.
        /// </summary>
        /// <returns>Active design.</returns>
        Design GetActiveDesign();

        /// <summary>
        /// Gets the design with the specified name.
        /// </summary>
        /// <param name="name">Name of design to return.</param>
        /// <returns>Design with the specified name.</returns>
        Design GetDesign(string name);

        /// <summary>
        /// Selects the design and parser with the specified name.
        /// </summary>
        /// <param name="name">Name of design to select.</param>
        void SelectFile(string name);

        /// <summary>
        /// Creates a design from the specified path.
        /// </summary>
        /// <param name="path">Path of the design</param>
        /// <returns>Created design</returns>
        Design CreateDesign(string path);

        /// <summary>
        /// Saves the design with the specified name.
        /// </summary>
        /// <param name="name">Name of design to be saved.</param>
        void SaveDesign(string name = null);

        /// <summary>
        /// Saves all designs.
        /// </summary>
        void SaveDesigns();

        /// <summary>
        /// Saves and closes the design with the specified name.
        /// </summary>
        /// <param name="name">Name of the design to close.</param>
        /// <param name="save">Whether the closing design should be saved.</param>
        void CloseDesign(string name, bool save);

        /// <summary>
        /// Closes a specific instantiation from the active design.
        /// </summary>
        /// <param name="name">Name of instantiation to close.</param>
        void CloseInstantiation(string name);

        /// <summary>
        /// Closes all instantiations from the active design.
        /// </summary>
        void CloseInstantiations();

        /// <summary>
        /// Updates the font sizes of all designs.
        /// </summary>
        void SetDesignFontSizes();

        /// <summary>
        /// Changes the themes of all designs
        /// </summary>
        void SetThemes();

        /// <summary>
        /// Returns whether the active design has a state.
        /// </summary>
        /// <returns>Whether the active design has a state.</returns>
        bool ActiveDesignHasState();

        /// <summary>
        /// Gets the current state of the active design.
        /// </summary>
        /// <returns>Current state of active design.</returns>
        List<Variable> GetActiveDesignState();

        /// <summary>
        /// Parses the active design with the provided input variables.
        /// </summary>
        /// <returns>Output of the parsed design.</returns>
        /// <param name="inputVariables">List of inputs variables</param>
        List<IObjectCodeElement> Parse(List<Variable> inputVariables = null);

        /// <summary>
        /// Parses a tick for the active design.
        /// </summary>
        /// <returns>Output of the parsed tick.</returns>
        List<IObjectCodeElement> ParseTick();

        /// <summary>
        /// Parses a variable click for the active design.
        /// </summary>
        /// <param name="variableName">Name of the variable that was clicked by the user.</param>
        /// <param name="nextValue">Next value if formatter click./param>
        /// <returns>Output of the parsed variable click.</returns>
        List<IObjectCodeElement> ParseVariableClick(string variableName, string nextValue = null);

        /// <summary>
        /// Opens the provided instantiation from the active design.
        /// </summary>
        /// <param name="instantiation">Instantiation to open</param>
        /// <returns>Output of the parsed design</returns>
        List<IObjectCodeElement> OpenInstantiation(string instantiation);
    }
}
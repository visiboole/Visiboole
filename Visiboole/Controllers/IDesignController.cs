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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.Controllers
{
    public interface IDesignController
    {
        /// <summary>
        /// Saves the handle to the controller for the MainWindow
        /// </summary>
        void AttachMainWindowController(IMainWindowController mwController);

        /// <summary>
        /// Returns the names of all Designs.
        /// </summary>
        /// <returns>Names of all Designs.</returns>
        string[] GetDesigns();

        /// <summary>
        /// Returns the active design.
        /// </summary>
        /// <returns>Active design</returns>
        Design GetActiveDesign();

        /// <summary>
        /// Gets a design by name.
        /// </summary>
        /// <param name="name">Name of design</param>
        /// <returns>Design with the provided name</returns>
        Design GetDesign(string name);

        /// <summary>
        /// Selects a design with the provided name
        /// </summary>
        /// <param name="design">Design to select</param>
        void SelectDesign(string design);

        /// <summary>
        /// Selects a parser for the provided design name
        /// </summary>
        /// <param name="design"></param>
        void SelectParser(string design);

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
        /// Saves all Designs
        /// </summary>
        /// <returns>Whether the save was successful</returns>
        bool SaveDesigns();

        /// <summary>
        /// Closes a given Design.
        /// </summary>
        /// <param name="name">Name of Design</param>
        /// <param name="save">Indicates whether the user wants the design saved</param>
        /// <returns>Indicates whether the Design was closed</returns>
        bool CloseDesign(string name, bool save);

        /// <summary>
        /// Update the font sizes of all Designs.
        /// </summary>
        void SetDesignFontSizes();

        /// <summary>
        /// Set the themes of all Designs
        /// </summary>
        void SetThemes();

        /// <summary>
        /// Parses the active design.
        /// </summary>
        /// <returns>Output of the parsed design</returns>
        List<IObjectCodeElement> Parse();

        /// <summary>
        /// Parses a tick for the active design.
        /// </summary>
        /// <returns>Output of the tick for the parsed design</returns>
        List<IObjectCodeElement> ParseTick();

        /// <summary>
        /// Parses a variable click for the active design.
        /// </summary>
        /// <param name="variableName">The name of the variable that was clicked by the user</param>
        /// <param name="value">Value for formatter click</param>
        /// <returns>Output of the tick for the parsed design</returns>
        List<IObjectCodeElement> ParseVariableClick(string variableName, string value = null);

        /// <summary>
        /// Parsers the current design text with input variables.
        /// </summary>
        /// <param name="inputVariables">Input variables</param>
        /// <returns>Parsed output</returns>
        List<IObjectCodeElement> ParseWithInput(List<Variable> inputVariables);

        /// <summary>
        /// Parsers a sub design with the provided instantiation.
        /// </summary>
        /// <param name="instantiation">Instnatiation</param>
        /// <returns>Output of the parsed design</returns>
        List<IObjectCodeElement> ParseSubdesign(string instantiation);

        /// <summary>
        /// Gets the active designs current state.
        /// </summary>
        /// <returns>Active designs current state</returns>
        List<Variable> GetActiveDesignState();

        void ClearParsers();

        /// <summary>
        /// Removes a parser from the open parsers.
        /// </summary>
        /// <param name="name">Design name of the parser to close</param>
        void CloseParser(string name);
    }
}
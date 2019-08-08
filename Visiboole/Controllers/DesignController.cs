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
using VisiBoole.Models;
using VisiBoole.ParsingEngine;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.Controllers
{
    public class DesignController : IDesignController
    {
        /// <summary>
        /// Handle to the controller for the MainWindow
        /// </summary>
        private IMainWindowController MainWindowController;

        /// <summary>
        /// Design parser.
        /// </summary>
        private Parser Parser;

        /// <summary>
		/// All opened designs currently loaded by this application.
		/// </summary>
        private Dictionary<string, Design> Designs;

        /// <summary>
        /// The active design.
        /// </summary>
        public static Design ActiveDesign { get; set; }

        /// <summary>
        /// Constructs design controller
        /// </summary>
        public DesignController()
        {
            Designs = new Dictionary<string, Design>();
            ActiveDesign = null;
        }

        /// <summary>
        /// Saves the handle to the controller for the MainWindow
        /// </summary>
        public void AttachMainWindowController(IMainWindowController mainWindowController)
        {
            MainWindowController = mainWindowController;
        }

        /// <summary>
        /// Gets an array of design names that are currently opened.
        /// </summary>
        /// <returns>Array of design names that are currently opened.</returns>
        public string[] GetDesigns()
        {
            // Return array containing the names of all designs currently opened
            return Designs.Keys.ToArray();
        }

        /// <summary>
        /// Gets the active design.
        /// </summary>
        /// <returns>Active design.</returns>
        public Design GetActiveDesign()
        {
            // Return the active design
            return ActiveDesign;
        }

        /// <summary>
        /// Gets the design with the specified name.
        /// </summary>
        /// <param name="name">Name of design to return.</param>
        /// <returns>Design with the specified name.</returns>
        public Design GetDesign(string name)
        {
            if (name.Contains("."))
            {
                return ActiveDesign.GetInstantiationDesign(name);
            }
            else
            {
                // If design dictionary has a design for the specified name, return that design
                // Otherwise, return null
                return Designs.ContainsKey(name) ? Designs[name] : null;
            }
        }

        /// <summary>
        /// Selects the design and parser with the specified name.
        /// </summary>
        /// <param name="name">Name of design to select.</param>
        public void SelectFile(string name)
        {
            // If specified name is null
            if (name == null)
            {
                // Set active design to null
                ActiveDesign = null;
            }
            // If specified name is not null
            else
            {
                // Set active design to the specified design
                ActiveDesign = GetDesign(name);
            }
        }

        /// <summary>
        /// Creates a design from the specified path.
        /// </summary>
        /// <param name="path">Path of the design</param>
        /// <returns>Created design</returns>
        public Design CreateDesign(string path)
        {
            // Create new design with the specified path
            var newDesign = new Design(path);
            if (Designs.ContainsKey(newDesign.FileName))
            {
                Designs[newDesign.FileName] = newDesign;
            }
            else
            {
                // Add new design to the designs dictionary
                Designs.Add(newDesign.FileName, newDesign);
            }
            // Set new design as the active design
            ActiveDesign = newDesign;
            // Return new design
            return newDesign;
        }

        /// <summary>
        /// Saves the specified design if it is dirty.
        /// </summary>
        /// <param name="design">Design to save.</param>
        /// <param name="closing">Whether the saved design is closing.</param>
        private void SaveDesign(Design design, bool closing)
        {
            // If design is dirty
            if (design.IsDirty)
            {
                // Save design
                design.SaveTextToFile(closing);
            }
        }

        /// <summary>
        /// Saves the design with the specified name.
        /// </summary>
        /// <param name="name">Name of design to be saved.</param>
        /// <param name="closing">Whether the saved design is closing.</param>
        public void SaveDesign(string name = null)
        {
            // If no name is specified, save the active design
            // Otherwise, save the design with specified name
            SaveDesign(name == null ? ActiveDesign : GetDesign(name), false);
        }

        /// <summary>
        /// Saves all designs.
        /// </summary>
        public void SaveDesigns()
        {
            // For each opened design
            foreach (Design design in Designs.Values)
            {
                // Save design
                SaveDesign(design, false);
            }
        }

        /// <summary>
        /// Saves and closes the design with the specified name.
        /// </summary>
        /// <param name="name">Name of the design to close.</param>
        /// <param name="save">Whether the closing design should be saved.</param>
        public void CloseDesign(string name, bool save)
        {
            // If the closing design should be saved
            if (save)
            {
                // Save design
                SaveDesign(GetDesign(name), true);
            }
            // Remove design from designs dictionary
            Designs.Remove(name);
            // If there are no designs opened
            if (Designs.Count == 0)
            {
                // Set active design to none
                ActiveDesign = null;
                // Reload the display
                MainWindowController.LoadDisplay(DisplayType.EDIT);
            }
        }

        /// <summary>
        /// Closes a specific instantiation from the active design.
        /// </summary>
        /// <param name="name">Name of instantiation to close.</param>
        public void CloseInstantiation(string name)
        {
            ActiveDesign.CloseInstantiation(name);
        }

        /// <summary>
        /// Closes all instantiations from the active design.
        /// </summary>
        public void CloseInstantiations()
        {
            ActiveDesign.CloseActiveInstantiation();
        }

        /// <summary>
        /// Updates the font sizes of all designs.
        /// </summary>
        public void SetDesignFontSizes()
        {
            // For each design in opened designs
            foreach (Design design in Designs.Values)
            {
                // Set font size
                design.SetFontSize();
            }
        }

        /// <summary>
        /// Changes the themes of all designs
        /// </summary>
        public void SetThemes()
        {
            // For each design in opened designs
            foreach (Design design in Designs.Values)
            {
                // Set theme
                design.SetTheme();
            }
        }

        /// <summary>
        /// Returns whether the active design has a state.
        /// </summary>
        /// <returns>Whether the active design has a state.</returns>
        public bool ActiveDesignHasState()
        {
            return ActiveDesign.Database != null && ActiveDesign.Database.IndVars.Count != 0;
        }

        /// <summary>
        /// Gets the current state of the active design.
        /// </summary>
        /// <returns>Current state of active design.</returns>
        public List<Variable> GetActiveDesignState()
        {
            // Return current state of the active design
            return ActiveDesign.ExportState();
        }

        /// <summary>
        /// Parses the active design with the provided input variables.
        /// </summary>
        /// <returns>Output of the parsed design.</returns>
        /// <param name="inputVariables">List of inputs variables</param>
        public List<IObjectCodeElement> Parse(List<Variable> inputVariables = null)
        {
            // If the parser hasn't been used
            if (Parser == null)
            {
                // Create the design parser
                Parser = new Parser();
            }

            // Return the output of the parsed design
            return Parser.Parse(ActiveDesign, inputVariables);
        }

        /// <summary>
        /// Parses a tick for the active design.
        /// </summary>
        /// <returns>Output of the parsed tick.</returns>
        public List<IObjectCodeElement> ParseTick()
        {
            ActiveDesign.TickClocks();
            return ActiveDesign.GetOutput();
        }

        /// <summary>
        /// Parses a variable click for the active design.
        /// </summary>
        /// <param name="variableName">Name of the variable that was clicked by the user.</param>
        /// <param name="nextValue">Next value if formatter click./param>
        /// <returns>Output of the parsed variable click.</returns>
        public List<IObjectCodeElement> ParseVariableClick(string variableName, string nextValue = null)
        {
            ActiveDesign.ClickVariables(variableName, nextValue);
            return ActiveDesign.GetOutput();
        }

        /// <summary>
        /// Opens the provided instantiation from the active design.
        /// </summary>
        /// <param name="instantiation">Instantiation to open</param>
        /// <returns>Output of the parsed design</returns>
        public List<IObjectCodeElement> OpenInstantiation(string instantiation)
        {
            return ActiveDesign.OpenInstantiation(instantiation);
        }
    }
}
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
		/// All opened designs currently loaded by this application.
		/// </summary>
        private Dictionary<string, Design> Designs;

        /// <summary>
        /// All opened parsers currently loaded by this application.
        /// </summary>
        private Dictionary<string, Parser> Parsers;

        /// <summary>
        /// The active design.
        /// </summary>
        public static Design ActiveDesign { get; set; }

        /// <summary>
        /// Active parsing instance.
        /// </summary>
        private Parser ActiveParser;

        /// <summary>
        /// Constructs design controller
        /// </summary>
        public DesignController()
        {
            Designs = new Dictionary<string, Design>();
            ActiveDesign = null;
            Parsers = new Dictionary<string, Parser>();
            ActiveParser = null;
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
            // If design dictionary has a design for the specified name, return that design
            // Otherwise, return null
            return Designs.ContainsKey(name) ? Designs[name] : null;
        }

        private Parser GetParser(string name)
        {
            // If parser dictionary has a parser for the specified name, return that parser
            // Otherwise, return null
            return Parsers.ContainsKey(name) ? Parsers[name] : null;
        }

        /// <summary>
        /// Returns whether the specified design has a parser already opened.
        /// </summary>
        /// <param name="name">Name of the design.</param>
        /// <returns>Whether the specified design has a parser already opened.</returns>
        public bool DesignHasParser(string name)
        {
            return Parsers.ContainsKey(name);
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
                // Set active parser to null
                ActiveParser = null;
            }
            // If specified name is not null
            else
            {
                // Set active design to the specified design
                ActiveDesign = GetDesign(name);
                // Set active parser to the specified parser
                ActiveParser = GetParser(name);
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
                if (Parsers.ContainsKey(newDesign.FileName))
                {
                    Parsers.Remove(newDesign.FileName);
                }
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
            // If design has a parser to close
            if (Parsers.ContainsKey(name))
            {
                // Remove parser from parsers
                Parsers.Remove(name);
                // If there are no parsers opened
                if (Parsers.Count == 0)
                {
                    // Set active parser to none
                    ActiveParser = null;
                }
            }

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
        /// Removes the parser of the specified instantiation from the dictionary of opened parsers.
        /// </summary>
        /// <param name="name">Name of parser to close.</param>
        public void CloseInstantiationParser(string name)
        {
            // If designs contains the parser to close
            if (Designs.ContainsKey(name))
            {
                Designs.Remove(name);
            }

            // If parsers contains the parser to close
            if (Parsers.ContainsKey(name))
            {
                // Remove parser from parsers
                Parsers.Remove(name);
                // If there are no parsers opened
                if (Parsers.Count == 0)
                {
                    // Set active parser to none
                    ActiveParser = null;
                    // Reload the display
                    MainWindowController.LoadDisplay(DisplayType.EDIT);
                }
            }
        }

        /// <summary>
        /// Clears all instantiation parsers from the parser dictionary.
        /// </summary>
        public void CloseInstantiationParsers()
        {
            // For each parser in the parser dictionary
            foreach (string parserName in Parsers.Keys.ToList())
            {
                // If parser is an instantiation
                if (parserName.Contains("."))
                {
                    // Remove parser from parser dictionary
                    Parsers.Remove(parserName);
                }
            }
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
        /// Gets the current state of the active design.
        /// </summary>
        /// <returns>Current state of active design.</returns>
        public List<Variable> GetActiveDesignState()
        {
            // Return current state of the active design
            return ActiveParser.ExportState();
        }

        /// <summary>
        /// Parses the active design.
        /// </summary>
        /// <returns>Output of the parsed design.</returns>
        public List<IObjectCodeElement> Parse()
        {
            // Create a parser for the active design
            var parser = new Parser(ActiveDesign);
            // Get output of the parsed active design
            var output = parser.Parse();
            // If output is not null
            if (output != null)
            {
                // If the parsers dictionary has a previous parser for the design
                if (Parsers.ContainsKey(ActiveDesign.FileName))
                {
                    // Override the previous parser with the new parser
                    Parsers[ActiveDesign.FileName] = parser;
                }
                // If the parsers dictionary doesn't have a previous parser for the design
                else
                {
                    // Save parser for the design
                    Parsers.Add(ActiveDesign.FileName, parser);
                }
                // Set parser to be the active parser
                ActiveParser = parser;
            }
            // Return output from the parsed active design
            return output;
        }

        /// <summary>
        /// Parses a tick for the active design.
        /// </summary>
        /// <returns>Output of the parsed tick.</returns>
        public List<IObjectCodeElement> ParseTick()
        {
            // Return output from the parsed design tick
            return ActiveParser.ParseTick();
        }

        /// <summary>
        /// Parses a variable click for the active design.
        /// </summary>
        /// <param name="variableName">Name of the variable that was clicked by the user.</param>
        /// <param name="nextValue">Next value if formatter click./param>
        /// <returns>Output of the parsed variable click.</returns>
        public List<IObjectCodeElement> ParseVariableClick(string variableName, string nextValue = null)
        {
            // Return ouput from the parsed design variable click
            return ActiveParser.ParseClick(variableName, nextValue);
        }

        /// <summary>
        /// Parsers the active design with the specified input variables.
        /// </summary>
        /// <param name="inputVariables">Input variables.</param>
        /// <returns>Output of the parsed design.</returns>
        public List<IObjectCodeElement> ParseWithInput(List<Variable> inputVariables)
        {
            // Create a parser for the active design
            var parser = new Parser(ActiveDesign);
            // Get output of the parsed active design
            var output = parser.ParseWithInput(inputVariables);
            // If output is not null
            if (output != null)
            {
                // If the parsers dictionary has a previous parser for the design
                if (Parsers.ContainsKey(ActiveDesign.FileName))
                {
                    // Override the previous parser with the new parser
                    Parsers[ActiveDesign.FileName] = parser;
                }
                // If the parsers dictionary doesn't have a previous parser for the design
                else
                {
                    // Save parser for the design
                    Parsers.Add(ActiveDesign.FileName, parser);
                }
                // Set parser to be the active parser
                ActiveParser = parser;
            }
            // Return output from the parsed design
            return output;
        }

        /// <summary>
        /// Parsers a sub design with the provided instantiation.
        /// </summary>
        /// <param name="instantiation">Instantiation</param>
        /// <returns>Output of the parsed design</returns>
        public List<IObjectCodeElement> ParseSubdesign(string instantiation)
        {
            // Save the active design
            var currentDesign = ActiveDesign;
            // Get the design name from the instantiations dictionary inside the active parser
            string designName = ActiveParser.Instantiations[instantiation].Split('.').First().TrimStart();
            // Get full instantiation name
            string fullInstantName = string.Concat(designName, '.', instantiation);
            
            // Get the sub design from the design name
            var subDesign = ActiveParser.Subdesigns[designName];
            // If the sub design isn't in the design dictionary
            if (!Designs.ContainsKey(fullInstantName))
            {
                // Add sub design to the design dictionary
                Designs.Add(fullInstantName, subDesign);
            }
            // Get the input variables from the active parser
            var inputVariables = ActiveParser.GetModuleInputs(instantiation, subDesign.HeaderLine);

            // Set the sub design to be the active design
            ActiveDesign = subDesign;
            // Create a parser for the sub design
            var subParser = new Parser(subDesign);
            // Get the output of the parsed sub design
            var output = subParser.ParseWithInput(inputVariables);
            // If output is not null
            if (output != null)
            {
                // If the parsers dictionary has a previous parser for the current instantiation
                if (Parsers.ContainsKey(fullInstantName))
                {
                    // Override the previous parser with the new parser
                    Parsers[fullInstantName] = subParser;
                }
                // If the parsers dictionary doesn't have a previous parser for the current instantiation
                else
                {
                    // Save parser for the current instantiation
                    Parsers.Add(fullInstantName, subParser);
                }
                // Set active parser to the parser of the current instantiation
                ActiveParser = subParser;
            }

            // Return output of the instantiation
            return output;
        }
    }
}
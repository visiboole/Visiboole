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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisiBoole.Models;
using VisiBoole.ParsingEngine;
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.Views;

namespace VisiBoole.Controllers
{
    public class DesignController : IDesignController
    {
        /// <summary>
		/// Handle to the controller for the MainWindow
		/// </summary>
		private IMainWindowController mwController;

        /// <summary>
		/// All opened Designs currently loaded by this application
		/// </summary>
        private Dictionary<string, Design> Designs;

        /// <summary>
        /// All parsers of running designs in this application
        /// </summary>
        private Dictionary<string, Parser> Parsers;

        /// <summary>
        /// The active Design.
        /// </summary>
        public static Design ActiveDesign { get; set; }

        /// <summary>
        /// Active parser instance
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
		/// <param name="mwController"></param>
		public void AttachMainWindowController(IMainWindowController mwController)
        {
            this.mwController = mwController;
        }

        /// <summary>
        /// Returns the names of all Designs.
        /// </summary>
        /// <returns>Names of all Designs.</returns>
        public string[] GetDesigns()
        {
            return Designs.Keys.ToArray();
        }

        /// <summary>
        /// Returns the active design.
        /// </summary>
        /// <returns>Active design</returns>
        public Design GetActiveDesign()
        {
            return ActiveDesign;
        }

        /// <summary>
        /// Gets a design by name.
        /// </summary>
        /// <param name="name">Name of design</param>
        /// <returns>Design with the provided name</returns>
        public Design GetDesign(string name)
        {
            Design design;
            Designs.TryGetValue(name, out design);

            if (design != null)
            {
                return design;
            }
            else
            {
                return null;
            }
        }

        private Parser GetParser(string name)
        {
            Parser parser;
            Parsers.TryGetValue(name, out parser);

            if (parser != null)
            {
                return parser;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Selects a design with the provided name
        /// </summary>
        /// <param name="design">Design to select</param>
        public void SelectDesign(string design)
        {
            ActiveDesign = design == null ? null : GetDesign(design);
        }

        /// <summary>
        /// Selects a parser for the provided design name
        /// </summary>
        /// <param name="design"></param>
        public void SelectParser(string design)
        {
            ActiveParser = GetParser(design);
            ActiveDesign = GetDesign(design);
        }

        /// <summary>
        /// Creates a Design with the given name
        /// </summary>
        /// <param name="path">Path of the design</param>
        /// <returns>The Design created</returns>
        public Design CreateDesign(string path)
        {
            Design newDesign = new Design(path);

            if (!Designs.ContainsKey(newDesign.FileName))
            {
                Designs.Add(newDesign.FileName, newDesign);
                ActiveDesign = newDesign;
            }

            return newDesign;
        }

        /// <summary>
        /// Saves the provided Design.
        /// </summary>
        /// <param name="design">Design to save.</param>
        private void SaveDesign(Design design)
        {
            if (design.IsDirty)
            {
                design.SaveTextToFile();
            }
        }

        /// <summary>
        /// Saves the active Design.
        /// </summary>
        /// <returns>Whether the save was successful</returns>
        public bool SaveActiveDesign()
        {
            SaveDesign(ActiveDesign);
            return true;
        }

        /// <summary>
        /// Saves all Designs
        /// </summary>
        /// <returns>Whether the save was successful</returns>
        public bool SaveDesigns()
        {
            foreach (Design design in Designs.Values)
            {
                SaveDesign(design);
            }
            return true;
        }

        /// <summary>
        /// Closes a given Design.
        /// </summary>
        /// <param name="name">Name of Design</param>
        /// <param name="save">Indicates whether the user wants the design saved</param>
        /// <returns>Indicates whether the Design was closed</returns>
        public bool CloseDesign(string name, bool save)
        {
            Design design = GetDesign(name);

            if (design != null)
            {
                if (save)
                {
                    SaveDesign(design);
                }

                Designs.Remove(name);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Update the font sizes of all Designs.
        /// </summary>
        public void SetDesignFontSizes()
        {
            foreach (Design design in Designs.Values)
            {
                design.SetFontSize();
            }
        }

        /// <summary>
        /// Change the themes of all Designs
        /// </summary>
        public void SetThemes()
        {
            foreach (Design design in Designs.Values)
            {
                design.SetTheme();
            }
        }

        /// <summary>
        /// Parses the active design.
        /// </summary>
        /// <returns>Output of the parsed design</returns>
        public List<IObjectCodeElement> Parse()
        {
            Parser parser = new Parser(ActiveDesign);
            var output = parser.Parse();
            if (output != null)
            {
                Parsers.Add(ActiveDesign.FileName, parser);
                ActiveParser = parser;
            }
            return output;
        }

        /// <summary>
        /// Parses a tick for the active design.
        /// </summary>
        /// <returns>Output of the tick for the parsed design</returns>
        public List<IObjectCodeElement> ParseTick()
        {
            return ActiveParser.ParseTick();
        }

        /// <summary>
        /// Parses a variable click for the active design.
        /// </summary>
        /// <param name="variableName">The name of the variable that was clicked by the user</param>
        /// <param name="value">Value for formatter click</param>
        /// <returns>Output of the tick for the parsed design</returns>
        public List<IObjectCodeElement> ParseVariableClick(string variableName, string value = null)
        {
            return ActiveParser.ParseClick(variableName, value);
        }

        /// <summary>
        /// Parsers the current design text with input variables.
        /// </summary>
        /// <param name="inputVariables">Input variables</param>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseWithInput(List<Variable> inputVariables)
        {
            Parser parser = new Parser(ActiveDesign);
            var output = parser.ParseWithInput(inputVariables);
            if (output != null)
            {
                Parsers.Add(ActiveDesign.FileName, parser);
                ActiveParser = parser;
            }
            return output;
        }

        /// <summary>
        /// Parsers a sub design with the provided instantiation.
        /// </summary>
        /// <param name="instantiation">Instnatiation</param>
        /// <returns>Output of the parsed design</returns>
        public List<IObjectCodeElement> ParseSubdesign(string instantiation)
        {
            Design currentDesign = ActiveDesign;

            string designName = instantiation.Split('.')[0];
            string instantName = instantiation.Split('.')[1].TrimEnd('(');
            Design subDesign = Parsers[ActiveDesign.FileName].Subdesigns[designName];

            // Get input variables
            List<Variable> inputVariables = Parsers[ActiveDesign.FileName].GetModuleInputs(instantName, subDesign.ModuleDeclaration);

            // Parse sub design
            ActiveDesign = subDesign;
            Parser subParser = new Parser(subDesign);
            var output = subParser.ParseWithInput(inputVariables); // Parse subdesign
            if (output != null)
            {
                if (Parsers.ContainsKey(designName))
                {
                    Parsers[designName] = subParser;
                }
                else
                {
                    Parsers.Add(designName, subParser);
                }
                ActiveParser = subParser;
            }

            return output;
        }

        /// <summary>
        /// Gets the active designs current state.
        /// </summary>
        /// <returns>Active designs current state</returns>
        public List<Variable> GetActiveDesignState()
        {
            return ActiveParser.ExportState();
        }

        public void ClearParsers()
        {
            Parsers.Clear();
        }

        /// <summary>
        /// Removes a parser from the open parsers.
        /// </summary>
        /// <param name="name">Design name of the parser to close</param>
        public void CloseParser(string name)
        {
            Parsers.Remove(name);
            if (Parsers.Count == 0)
            {
                ActiveParser = null;
            }
        }
    }
}
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
using System.Text.RegularExpressions;
using VisiBoole.Controllers;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// An instance creation statement that creates an instance of design that has defined a module.
    /// </summary>
	public class InstantiationStmt : Statement
	{
        /// <summary>
        /// Regex for getting output tokens.
        /// </summary>
        private Regex OutputRegex = new Regex($@"({Parser.InstantiationPattern}\()|(~?{Parser.ScalarPattern})|[\s01:,{{}})]");

        /// <summary>
        /// Subdesign of the instantiation.
        /// </summary>
        private Design Subdesign;

        /// <summary>
        /// List of input variables
        /// </summary>
        private List<string> InputVariables;

        /// <summary>
        /// List of input values.
        /// </summary>
        private List<bool> InputValues;

        /// <summary>
        /// List of no concant values in the instaniation.
        /// </summary>
        private List<bool> NoContactValues;

        /// <summary>
        /// Constructs a SubmoduleInstatiationStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
        /// <param name="subdesign">Subdesign of instantiation</param>
		public InstantiationStmt(string text, Design subdesign) : base(text)
		{
            Subdesign = subdesign;
            InputVariables = new List<string>();
            InputValues = new List<bool>();
            NoContactValues = new List<bool>();
        }

        /// <summary>
        /// Runs the instance and returns whether the instance was successful or not
        /// </summary>
        /// <returns>Whether the instance was successful or not</returns>
        public bool TryRunInstance()
        {
            // Save current design
            Design currentDesign = DesignController.ActiveDesign;
            // Get input side text
            string inputSideText = Text.Substring(Text.IndexOf('('), Text.IndexOf(':') + 1 - Text.IndexOf('('));
            // Get output side text
            string outputSideText = Text.Substring(Text.IndexOf('(') + inputSideText.Length);

            InputVariables = new List<string>();
            InputValues = new List<bool>();
            MatchCollection matches = Parser.VariableRegex.Matches(inputSideText);
            foreach (Match match in matches)
            {
                InputVariables.Add(match.Value);
                InputValues.Add(currentDesign.Database.GetValue(match.Value) == 1);
            }

            Parser subParser = new Parser(Subdesign);
            DesignController.ActiveDesign = Subdesign;
            List<bool> outputValues = subParser.ParseAsModule(InputValues);
            // Reset active design
            DesignController.ActiveDesign = currentDesign;
            // If no output was returned
            if (outputValues == null)
            {
                // Return false for error
                return false;
            }

            int outputValueIndex = 0;
            NoContactValues = new List<bool>();
            matches = Parser.VariableRegex.Matches(outputSideText);
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token != "NC")
                {
                    currentDesign.Database.SetValue(token, outputValues[outputValueIndex]);
                }
                else
                {
                    NoContactValues.Add(outputValues[outputValueIndex]);
                }
                outputValueIndex++;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the instantiation was reran due to new input values.
        /// </summary>
        /// <returns>Whether the instantiation was reran due to new input values.</returns>
        public bool CheckRerun()
        {
            // Start rerun with false
            bool rerun = false;
            // For each input variable in input variables
            for (int i = 0; i < InputVariables.Count; i++)
            {
                // Get the input variable
                string variable = InputVariables[i];
                // If the input variable has a new value
                if (DesignController.ActiveDesign.Database.GetValue(variable) == 1 != InputValues[i])
                {
                    // Set rerun to true
                    rerun = true;
                    // Break out of loop
                    break;
                }
            }

            // If instantiation needs to be reran
            if (rerun)
            {
                // Rerun instance
                TryRunInstance();
                // Return true for the instantiation being ran again
                return true;
            }

            // Return false for the instantiation was not ran again
            return false;
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override List<IObjectCodeElement> Parse()
        {
            var output = new List<IObjectCodeElement>();
            int seperatorIndex = Text.IndexOf(':');
            int currentNoContactIndex = 0;
            MatchCollection matches = OutputRegex.Matches(Text);
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == " ")
                {
                    output.Add(new SpaceFeed());
                }
                else if (token == "\n")
                {
                    // Output newline
                    output.Add(new LineFeed());
                }
                else if (token.Contains("("))
                {
                    output.Add(new Instantiation(token));
                }
                else if (token == "," || token == "{" || token == "}" || token == ":" || token == ")")
                {
                    output.Add(new Operator(token));
                }
                else
                {
                    if (match.Index > seperatorIndex && token == "NC")
                    {
                        output.Add(new DependentVariable(token, NoContactValues[currentNoContactIndex++]));
                    }
                    else
                    {
                        if (char.IsDigit(token[0]))
                        {
                            output.Add(new Constant(token));
                        }
                        else
                        {
                            IndependentVariable indVar = DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(token) as IndependentVariable;
                            DependentVariable depVar = DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>(token) as DependentVariable;
                            if (indVar != null)
                            {
                                output.Add(indVar);
                            }
                            else if (depVar != null)
                            {
                                output.Add(depVar);
                            }
                        }
                    }
                }
            }

            // Output ending semicolon
            output.Add(new Operator(";"));
            // Output new line
            output.Add(new LineFeed());
            // Return output list
            return output;
        }
	}
}
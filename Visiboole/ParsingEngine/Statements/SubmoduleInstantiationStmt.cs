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
	public class SubmoduleInstantiationStmt : Statement
	{
        /// <summary>
        /// Regex for getting output tokens.
        /// </summary>
        private Regex OutputRegex = new Regex($@"({Parser.InstantiationPattern}\()|(~?{Parser.ConstantPattern})|(~?{Parser.ScalarPattern})|[\s:,{{}})]");

        /// <summary>
        /// Design path of the instantiation.
        /// </summary>
        private string DesignPath;

        /// <summary>
        /// List of no concant values in the instaniation.
        /// </summary>
        private List<bool> NoContactValues;

        /// <summary>
        /// Constructs a SubmoduleInstatiationStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
        /// <param name="designPath">Path to the design</param>
		public SubmoduleInstantiationStmt(string text, string designPath) : base(text)
		{
            DesignPath = designPath;
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
            // Create input values list
            List<bool> inputValues = new List<bool>();
            // Get input side text
            string inputSideText = Text.Substring(Text.IndexOf('('), Text.IndexOf(':') + 1 - Text.IndexOf('('));
            // Get output side text
            string outputSideText = Text.Substring(Text.IndexOf('(') + inputSideText.Length);

            MatchCollection matches = Parser.VariableRegex.Matches(inputSideText);
            foreach (Match match in matches)
            {
                inputValues.Add(currentDesign.Database.TryGetValue(match.Value) == 1);
            }

            Design subDesign = new Design(DesignPath);
            Parser subParser = new Parser(subDesign);
            DesignController.ActiveDesign = subDesign;
            List<bool> outputValues = subParser.ParseAsModule(inputValues);
            if (outputValues == null)
            {
                return false;
            }

            int outputValueIndex = 0;
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

            // Reset active design
            DesignController.ActiveDesign = currentDesign;

            return true;
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            int seperatorIndex = Text.IndexOf(':');
            int currentNoContactIndex = 0;
            MatchCollection matches = OutputRegex.Matches(Text);
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == " ")
                {
                    Output.Add(new SpaceFeed());
                }
                else if (token == "\n")
                {
                    // Output newline
                    Output.Add(new LineFeed());
                }
                else if (token.Contains("("))
                {
                    Output.Add(new Instantiation(token));
                }
                else if (token == "," || token == "{" || token == "}" || token == ":" || token == ")")
                {
                    OutputOperator(token);
                }
                else
                {
                    if (match.Index < seperatorIndex)
                    {
                        // Output each input var in the input list
                        IndependentVariable indVar = DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(token) as IndependentVariable;
                        DependentVariable depVar = DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>(token) as DependentVariable;
                        if (indVar != null)
                        {
                            Output.Add(indVar);
                        }
                        else
                        {
                            Output.Add(depVar);
                        }
                    }
                    else
                    {
                        bool value = token != "NC"
                            ? DesignController.ActiveDesign.Database.TryGetValue(token) == 1
                            : NoContactValues[currentNoContactIndex++];
                        Output.Add(new DependentVariable(token, value));
                    }
                }
            }

            base.Parse();
        }
	}
}
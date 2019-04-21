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
        private Regex OutputRegex = new Regex($@"({Parser.InstantiationPattern}\()|(~?{Parser.ConstantPattern})|(~?{Parser.ScalarPattern})|[\s;:,{{}})]");

        /// <summary>
        /// Design path of the instantiation.
        /// </summary>
        private string DesignPath;

        /// <summary>
        /// Constructs a SubmoduleInstatiationStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
        /// <param name="designPath">Path to the design</param>
		public SubmoduleInstantiationStmt(string text, string designPath) : base(text)
		{
            DesignPath = designPath;

            // Initialize variables in the statement
            InitVariables(text);
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            // Save current design
            Design currentDesign = DesignController.ActiveDesign;
            // Create input values list
            List<bool> inputValues = new List<bool>();
            // Get input side text
            string inputSideText = Text.Substring(0, Text.IndexOf(':') + 1);
            // Get output side text
            string outputSideText = Text.Substring(inputSideText.Length);

            MatchCollection matches = OutputRegex.Matches(inputSideText);
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == " ")
                {
                    Output.Add(new SpaceFeed());
                }
                else if (token.Contains("("))
                {
                    Output.Add(new Instantiation(token));
                }
                else if (token == "," || token == "{" || token == "}" || token == ":")
                {
                    OutputOperator(token);
                }
                else
                {
                    // Output each input var in the input list
                    IndependentVariable indVar = DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(token) as IndependentVariable;
                    DependentVariable depVar = DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>(token) as DependentVariable;
                    if (indVar != null)
                    {
                        Output.Add(indVar);
                        inputValues.Add(indVar.Value);
                    }
                    else
                    {
                        Output.Add(depVar);
                        inputValues.Add(depVar.Value);
                    }
                }
            }

            Design subDesign = new Design(DesignPath);
            Parser subParser = new Parser(subDesign);
            DesignController.ActiveDesign = subDesign;
            List<bool> outputValues = subParser.ParseAsModule(inputValues);
            if (outputValues == null)
            {
                Output.Add(new Comment(outputSideText)); // Output as comment since there was an error
            }
            else
            {
                // Output input variables
                int outputValueIndex = 0;

                matches = OutputRegex.Matches(outputSideText);
                foreach (Match match in matches)
                {
                    string token = match.Value;
                    if (token == " ")
                    {
                        Output.Add(new SpaceFeed());
                    }
                    else if (token == "," || token == "{" || token == "}" || token == ")" || token == ";")
                    {
                        OutputOperator(token);
                    }
                    else
                    {
                        if (token != "NC")
                        {
                            Output.Add(new DependentVariable(token, outputValues[outputValueIndex]));
                        }
                        else
                        {
                            Output.Add(new Comment("NC"));
                        }
                        outputValueIndex++;
                    }
                }
            }

            // Reset active design
            DesignController.ActiveDesign = currentDesign;

            // Output newline
            Output.Add(new LineFeed());
        }
	}
}
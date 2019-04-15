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
        private string DesignPath;

        /// <summary>
        /// Constructs a SubmoduleInstatiationStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
        /// <param name="designPath">Path to the design</param>
		public SubmoduleInstantiationStmt(string text, string designPath) : base(text)
		{
            DesignPath = designPath;
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            Match instantiationMatch = Regex.Match(Text, Parser.ModuleInstantiationPattern);

            // Output padding (if present)
            for (int i = 0; i < instantiationMatch.Groups["Padding"].Value.Length; i++)
            {
                Output.Add(new SpaceFeed());
            }

            // Output instantiation
            Output.Add(new Instantiation($"{instantiationMatch.Groups["Instantiation"].Value}("));

            // Output input variables
            List<bool> inputValues = new List<bool>();
            string[] inputLists = Regex.Split(instantiationMatch.Groups["Inputs"].Value, @",\s+");
            for (int i = 0; i < inputLists.Length; i++)
            {
                string inputList = inputLists[i];

                Output.Add(new Comment("{")); // Add starting concat

                foreach (string var in Parser.WhitespaceRegex.Split(inputList.Substring(1, inputList.Length - 2)))
                {
                    // Output each input var in the input list
                    IndependentVariable indVar = DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(var) as IndependentVariable;
                    DependentVariable depVar = DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>(var) as DependentVariable;

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

                Output.Add(new Comment("}")); // Add ending concat

                if (i < inputLists.Length - 1)
                {
                    Output.Add(new Comment(",")); // Add comma seperator if not last list
                }
            }

            // Output seperator
            Output.Add(new Comment(":"));

            // Get output values
            Design currentDesign = DesignController.ActiveDesign;
            Design subDesign = new Design(DesignPath);
            Parser subParser = new Parser(subDesign);
            DesignController.ActiveDesign = subDesign;
            List<bool> outputValues = subParser.ParseAsModule(inputValues);

            // Output output
            if (outputValues == null)
            {
                Output.Add(new Comment(instantiationMatch.Groups["Outputs"].Value)); // Output as comment since there was an error
            }
            else
            {
                int outputValueIndex = 0;
                string[] outputLists = Regex.Split(instantiationMatch.Groups["Outputs"].Value, @",\s+");
                for (int i = 0; i < outputLists.Length; i++)
                {
                    string outputList = outputLists[i];

                    Output.Add(new Comment("{")); // Add starting concat

                    foreach (string var in Parser.WhitespaceRegex.Split(outputList.Substring(1, outputList.Length - 2)))
                    {
                        // Output each input var in the input list
                        if (var != "NC")
                        {
                            Output.Add(new DependentVariable(var, outputValues[outputValueIndex]));
                        }
                        else
                        {
                            Output.Add(new Comment("NC"));
                        }
                        outputValueIndex++;
                    }

                    Output.Add(new Comment("}")); // Add ending concat

                    if (i < outputLists.Length - 1)
                    {
                        Output.Add(new Comment(",")); // Add comma seperator if not last list
                    }
                }
            }

            // Reset active design
            DesignController.ActiveDesign = currentDesign;

            // Output ending ) and newline
            Output.Add(new Comment(")"));
            Output.Add(new LineFeed());
        }
	}
}
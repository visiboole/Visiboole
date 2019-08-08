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
        private Regex OutputRegex = new Regex($@"({Parser.InstantPattern}\()|(~?{Parser.ScalarPattern})|[\s01:,{{}})]");

        /// <summary>
        /// Design instantiation of the statement.
        /// </summary>
        private DesignInstantiation Instantiation;

        /// <summary>
        /// Constructs a SubmoduleInstatiationStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
        /// <param name="subdesign">Subdesign of instantiation</param>
		public InstantiationStmt(string text, DesignInstantiation instantiation) : base(text)
		{
            Instantiation = instantiation;
        }

        /// <summary>
        /// Runs the instance and returns whether the instance was successful or not
        /// </summary>
        /// <returns>Whether the instance was successful or not</returns>
        public bool TryRunInstance(bool tick)
        {
            return Instantiation.TryRun(tick);
        }

        /// <summary>
        /// Returns whether the instantiation was reran due to new input values.
        /// </summary>
        /// <returns>Whether the instantiation was reran due to new input values.</returns>
        public bool CheckRerun()
        {
            return Instantiation.CheckRerun();
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override List<IObjectCodeElement> Parse()
        {
            // Create output list to return
            var output = new List<IObjectCodeElement>();
            // Get input and output seperator index
            int seperatorIndex = Text.IndexOf(':');
            // Start no contact index at 0
            int currentNoContactIndex = 0;
            // Get all output matches
            MatchCollection matches = OutputRegex.Matches(Text);
            // For each output match
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
                        output.Add(new DependentVariable(token, Instantiation.NoContactValues[currentNoContactIndex++]));
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
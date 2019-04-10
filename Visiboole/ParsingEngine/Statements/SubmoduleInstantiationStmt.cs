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
using System.Text.RegularExpressions;
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
        /// Regex for getting instantiation tokens (variables and concatenations).
        /// </summary>
        private static Regex TokenRegex = new Regex($@"({Parser.ConcatenationPattern}|{Parser.VariablePattern1})");

        /// <summary>
        /// Constructs a SubmoduleInstatiationStmt instance.
        /// </summary>
        /// <param name="database">Database of the parsed design</param>
        /// <param name="text">Text of the statement</param>
        /// <param name="instantiation">Instantiation object</param>
		public SubmoduleInstantiationStmt(string text) : base(text)
		{
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            // Output padding (if present)
            Match padding = Parser.WhitespaceRegex.Match(Text);
            if (padding.Success)
            {
                for (int i = 0; i < padding.Value.Length; i++)
                {
                    Output.Add(new SpaceFeed());
                }
            }

            // Output instantiation
            Match match = Regex.Match(Text, Parser.InstantiationNotationPattern);
            Output.Add(new Instantiation(match.Value));

            /*
            // Output seperator
            OutputOperator(":");

            // Add variables and concatenations to output
            string instantiationVariables = Regex.Match(Text.Substring(Text.IndexOf('(')), $@"({Parser.ScalarPattern2}(\s+{Parser.ScalarPattern2})*)(,\s+({Parser.ScalarPattern2}(\s+{Parser.ScalarPattern2})*))*").Value;
            string[] variableLists = Regex.Split(instantiationVariables, @",\s+");
            for (int i = 0; i < variableLists.Length; i++)
            {
                string variableList = variableLists[i];

                MatchCollection matches = Parser.ScalarRegex2.Matches(variableList);
                if (matches.Count > 1)
                {
                    // Output {
                    OutputOperator("{");
                }
                foreach (Match match in matches)
                {
                    if (match.Value == "NC")
                    {
                        OutputOperator("NC");
                    }
                    else
                    {
                        IndependentVariable indVar = Database.TryGetVariable<IndependentVariable>(match.Value) as IndependentVariable;
                        DependentVariable depVar = Database.TryGetVariable<DependentVariable>(match.Value) as DependentVariable;
                        if (indVar != null)
                        {
                            Output.Add(indVar);
                        }
                        else
                        {
                            Output.Add(depVar);
                        }
                    }
                }
                if (matches.Count > 1)
                {
                    OutputOperator("}");
                }

                if (i < variableLists.Length - 1)
                {
                    OutputOperator(",");
                }
            }
            */

            // Output newline
            Output.Add(new LineFeed());
        }
	}
}

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
    /// The format of a submodule instantiation statement is identical to the module declaration statement except they
    /// are preceded by the commercial at(@) and give a numeric value to parameters. They create an
    /// instance of a module described in another file(with the same name) as part of the current design
    /// </summary>
	public class SubmoduleInstantiationStmt : Statement
	{
        /// <summary>
        /// Name of the design being instantiated
        /// </summary>
        private string DesignName;

        /// <summary>
        /// Path of the design being instantiated
        /// </summary>
        private string DesignPath;

        /// <summary>
        /// Regex for getting format specifier tokens (format specifiers and extra spacing).
        /// </summary>
        private static Regex TokenRegex = new Regex($@"({Parser.VariablePattern}|{Parser.ConcatenationPattern})");

        /// <summary>
        /// Constructs an instance of SubmoduleInstantiationStmt at given linenumber with txt string input
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
		public SubmoduleInstantiationStmt(int lnNum, string txt, string designName, string designPath) : base(lnNum, txt)
		{
            DesignName = designName;
            DesignPath = designPath;
		}

        /// <summary>
        /// Parses the Text of this statement into a list of discrete IObjectCodeElement elements
        /// to be used by the html parser to generate formatted output to be displayed in simulation mode.
        /// </summary>
        public override void Parse()
        {
            // Add padding if present to output
            Match padding = Regex.Match(Text, @"\s+");
            if (padding.Success)
            {
                for (int i = 0; i < padding.Value.Length; i++)
                {
                    Output.Add(new SpaceFeed());
                }
            }

            // Add instantiation to output
            Match instantiation = Regex.Match(Text, Parser.InstantiationNotationPattern);
            Output.Add(new Instantiation(instantiation.Value, DesignName, DesignPath));

            // Add variables and concatenations to output
            MatchCollection matches = TokenRegex.Matches(Text);
            foreach (Match match in matches)
            {
                /*
                // Variables and concatenations
                string[] var;
                if (!match.Value.Contains("{"))
                {
                    var = new string[] { match.Value };
                }
                else
                {
                    var = Regex.Split(match.Groups["Vars"].Value, @"\s+");
                }

                // Output vars
                */
            }

            Output.Add(new LineFeed());
        }
	}
}

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

using System.Text.RegularExpressions;
using VisiBoole.ParsingEngine.ObjectCode;
using System;
using System.Collections.Generic;
using VisiBoole.Models;
using VisiBoole.Controllers;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// A variable list statement that can be interacted with by the user.
    /// </summary>
	public class VariableListStmt : Statement
	{
        /// <summary>
        /// Regex for getting variable list tokens (extra spaces and scalars).
        /// </summary>
        private static Regex TokenRegex = new Regex($@"(('b[0-1])|{Lexer.ScalarPattern}|\s|;)");

        /// <summary>
        /// Constructs a VariableListStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
		public VariableListStmt(string text) : base(text)
		{
            // Initialize variables in the statement
            InitVariables(text);
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
		{
            // Clean content and make format string
            MatchCollection matches = TokenRegex.Matches(Text);
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (String.IsNullOrWhiteSpace(token))
                {
                    Output.Add(new SpaceFeed());
                }
                else if (token == ";")
                {
                    // Output ;
                    OutputOperator(";");
                }
                else
                {
                    OutputVariable(match.Value);
                }
            }

            // Output newline
            Output.Add(new LineFeed());
        }
	}
}
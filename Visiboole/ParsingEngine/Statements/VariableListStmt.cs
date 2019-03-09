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

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// A list of visiboole independent variables that can be interacted with by the user
    /// </summary>
	public class VariableListStmt : Statement
	{
        /// <summary>
        /// Constructs an instance of VariableListStmt
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
		public VariableListStmt(int lnNum, string txt) : base(lnNum, txt)
		{
		}

	    /// <summary>
	    /// Parses the Text of this statement into a list of discrete IObjectCodeElement elements
	    /// to be used by the html parser to generate formatted output to be displayed in simulation mode.
	    /// </summary>
        public override void Parse()
		{
            // Clean content and make format string
            MatchCollection matches = Regex.Matches(Text, $@"({Parser.NamePattern}|{Parser.SpacingPattern})");
            foreach (Match match in matches)
            {
                if (String.IsNullOrWhiteSpace(match.Value))
                {
                    for (int i = 0; i < match.Value.Length; i++)
                    {
                        Output.Add(new SpaceFeed());
                    }
                }
                else
                {
                    string var = (match.Value[0] == '*') ? match.Value.Substring(1) : match.Value;

                    IndependentVariable indVar = Globals.TabControl.SelectedTab.Design().Database.TryGetVariable<IndependentVariable>(var) as IndependentVariable;
                    DependentVariable depVar = Globals.TabControl.SelectedTab.Design().Database.TryGetVariable<DependentVariable>(var) as DependentVariable;
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

            Output.Add(new LineFeed());
        }
	}
}
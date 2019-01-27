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
        /// Regex for a Variable List Statement
        /// </summary>
        public static readonly Regex Regex = new Regex (
            @"^"                                    // Start of Line
            + @"\s*"                                // Any Number of Whitespace
            + Globals.PatternVariable               // Variable List
            + @"("                                  // Begin Optional Group
                + @"\s*"                            // Any Number of Whitespace
                + Globals.PatternVariable           // Variable List
            + @")*"                                 // End Optional Group
            + @"\;$"                                // Ending ;
        );

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
            string content = Regex.Replace(Text, @"[;]", string.Empty); // Remove syntax
            string outputFormat = Regex.Replace(content, @"\s", "_"); // Get output format with spacing
            outputFormat = Regex.Replace(outputFormat, @"\*?" + Globals.PatternAnyVariableType, "X"); // Replace variables
            outputFormat = Regex.Replace(outputFormat, @"_X", "X"); // Remove one extra space
            outputFormat = Regex.Replace(outputFormat, @"\s", string.Empty); // Remove spacing

            // Split variables by whitespace
            string[] variables = Regex.Split(content.Trim(), @"\s+");

            // Output all variables
            int index = 0;
            foreach (char c in outputFormat)
            {
                if (c == '_')
                {
                    SpaceFeed sf = new SpaceFeed();
                    Output.Add(sf);
                }
                else
                {
                    string var = variables[index++]; // Variable to be created
                    var = (var[0] == '*') ? var.Substring(1) : var;

                    IndependentVariable indVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(var) as IndependentVariable;
                    DependentVariable depVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(var) as DependentVariable;
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

            LineFeed lf = new LineFeed();
            Output.Add(lf);
		}
	}
}
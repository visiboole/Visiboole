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
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// Comment statements provide a way to document either just the code
    /// or to label the screen during simulation.
    /// </summary>
	public class CommentStmt : Statement, IObjectCodeElement
	{
	    /// <summary>
	    /// The identifying pattern that can be used to identify and extract this statement from raw text
	    /// </summary>
        public static Regex Regex { get; } = new Regex(@"^"".*""$");

        /// <summary>
        /// Constructs an instance of CommentStmt
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
        public CommentStmt(int lnNum, string txt) : base(lnNum, txt)
		{
		}

	    /// <summary>
	    /// Parses the Text of this statement into a list of discrete IObjectCodeElement elements
	    /// to be used by the html parser to generate formatted output to be displayed in simulation mode.
	    /// </summary>
        public override void Parse()
		{
			// only add comments to simulator if the user has the setting enabled
			if (Properties.Settings.Default.SimulationComments)
			{
				Output.Add(this);
			}
            LineFeed lf = new LineFeed();
            Output.Add(lf);
		}

		#region IObjectCodeElement attributes

		public bool? ObjCodeValue { get; set; } = false;
		public string ObjCodeText { get { return Text; } set { } } 
        public int Match { get; set; }
        public int MatchingIndex { get; set; }

		#endregion

	}
}
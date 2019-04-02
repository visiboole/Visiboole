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
    /// A description statement that provides a way to document code or label the screen.
    /// </summary>
	public class CommentStmt : Statement, IObjectCodeElement
	{
        #region IObjectCodeElement attributes

        public bool? ObjCodeValue { get; set; } = false;
        public string ObjCodeText { get { return Text; } set { } }
        public int Match { get; set; }
        public int MatchingIndex { get; set; }

        #endregion

        /// <summary>
        /// Constructs a CommentStmt instance.
        /// </summary>
        /// <param name="database">Database of the parsed design</param>
        /// <param name="text">Text of the statement</param>
        public CommentStmt(Database database, string text) : base(database, text)
		{
		}

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
		{
            // Output padding (if present)
            Match comment = Parser.CommentRegex.Match(Text);
            foreach (char space in comment.Groups["Spacing"].Value)
            {
                Output.Add(new SpaceFeed());
            }

            // Remove "" and ;
            Text = String.Concat(comment.Groups["Comment"].Value.Substring(1, comment.Groups["Comment"].Value.Length - 3));

            // Output comment and newline
            Output.Add(this);
            Output.Add(new LineFeed());
		}
	}
}
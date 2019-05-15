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
using System.Text;
using System.Text.RegularExpressions;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// A description statement that provides a way to document code or label the screen.
    /// </summary>
	public class CommentStmt : Statement
	{
        /// <summary>
        /// Constructs a Comment Statement with the provided comment text.
        /// </summary>
        /// <param name="comment">Comment text</param>
        public CommentStmt(string text) : base(text)
        {
        }

        /// <summary>
        /// Parses the tokens of this statement into a list of output elements.
        /// </summary>
        public override List<IObjectCodeElement> Parse()
        {
            return new List<IObjectCodeElement>(new IObjectCodeElement[] { new Comment(Text), new LineFeed() });
        }
	}
}
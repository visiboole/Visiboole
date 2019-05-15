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

using System.Collections.Generic;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// Base class for Visiboole statements.
    /// </summary>
	public abstract class Statement
	{
        /// <summary>
        /// Text of the statement.
        /// </summary>
		public string Text { get; private set; }

        /// <summary>
        /// Constructs a Statement with the provided text.
        /// </summary>
        /// <param name="text">Text of the statement</param>
		protected Statement(string text)
		{
			Text = text;
		}

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
		public abstract List<IObjectCodeElement> Parse();
    }
}
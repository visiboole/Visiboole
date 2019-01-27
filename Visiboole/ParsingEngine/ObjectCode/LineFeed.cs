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

namespace VisiBoole.ParsingEngine.ObjectCode
{
    /// <summary>
    /// A discrete element of output representing a linefeed
    /// </summary>
	public class LineFeed : IObjectCodeElement
	{
        /// <summary>
        /// The text representation of this outpute element, a newline character
        /// </summary>
		public string ObjCodeText { get { return Environment.NewLine; } set { } }

        /// <summary>
        /// The value of this element is null as it is a newline character, not a variable
        /// </summary>
		public bool? ObjCodeValue { get { return null; }set { } }

        public int Match { get; set; }
        public int MatchingIndex { get; set; }
    }
}
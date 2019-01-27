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

namespace VisiBoole.ParsingEngine.ObjectCode
{
    /// <summary>
    /// A variable that is assigned an expression; e.g. to the left of the "=" sign
    /// </summary>
	public class DependentVariable : Variable, IObjectCodeElement
	{
        /// <summary>
        /// The boolean value of this variable to be added to the statement's Output
        /// </summary>
		public bool? ObjCodeValue { get { return Value; } set { } }

        /// <summary>
        /// The string representation of this variable to be added to the statement's Output
        /// </summary>
		public string ObjCodeText { get { return Name; } set { } }

        public int Match { get; set; }
        public int MatchingIndex { get; set; }

        /// <summary>
        /// Constructs an instance of DependentVariable with name and value
        /// </summary>
        /// <param name="name">The string name of this variable</param>
        /// <param name="value">The boolean value of this variable</param>
		public DependentVariable(string name, bool value)
		{
			Name = name;
			Value = value;
		}
	}
}

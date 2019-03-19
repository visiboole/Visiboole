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
    /// A discrete element of output representing a mathematical operator
    /// </summary>
	public class Constant : IObjectCodeElement
	{
        private bool? Value = false;
        /// <summary>
        /// The string representation of this output element
        /// </summary>
		public string ObjCodeText { get { return ConstantText; } }

        /// <summary>
        /// The boolean value of this output element, null
        /// </summary>
		public bool? ObjCodeValue { get { return Value; } set { Value = value; } }

        /// <summary>
        /// The string representation of this element
        /// </summary>
		public string ConstantText { get; set; }

        public int Match { get; set; }
        public int MatchingIndex { get; set; }

        /// <summary>
        /// Constructs an instance of Operator 
        /// </summary>
        /// <param name="opChar">The string representation of this element</param>
		public Constant(string constantText)
		{
			ConstantText = constantText;
            Value = Convert.ToInt32(constantText[2].ToString()) == 1;
        }
	}
}
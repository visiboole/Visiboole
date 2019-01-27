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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisiBoole.ParsingEngine.ObjectCode
{
    public class Parentheses: IObjectCodeElement
    {
        public bool? Value;

        public string ObjCodeText { get { return OperatorChar; } }

        /// <summary>
        /// The boolean value of this output element, null
        /// </summary>
		public bool? ObjCodeValue { get { return this.Value; } set { this.Value = value; } }

        /// <summary>
        /// The string representation of this element
        /// </summary>
		public string OperatorChar { get; set; }

        public int Match { get; set; }

        public int MatchingIndex { get; set; }

        /// <summary>
        /// Constructs an instance of Operator 
        /// </summary>
        /// <param name="opChar">The string representation of this element</param>
		public Parentheses(string opChar)
        {
            OperatorChar = opChar;
            this.Value = false;
        }
    }
}

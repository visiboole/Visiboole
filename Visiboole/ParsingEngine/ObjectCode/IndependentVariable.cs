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
    /// Variable whose value does not depend on that of any other. (Right of an "=" sign)
    /// </summary>
    public class IndependentVariable : Variable, IObjectCodeElement
	{
        /// <summary>
        /// String representation of this output element.
        /// </summary>
        public string ObjCodeText { get { return Name; } private set { } }

        /// <summary>
        /// Boolean value of this output element.
        /// </summary>
		public bool? ObjCodeValue { get { return Value; } private set { } }

        /// <summary>
        /// Indicates whether this output element contains a negation.
        /// </summary>
        public bool ObjHasNegation { get; private set; }

        /// <summary>
        /// Constructs a independent variable instance with the provided name and value.
        /// </summary>
        /// <param name="name">String representation of this variable</param>
        /// <param name="value">Boolean value of this variable</param>
		public IndependentVariable(string name, bool value)
        {
            ObjHasNegation = name[0] == '~';
            Name = (ObjHasNegation) ? name.Substring(1) : name;
            Value = value;
        }
    }
}
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

namespace VisiBoole.ParsingEngine.ObjectCode
{
    /// <summary>
    /// A discrete element of output representing a mathematical operator.
    /// </summary>
	public class AltClock : IObjectCodeElement
	{
        /// <summary>
        /// String representation of this output element.
        /// </summary>
		public string ObjCodeText { get; private set; }

        /// <summary>
        /// Boolean value of this output element.
        /// </summary>
		public bool? ObjCodeValue { get; private set; }

        /// <summary>
        /// Indicates whether this output element contains a negation.
        /// </summary>
        public bool ObjHasNegation { get; private set; }

        /// <summary>
        /// Constructs an alternating clock instace with the provided name. 
        /// </summary>
        /// <param name="name">Variable name of the alternating clock</param>
		public AltClock(string name, bool value)
		{
			ObjCodeText = name;
            ObjCodeValue = value;
            ObjHasNegation = false;
		}

        /// <summary>
        /// Updates the value of the alternating clock.
        /// </summary>
        /// <param name="value">New value of the clock</param>
        public void UpdateValue(bool value)
        {
            ObjCodeValue = value;
        }
	}
}
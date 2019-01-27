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
    /// A discrete element of output consumed by the html parser
    /// </summary>
	public interface IObjectCodeElement
	{
        /// <summary>
        /// The text representation printed in simulation mode output
        /// </summary>
		string ObjCodeText { get; }

        /// <summary>
        /// The boolean value of this element
        /// </summary>
		bool? ObjCodeValue { get; set; }

        int Match { get; set; }

        int MatchingIndex { get; set; }
    }
}
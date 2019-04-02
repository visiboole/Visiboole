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
    /// A discrete element of output representing an instantiation
    /// </summary>
	public class Instantiation : IObjectCodeElement
	{
        private bool? Value = false;
        /// <summary>
        /// The string representation of this output element
        /// </summary>
		public string ObjCodeText { get; private set; }

        /// <summary>
        /// The boolean value of this output element, null
        /// </summary>
		public bool? ObjCodeValue { get { return Value; } set { this.Value = value; } }

        public int Match { get; set; }
        public int MatchingIndex { get; set; }

        /// <summary>
        /// Name of the instantiation
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Name of the design being instantiated
        /// </summary>
        public string DesignName { get; private set; }

        /// <summary>
        /// Path to the instantiation module
        /// </summary>
        public string DesignPath { get; private set; }

        /// <summary>
        /// Constructs an instantiation instance
        /// </summary>
        /// <param name="instantiation">Text of instantiation</param>
        /// <param name="designPath">Path to design being instantiated</param>
		public Instantiation(string instantiation, string designPath)
		{
			ObjCodeText = instantiation;
            DesignName = instantiation.Split('.')[0];
            Name = instantiation.Split('.')[1];
            DesignPath = designPath;
            Value = null;
		}
	}
}
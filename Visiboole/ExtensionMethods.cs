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
using System.IO;
using System.Windows.Forms;
using VisiBoole.Models;

namespace VisiBoole
{
	/// <summary>
	/// Extension methods for this application
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Returns the Design displayed by this tabpage
		/// </summary>
		/// <param name="tab">The parent control for the subdesign</param>
		/// <returns>Returns the Design for this tabpage</returns>
		public static Design Design(this TabPage tab)
		{
			foreach (Control c in tab.Controls)
			{
                if ((c as Design) != null)
                {
                    return c as Design;
                }
			}
			return null;
		}
	}
}

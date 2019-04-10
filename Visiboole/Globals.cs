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
using System.Windows.Forms;
using VisiBoole.Views;

namespace VisiBoole
{
    /// <summary>
    /// The different display types for the UserControl displays that are hosted by the MainWindow
    /// </summary>
    public enum DisplayType
    {
        EDIT,
        RUN,
        NONE
    }

    /// <summary>
    /// Global variables for this application
    /// </summary>
    public static class Globals
	{
        /// <summary>
        /// Tab Control
        /// </summary>
        public static TabControl TabControl = null;
    }
}
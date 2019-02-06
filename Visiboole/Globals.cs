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

namespace VisiBoole
{
	/// <summary>
	/// Global variables for this application
	/// </summary>
	public static class Globals
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
        /// Error-handling method for errors in the application
        /// </summary>
        /// <param name="e"></param>
        public static void DisplayException(Exception e)
		{
			Cursor.Current = Cursors.Default;
			MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

        /// <summary>
        /// Tab Control
        /// </summary>
        public static TabControl TabControl = null;

        /// <summary>
        /// Theme
        /// </summary>
        public static string Theme = Properties.Settings.Default.Theme;

        /// <summary>
        /// Font size
        /// </summary>
        public static float FontSize = Properties.Settings.Default.FontSize;

        /// <summary>
        /// Whether Color Blind mode is toggled
        /// </summary>
        public static bool ColorBlind = Properties.Settings.Default.Colorblind;

        /// <summary>
        /// Regular Expression Patterns for Variables
        /// </summary>
        public static readonly string PatternVariable = @"(\*?[a-zA-Z0-9]+)";
        public static readonly string PatternVector = @"((?<Name>\*?[a-zA-Z0-9]+)\[(?<LeftBound>\d+)\.\.(?<RightBound>\d+)\])";
        public static readonly string PatternStepVector = @"((?<Name>\*?[a-zA-Z0-9]+)\[(?<LeftBound>\d+)\.(?<Step>\d+)\.(?<RightBound>\d+)\])";
        public static readonly string PatternAnyVectorType = @"(" + PatternStepVector + @"|" + PatternVector + @")";
        public static readonly string PatternAnyVariableType = @"(" + PatternStepVector + @"|" + PatternVector + @"|" + PatternVariable + @")";
        public static readonly string PatternConstant = @"((\'[hH][a-fA-F\d]+)|(\'[dD]\d+)|(\'[bB][0-1]+))";
    }
}
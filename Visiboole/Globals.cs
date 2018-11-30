using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VisiBoole.Models;

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
        public static TabControl tabControl = null;

        /// <summary>
        /// Theme
        /// </summary>
        public static string Theme = "dark";

        /// <summary>
        /// Font size
        /// </summary>
        public static float FontSize = 12;

        /// <summary>
        /// Regex Expressions for all type of variables
        /// </summary>
        public static readonly string regexVariable = @"([a-zA-Z0-9]+)";
        public static readonly string regexArrayVariables = @"([a-zA-Z0-9]+\[[0-9]+\.\.[0-9]+\])";
        public static readonly string regexStepArrayVariables = @"([a-zA-Z0-9]+\[[0-9]+\.[0-9]+\.[0-9]+\])";
        public static readonly string regexConstant = @"((\'[hH][a-fA-F0-9]+)|(\'[dD][0-9]+)|(\'[bB][0-1]+))";
    }
}
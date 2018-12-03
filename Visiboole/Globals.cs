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
        /// Regular Expression Patterns for Variables
        /// </summary>
        public static readonly string PatternVariable = @"([a-zA-Z0-9]+)";
        public static readonly string PatternVector = @"([a-zA-Z0-9]+\[\d+\.\.\d+\])";
        public static readonly string PatternStepVector = @"([a-zA-Z0-9]+\[\d+\.\d+\.\d+\])";
        public static readonly string PatternAnyVectorType = @"(" + PatternVector + @"|" + PatternStepVector + @")";
        public static readonly string PatternAnyVariableType = @"(" + PatternVariable + @"|" + PatternVector + @"|" + PatternStepVector + @")";
        public static readonly string PatternConstant = @"((\'[hH][a-fA-F\d]+)|(\'[dD]\d+)|(\'[bB][0-1]+))";
    }
}
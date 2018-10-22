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
			RUN
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

        public static TabControl tabControl = null;

        public static string Theme = "dark";

        public static float FontSize = 12;
    }
}

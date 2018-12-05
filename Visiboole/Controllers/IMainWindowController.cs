using System.Collections.Generic;
using VisiBoole.Views;

namespace VisiBoole.Controllers
{
	/// <summary>
	/// Exposes methods on the controller for the MainWindow
	/// </summary>
	public interface IMainWindowController
	{
        /// <summary>
        /// Gets the display of the main window.
        /// </summary>
        /// <returns>The display</returns>
        IDisplay GetDisplay();

        /// <summary>
        /// Set theme of SubDesigns
        /// </summary>
        void SetTheme();

        /// <summary>
        /// Update font sizes
        /// </summary>
        void SetFontSize();

        /// <summary>
		/// Processes a new file that is created or opened by the user
		/// </summary>
		/// <param name="path">The path of the file that was created or opened by the user</param>
		/// <param name="overwriteExisting">True if the file at the given path should be overwritten</param>
		void ProcessNewFile(string path, bool overwriteExisting = false);

        /// <summary>
        /// Loads into the MainWindow the display of the given type
        /// </summary>
        /// <param name="dType">The type of display that should be loaded</param>
        void LoadDisplay(Globals.DisplayType dType);

        /// <summary>
        /// Switch display mode
        /// </summary>
        void SwitchDisplay();

        /// <summary>
		/// Selects the tabpage in the tabcontrol with name matching the given string
		/// </summary>
		/// <param name="fileName">The name of the tabpage to select</param>
		void SelectTabPage(string fileName);

        /// <summary>
        /// Saves the file that is currently active in the selected tabpage
        /// </summary>
        void SaveFile();

		/// <summary>
		/// Saves the file that is currently active in the selected tabpage with the filename chosen by the user
		/// </summary>
		/// <param name="path">The new file path to save the active file to</param>
		void SaveFileAs(string filePath);

        /// <summary>
		/// Saves all files opened
		/// </summary>
		void SaveAll();

        /// <summary>
        /// Run mode.
        /// </summary>
        void Run();

        /// <summary>
        /// Gets variable debugger information for the active SubDesign
        /// </summary>
        /// <returns>Variable debugger information</returns>
        string DebugVariables();

        /// <summary>
        /// Closes the selected open file
        /// </summary>
        /// <returns>The name of the file closed</returns>
        string CloseFile();

        /// <summary>
        /// Performs a dirty check and confirms application exit with the user
        /// </summary>
        /// <returns>Indicates whether the user wants to close</returns>
        bool ExitApplication();
    }
}
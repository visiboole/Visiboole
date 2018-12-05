using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiBoole.Models;

namespace VisiBoole.Controllers
{
    public interface IDesignController
    {
        /// <summary>
		/// Saves the handle to the controller for the MainWindow
		/// </summary>
		void AttachMainWindowController(IMainWindowController mwController);

        /// <summary>
        /// Creates a SubDesign with the given name.
        /// </summary>
        /// <param name="path">Name of SubDesign</param>
        /// <returns>The SubDesign created</returns>
        SubDesign CreateSubDesign(string name);

        /// <summary>
        /// Closes a given SubDesign.
        /// </summary>
        /// <param name="path">Name of SubDesign</param>
        bool CloseSubDesign(string name);

        /// <summary>
        /// Update the font sizes of all SubDesigns.
        /// </summary>
        void SetSubDesignFontSizes();

        /// <summary>
        /// Set the themes of all SubDesigns
        /// </summary>
        void SetThemes();

        /// <summary>
        /// Checks all SubDesigns for unsaved changes
        /// </summary>
        /// <returns>Indicates whether there are unsaved changes</returns>
        bool CheckUnsavedChanges();

        /// <summary>
        /// Gets variable debugger information for a given SubDesign
        /// </summary>
        /// <param name="name">Name of SubDesign</param>
        /// <returns>Variable debugger information</returns>
        string DebugVariables(string name);
    }
}
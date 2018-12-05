using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisiBoole.Models;

namespace VisiBoole.Controllers
{
    public class DesignController : IDesignController
    {
        /// <summary>
		/// Handle to the controller for the MainWindow
		/// </summary>
		private IMainWindowController mwController;

        /// <summary>
		/// All opened SubDesigns currently loaded by this application
		/// </summary>
        private Dictionary<string, SubDesign> SubDesigns;

        /// <summary>
        /// Constructs design controller
        /// </summary>
        public DesignController()
        {
            SubDesigns = new Dictionary<string, SubDesign>();
        }

        /// <summary>
		/// Saves the handle to the controller for the MainWindow
		/// </summary>
		/// <param name="mwController"></param>
		public void AttachMainWindowController(IMainWindowController mwController)
        {
            this.mwController = mwController;
        }

        /// <summary>
        /// Creates a SubDesign with the given name
        /// </summary>
        /// <param name="path">Name of SubDesign</param>
        /// <returns>The SubDesign created</returns>
        public SubDesign CreateSubDesign(string name)
        {
            try
            {
                SubDesign newSubDesign = new SubDesign(name, mwController.LoadDisplay);
                if (!SubDesigns.ContainsKey(newSubDesign.FileSourceName))
                {
                    SubDesigns.Add(newSubDesign.FileSourceName, newSubDesign);
                }

                return newSubDesign;
            }
            catch (Exception ex)
            {
                Globals.DisplayException(ex);
                return null;
            }
        }

        /// <summary>
        /// Closes a given SubDesign.
        /// </summary>
        /// <param name="name">Name of SubDesign</param>
        /// <returns>Indicates whether the SubDesign was closed</returns>
        public bool CloseSubDesign(string name)
        {
            SubDesign sd;
            SubDesigns.TryGetValue(name, out sd);

            if (sd != null)
            {
                SubDesigns.Remove(name);
                for (int i = 0; i < Globals.tabControl.TabPages.Count; i++)
                    Globals.tabControl.TabPages[i].SubDesign().TabPageIndex = i;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Update the font sizes of all SubDesigns.
        /// </summary>
        public void SetSubDesignFontSizes()
        {
            foreach (SubDesign s in SubDesigns.Values)
            {
                s.SetFontSize();
            }
        }

        /// <summary>
        /// Change the themes of all SubDesigns
        /// </summary>
        public void SetThemes()
        {
            foreach (SubDesign s in SubDesigns.Values)
            {
                s.SetTheme();
            }
        }

        /// <summary>
        /// Checks all SubDesigns for unsaved changes
        /// </summary>
        /// <returns>Indicates whether there are unsaved changes</returns>
        public bool CheckUnsavedChanges()
        {
            foreach (SubDesign s in SubDesigns.Values)
            {
                if (s.isDirty)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets variable debugger information for a given SubDesign
        /// </summary>
        /// <param name="name">Name of SubDesign</param>
        /// <returns>Variable debugger information</returns>
        public string DebugVariables(string name)
        {
            string debugInfo = null;
            SubDesign sd;
            SubDesigns.TryGetValue(name, out sd);

            if (sd != null)
            {
                debugInfo = sd.Database.GetVariableDebugInformation();
            }

            return debugInfo;
        }
    }
}
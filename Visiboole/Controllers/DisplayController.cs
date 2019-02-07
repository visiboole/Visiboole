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
using System.Security.Permissions;
using System.Windows.Forms;
using VisiBoole.Models;
using VisiBoole.ParsingEngine;
using VisiBoole.Views;
using VisiBoole.ParsingEngine.ObjectCode;
using System.Drawing;

namespace VisiBoole.Controllers
{
    /// <summary>
    /// Handles the logic, and communication with other objects for the displays hosted by the MainWindow
    /// </summary>
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class DisplayController : IDisplayController
	{
		/// <summary>
		/// No-split input view that is hosted by the MainWindow
		/// </summary>
		private IDisplay edit;

		/// <summary>
		/// Horizontal-split view that is hosted by the MainWindow
		/// </summary>
		private IDisplay run;

		/// <summary>
		/// No-split view that is hosted by the MainWindow
		/// </summary>
		private Dictionary<Globals.DisplayType, IDisplay> allDisplays;

		/// <summary>
		/// Handle to the controller for the MainWindow
		/// </summary>
		private IMainWindowController mwController;

		/// <summary>
		/// The TabControl that shows the input that is shared amongst the displays that are hosted by the MainWindow
		/// </summary>
		private TabControl tabControl;

		/// <summary>
		/// The WebBrowser that shows the output that is shared amongst the displays that are hosted by the MainWindow
		/// </summary>
		public WebBrowser browser;

		/// <summary>
		/// Handle to the output parser that parses the output that is viewed by the user
		/// </summary>
		private OutputParser parseOut;

		/// <summary>
		/// The display that was hosted by the MainWindow before the current one
		/// </summary>
		public IDisplay PreviousDisplay { get; set; }

		/// <summary>
		/// The display that is currently hosted by the MainWindow
		/// </summary>
		private IDisplay currentDisplay;

		/// <summary>
		/// The display that is currently hosted by the MainWindow
		/// </summary>
		public IDisplay CurrentDisplay
		{
			get
			{
				return currentDisplay;
			}
			set
			{
				value.LoadTabControl(tabControl);
				value.LoadWebBrowser(browser);
				currentDisplay = value;
			}
		}

        /// <summary>
		/// Returns a handle to the display of the matching type
		/// </summary>
		/// <param name="dType">The type of the display to return</param>
		/// <returns>Returns a handle to the display of the matching type</returns>
		public IDisplay GetDisplayOfType(Globals.DisplayType dType)
        {
            switch (dType)
            {
                case Globals.DisplayType.EDIT:
                    return edit;
                case Globals.DisplayType.RUN:
                    return run;
                default: return null;
            }
        }

        /// <summary>
        /// Constructs an instance of DisplayController with a handle to the four displays
        /// </summary>
        /// <param name="single">Handle to the no-split input view hosted by the MainWindow</param>
        /// <param name="horizontal">Handle to the horizontally-split view hosted by the MainWindow</param>
        /// <param name="vertical">Handle to the vertically-split view hosted by the MainWindow</param>
        /// <param name="singleOutput">Handle to the no-split output view hosted by the MainWindow</param>
        public DisplayController(IDisplay edit, IDisplay run)
		{
			tabControl = new TabControl();
			browser = new WebBrowser();
			parseOut = new OutputParser();

            ImageList il = new ImageList();
            il.Images.Add("Close", VisiBoole.Properties.Resources.Close);
            tabControl.ImageList = il;

			this.edit = edit;
			this.run = run;

			allDisplays = new Dictionary<Globals.DisplayType, IDisplay>();
			allDisplays.Add(Globals.DisplayType.EDIT, edit);
			allDisplays.Add(Globals.DisplayType.RUN, run);

			CurrentDisplay = edit;
            Globals.TabControl = tabControl;
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
		/// Creates a new tab on the TabControl
		/// </summary>
		/// <param name="sd">The SubDesign that is displayed in the new tab</param>
		/// <returns>Returns true if a new tab was successfully created</returns>
		public bool CreateNewTab(SubDesign sd)
        {
            TabPage tab = new TabPage(sd.FileSourceName);

            tab.Name = sd.FileSourceName;
            tab.ImageKey = "Close";
            tab.ImageIndex = 0;
            tab.Controls.Add(sd);
            sd.Dock = DockStyle.Fill;

            if (tabControl.TabPages.ContainsKey(sd.FileSourceName))
            {
                int index = tabControl.TabPages.IndexOfKey(sd.FileSourceName);

                tabControl.TabPages.RemoveByKey(sd.FileSourceName);
                tabControl.TabPages.Insert(index, tab);
                sd.TabPageIndex = tabControl.TabPages.IndexOf(tab);
                tabControl.SelectTab(tab);
                return false;
            }
            else
            {
                tabControl.TabPages.Add(tab);
                sd.TabPageIndex = tabControl.TabPages.IndexOf(tab);
                tabControl.SelectTab(tab);
                return true;
            }
        }

        /// <summary>
        /// Saves a particular SubDesign
        /// </summary>
        /// <param name="sd">The SubDesign to save</param>
        /// <returns>Indicates whether the save was successful</returns>
        private bool SaveSubDesign(SubDesign sd)
        {
            if (sd == null)
            {
                return false;
            }
            else
            {
                sd.SaveTextToFile();
                return true;
            }
        }

        /// <summary>
        /// Saves the file that is associated with the currently selected tabpage
        /// </summary>
        /// <returns></returns>
        public bool SaveActiveTab()
		{
            bool saved = SaveSubDesign(tabControl.SelectedTab.SubDesign());
            return saved;
		}

        /// <summary>
		/// Saves the files associated to all tabpages
		/// </summary>
		/// <returns>Indicates whether the files were saved</returns>
        public bool SaveAllTabs()
        {
            foreach (TabPage tab in tabControl.TabPages)
            {
                SubDesign sd = tab.SubDesign();
                bool saved = SaveSubDesign(sd);

                if (!saved) return false;
            }

            return true;
        }

        /// <summary>
		/// Returns the TabPage that is currently selected
		/// </summary>
		/// <returns>Returns the TabPage that is currently selected</returns>
		public TabPage GetActiveTabPage()
        {
            return tabControl.SelectedTab;
        }

        /// <summary>
        /// Selects the tabpage with matching name
        /// </summary>
        /// <param name="fileName">The name of the tabpage to select</param>
        /// <returns>Returns the tabpage that matches the given string</returns>
        public bool SelectTabPage(string fileName)
        {
            if (tabControl.TabPages.IndexOfKey(fileName) != -1)
            {
                tabControl.SelectTab(fileName);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
		/// Handles the event that occurs when the user runs the parser
		/// </summary>
		public void Run()
        {
            SubDesign sd = tabControl.SelectedTab.SubDesign();
            Parser p = new Parser();
            List<IObjectCodeElement> output = p.Parse(sd, null, false);
            if (output == null)
            {
                return;
            }

            HtmlBuilder html = new HtmlBuilder(sd, output);
            if (html.HtmlText == null)
            {
                return;
            }
            string htmlOutput = html.GetHTML();

            browser.ObjectForScripting = this;
            html.DisplayHtml(htmlOutput, browser);

            if (CurrentDisplay is DisplayEdit)
            {
                mwController.LoadDisplay(Globals.DisplayType.RUN);
            }
        }

        /// <summary>
        /// Handles the event that occurs when the user ticks
        /// </summary>
        public void Tick()
        {
            SubDesign sd = tabControl.SelectedTab.SubDesign();
            Parser p = new Parser();
            List<IObjectCodeElement> output = p.Parse(sd, null, true);
            HtmlBuilder html = new HtmlBuilder(sd, output);
            string htmlOutput = html.GetHTML();

            browser.ObjectForScripting = this;
            int position = browser.Document.Body.ScrollTop;
            html.DisplayHtml(htmlOutput, browser);

            browser.DocumentCompleted += (sender, e) => { browser.Document.Body.ScrollTop = position; };

            if (CurrentDisplay is DisplayEdit)
            {
                mwController.LoadDisplay(Globals.DisplayType.RUN);
            }
        }

        /// <summary>
        /// Handles the event that occurs when the user clicks on an independent variable
        /// </summary>
        /// <param name="variableName">The name of the variable that was clicked by the user</param>
        public void Variable_Click(string variableName)
        {
            SubDesign sd = tabControl.SelectedTab.SubDesign();
            Parser p = new Parser();
            List<IObjectCodeElement> output = p.Parse(sd, variableName, false);
            if (output == null)
            {
                return;
            }

            HtmlBuilder html = new HtmlBuilder(sd, output);
            if (html.HtmlText == null)
            {
                return;
            }
            string htmlOutput = html.GetHTML();

            browser.ObjectForScripting = this;
            int position = browser.Document.Body.ScrollTop;
            html.DisplayHtml(htmlOutput, browser);

            browser.DocumentCompleted += (sender, e) => { browser.Document.Body.ScrollTop = position; };

            if (CurrentDisplay is DisplayEdit)
            {
                mwController.LoadDisplay(Globals.DisplayType.RUN);
            }
        }

        /// <summary>
		/// Closes the current tab
		/// </summary>
		/// <returns>Indicates whether the tab was closed</returns>
        public bool CloseActiveTab()
        {
            SubDesign sd = tabControl.SelectedTab.SubDesign();

            TabPage tab = tabControl.SelectedTab;
            if (tab != null)
            {
                if (tabControl.TabPages.Count > 1)
                {
                    if (tabControl.SelectedIndex != 0)
                    {
                        tabControl.SelectedIndex -= 1;
                    }
                    else
                    {
                        tabControl.SelectedIndex += 1;
                    }

                }
                tabControl.TabPages.Remove(tab); // Remove tab page

                return true;
            }
            else return false;
        }
    }
}
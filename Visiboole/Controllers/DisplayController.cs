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
		/// <param name="design">The Design that is displayed in the new tab</param>
		/// <returns>Returns true if a new tab was successfully created</returns>
		public bool CreateNewTab(Design design)
        {
            TabPage tab = new TabPage(design.FileSourceName);

            tab.Name = design.FileSourceName;
            tab.ImageKey = "Close";
            tab.ImageIndex = 0;
            tab.Controls.Add(design);
            design.Dock = DockStyle.Fill;

            if (tabControl.TabPages.ContainsKey(design.FileSourceName))
            {
                int index = tabControl.TabPages.IndexOfKey(design.FileSourceName);

                tabControl.TabPages.RemoveByKey(design.FileSourceName);
                tabControl.TabPages.Insert(index, tab);
                design.TabPageIndex = tabControl.TabPages.IndexOf(tab);
                tabControl.SelectTab(tab);
                return false;
            }
            else
            {
                tabControl.TabPages.Add(tab);
                design.TabPageIndex = tabControl.TabPages.IndexOf(tab);
                tabControl.SelectTab(tab);
                return true;
            }
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
		/// Selects the tab page with the given index.
		/// </summary>
		/// <param name="index">Index of tabpage to select</param>
		public void SelectTabPage(int index)
        {
            if (index != -1)
            {
                tabControl.SelectTab(index);
            }
        }

        /// <summary>
		/// Handles the event that occurs when the user runs the parser
		/// </summary>
		public void Run()
        {
            Design design = tabControl.SelectedTab.Design();
            Parser p = new Parser();
            List<IObjectCodeElement> output = p.Parse(design, null, false);
            if (output == null)
            {
                return;
            }

            HtmlBuilder html = new HtmlBuilder(design, output);
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
        public void Tick(int count)
        {
            Design design = tabControl.SelectedTab.Design();
            browser.ObjectForScripting = this;
            int position = browser.Document.Body.ScrollTop;

            for (int i = 0; i < count; i++)
            {
                Parser p = new Parser();
                List<IObjectCodeElement> output = p.Parse(design, null, true);
                HtmlBuilder html = new HtmlBuilder(design, output);
                string htmlOutput = html.GetHTML();
                html.DisplayHtml(htmlOutput, browser);
            }

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
            Design design = tabControl.SelectedTab.Design();
            Parser p = new Parser();
            List<IObjectCodeElement> output = p.Parse(design, variableName, false);
            if (output == null)
            {
                return;
            }

            HtmlBuilder html = new HtmlBuilder(design, output);
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
        /// Closes a specific tab in the tab control
        /// </summary>
        /// <param name="index">Index to close</param>
        /// <returns>Whether the operation was successful</returns>
        public bool CloseTab(int index)
        {
            TabPage tab = tabControl.TabPages[index];

            if (tab != null)
            {
                if (tabControl.SelectedIndex != 0)
                {
                    tabControl.SelectedIndex -= 1;
                }
                else
                {
                    tabControl.SelectedIndex += 1;
                }

                tabControl.TabPages.Remove(tab); // Remove tab page

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
		/// Closes the current tab
		/// </summary>
		/// <returns>Indicates whether the tab was closed</returns>
        public bool CloseActiveTab()
        {
            Design design = tabControl.SelectedTab.Design();

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
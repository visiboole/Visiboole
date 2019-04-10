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
using System.Threading;
using System;

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
		private IDisplay EditDisplay;

		/// <summary>
		/// Horizontal-split view that is hosted by the MainWindow
		/// </summary>
		private IDisplay RunDisplay;

		/// <summary>
		/// Handle to the controller for the MainWindow
		/// </summary>
		private IMainWindowController MainWindowController;

		/// <summary>
		/// The TabControl that shows the input that is shared amongst the displays that are hosted by the MainWindow
		/// </summary>
		private TabControl TabControl;

		/// <summary>
		/// The WebBrowser that shows the output that is shared amongst the displays that are hosted by the MainWindow
		/// </summary>
		private WebBrowser Browser;

        /// <summary>
        /// Last output of the browser.
        /// </summary>
        private List<IObjectCodeElement> LastOutput;

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
				value.AddTabControl(TabControl);
                string currentDesign = TabControl.SelectedTab != null ? TabControl.SelectedTab.Name.TrimStart('*') : "";
				value.AddBrowser(currentDesign, Browser);
				currentDisplay = value;
			}
		}

        /// <summary>
        /// Constructs an instance of DisplayController with a handle to the two displays.
        /// </summary>
        /// <param name="editDisplay">Handle to the edit display hosted by the MainWindow</param>
        /// <param name="runDisplay">Handle to the run display hosted by the MainWindow</param>
        public DisplayController(IDisplay editDisplay, IDisplay runDisplay)
		{
            // Init tab control
			TabControl = new TabControl();

            TabControl.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            ImageList il = new ImageList();
            il.Images.Add("Close", Properties.Resources.Close);
            TabControl.ImageList = il;

            TabControl.SelectedIndexChanged += (sender, e) => {
                MainWindowController.SelectFile(TabControl.SelectedIndex);
                MainWindowController.LoadDisplay(DisplayType.EDIT);
            };
            TabControl.MouseDown += (sender, e) => {
                if (TabControl.SelectedIndex != -1)
                {
                    Rectangle current = TabControl.GetTabRect(TabControl.SelectedIndex);
                    Rectangle close = new Rectangle(current.Left + 7, current.Top + 4, 12, 12);
                    if (close.Contains(e.Location))
                    {
                        MainWindowController.CloseActiveFile();
                    }
                }
            };
            Globals.TabControl = TabControl;

            // Init browser
            Browser = new WebBrowser();
            Browser.IsWebBrowserContextMenuEnabled = false;
            Browser.AllowWebBrowserDrop = false;
            Browser.WebBrowserShortcutsEnabled = false;

            // Init displays
            EditDisplay = editDisplay;
			RunDisplay = runDisplay;
			CurrentDisplay = editDisplay;
        }

        /// <summary>
        /// Saves the handle to the controller for the MainWindow
        /// </summary>
        /// <param name="mainWindowController"></param>
        public void AttachMainWindowController(IMainWindowController mainWindowController)
		{
			MainWindowController = mainWindowController;
		}

        /// <summary>
		/// Returns a handle to the display of the matching type
		/// </summary>
		/// <param name="dType">The type of the display to return</param>
		/// <returns>Returns a handle to the display of the matching type</returns>
		public IDisplay GetDisplayOfType(DisplayType dType)
        {
            switch (dType)
            {
                case DisplayType.EDIT:
                    return EditDisplay;
                case DisplayType.RUN:
                    return RunDisplay;
                default: return null;
            }
        }

        /// <summary>
		/// Returns the TabPage that is currently selected
		/// </summary>
		/// <returns>Returns the TabPage that is currently selected</returns>
		public TabPage GetActiveTabPage()
        {
            return TabControl.SelectedTab;
        }

        /// <summary>
		/// Selects the tab page with the given index.
		/// </summary>
		/// <param name="index">Index of tabpage to select</param>
        /// <returns>Design name that was selected</returns>
		public string SelectTabPage(int index)
        {
            if (index != -1)
            {
                TabControl.SelectTab(index);
                return TabControl.SelectedTab.Text;
            }
            else
            {
                return "";
            }
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

            if (TabControl.TabPages.ContainsKey(design.FileSourceName))
            {
                int index = TabControl.TabPages.IndexOfKey(design.FileSourceName);

                TabControl.TabPages.RemoveByKey(design.FileSourceName);
                TabControl.TabPages.Insert(index, tab);
                design.TabPageIndex = TabControl.TabPages.IndexOf(tab);
                TabControl.SelectTab(tab);
                return false;
            }
            else
            {
                TabControl.TabPages.Add(tab);
                design.TabPageIndex = TabControl.TabPages.IndexOf(tab);
                TabControl.SelectTab(tab);
                return true;
            }
        }

        /// <summary>
        /// Closes a specific tab in the tab control
        /// </summary>
        /// <param name="index">Index to close</param>
        /// <returns>Whether the operation was successful</returns>
        public bool CloseTab(int index)
        {
            TabPage tab = TabControl.TabPages[index];

            if (tab != null)
            {
                if (TabControl.SelectedIndex != 0)
                {
                    TabControl.SelectedIndex -= 1;
                }
                else
                {
                    TabControl.SelectedIndex += 1;
                }

                TabControl.TabPages.Remove(tab); // Remove tab page

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Displays the provided output to the Browser.
        /// </summary>
        /// <param name="output">Output of the parsed design</param>
        /// <param name="position">Scroll position of the Browser</param>
		public void DisplayOutput(List<IObjectCodeElement> output, int position = 0)
        {
            HtmlBuilder html = new HtmlBuilder(output);
            if (html.HtmlText == null)
            {
                return;
            }
            string htmlOutput = html.GetHTML();

            Browser.ObjectForScripting = this;
            html.DisplayHtml(htmlOutput, Browser);

            Browser.DocumentCompleted += (sender, e) => {
                Browser.Document.Body.ScrollTop = position;
                Browser.Document.Body.Click += (sender2, e2) => { MainWindowController.RetrieveFocus(); };
                MainWindowController.RetrieveFocus();
            };

            if (CurrentDisplay is DisplayEdit)
            {
                MainWindowController.LoadDisplay(DisplayType.RUN);
            }

            LastOutput = output;
        }

        /// <summary>
        /// Handles the event that occurs when the Browser needs to be refreshed.
        /// </summary>
        public void RefreshOutput()
        {
            DisplayOutput(LastOutput);
        }

        /// <summary>
        /// Switches the display to the edit mode.
        /// </summary>
        public void SwitchDisplay()
        {
            MainWindowController.LoadDisplay(DisplayType.EDIT);
        }

        /// <summary>
        /// Handles the event that occurs when the user ticks.
        /// </summary>
        /// <param name="count">Number of times to tick</param>
        public void Tick(int count)
        {
            Browser.ObjectForScripting = this;
            int position = Browser.Document.Body.ScrollTop;

            for (int i = 0; i < count; i++)
            {
                List<IObjectCodeElement> output = MainWindowController.Tick();
                DisplayOutput(output, position);
            }
        }

        /// <summary>
        /// Handles the event that occurs when the user clicks on an independent variable.
        /// </summary>
        /// <param name="variableName">The name of the variable that was clicked by the user</param>
        public void Variable_Click(string variableName)
        {
            Browser.ObjectForScripting = this;
            int position = Browser.Document.Body.ScrollTop;
            DisplayOutput(MainWindowController.Variable_Click(variableName), position);
        }

        /// <summary>
        /// Handles the event that occurs when the user clicks on an instantiation.
        /// </summary>
        /// <param name="instantiation">The instantiation that was clicked by the user</param>
        public void Instantiation_Click(string instantiation)
        {
            List<string> errorLog;
            List<IObjectCodeElement> output = MainWindowController.RunSubdesign(instantiation, out errorLog);
            if (output == null)
            {
                MainWindowController.DisplayErrors(errorLog);
                return;
            }

            HtmlBuilder html = new HtmlBuilder(output);
            if (html.HtmlText == null)
            {
                return;
            }
            string htmlOutput = html.GetHTML();

            WebBrowser subBrowser = new WebBrowser();
            subBrowser.IsWebBrowserContextMenuEnabled = false;
            subBrowser.AllowWebBrowserDrop = false;
            subBrowser.WebBrowserShortcutsEnabled = false;
            subBrowser.ObjectForScripting = this;
            html.DisplayHtml(htmlOutput, subBrowser);

            subBrowser.DocumentCompleted += (sender, e) => {
                subBrowser.Document.Body.Click += (sender2, e2) => { MainWindowController.RetrieveFocus(); };
                MainWindowController.RetrieveFocus();
            };

            CurrentDisplay.AddBrowser(instantiation.Split('.')[0], subBrowser);
        }
    }
}
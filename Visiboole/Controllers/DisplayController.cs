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
using CustomTabControl;

namespace VisiBoole.Controllers
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
        /// HTMLBuilder for the web browser.
        /// </summary>
        private HtmlBuilder HtmlBuilder;

        /// <summary>
        /// Last output of the browser.
        /// </summary>
        private List<IObjectCodeElement> LastOutput;

        private TreeNode InstantiationClicks;

        /// <summary>
        /// The display that was hosted by the MainWindow before the current one
        /// </summary>
        public IDisplay PreviousDisplay { get; set; }

        /// <summary>
        /// The display that is currently hosted by the MainWindow
        /// </summary>
        public IDisplay CurrentDisplay { get; set; }

        /// <summary>
        /// Constructs an instance of DisplayController with a handle to the two displays.
        /// </summary>
        /// <param name="editDisplay">Handle to the edit display hosted by the MainWindow</param>
        /// <param name="runDisplay">Handle to the run display hosted by the MainWindow</param>
        public DisplayController(IDisplay editDisplay, IDisplay runDisplay)
        {
            // Init tab control
            var designTabControl = new NewTabControl();
            designTabControl.Font = new Font("Segoe UI", 10.75F);
            designTabControl.SelectedTabColor = Color.DodgerBlue;
            designTabControl.TabBoundaryColor = Color.Black;
            designTabControl.SelectedTabTextColor = Color.White;
            designTabControl.SelectedIndexChanged += (sender, e) => {
                string fileSelection = designTabControl.SelectedIndex != -1 ? designTabControl.SelectedTab.Text.TrimStart('*') : null;
                MainWindowController.SelectFile(fileSelection);
                MainWindowController.LoadDisplay(DisplayType.EDIT);
            };
            designTabControl.TabClosing += (sender) => {
                if (DesignController.ActiveDesign != null)
                {
                    MainWindowController.CloseActiveFile(false);
                }
            };
            designTabControl.TabSwap += (sender, e) => {
                MainWindowController.SwapDesignNodes(e.SourceTabPageIndex, e.DestinationTabPageIndex);
            };

            var browserTabControl = new NewTabControl();
            browserTabControl.Font = new Font("Segoe UI", 10.75F);
            browserTabControl.SelectedTabColor = Color.DodgerBlue;
            browserTabControl.TabBoundaryColor = Color.Black;
            browserTabControl.SelectedTabTextColor = Color.White;
            browserTabControl.SelectedIndexChanged += (sender, e) => {
                if (browserTabControl.SelectedIndex != -1)
                {
                    MainWindowController.SelectParser(browserTabControl.TabPages[browserTabControl.SelectedIndex].Text);
                }
            };
            browserTabControl.TabClosing += (sender) => {
                MainWindowController.CloseParser(((TabPage)sender).Text);
            };
            browserTabControl.TabClosed += (sender, e) => {
                if (e.TabPagesCount == 0)
                {
                    MainWindowController.LoadDisplay(DisplayType.EDIT);
                }
                foreach (TreeNode treeNode in InstantiationClicks.Nodes)
                {
                    if (treeNode.Text.Split('.')[0] == ((TabPage)sender).Text)
                    {
                        InstantiationClicks.Nodes.Remove(treeNode);
                    }
                }
            };

            InstantiationClicks = new TreeNode();

            // Create html builder
            HtmlBuilder = new HtmlBuilder();

            // Init displays
            EditDisplay = editDisplay;
            EditDisplay.AttachTabControl(designTabControl);

            RunDisplay = runDisplay;
            RunDisplay.AttachTabControl(browserTabControl);

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
        /// Loads into the MainWindow the display of the given type
        /// </summary>
        /// <param name="dType">The type of display that should be loaded</param>
        public void LoadDisplay(DisplayType dType)
        {
            MainWindowController.LoadDisplay(dType);
        }

        /// <summary>
		/// Selects the tab page with the provided name.
		/// </summary>
		/// <param name="name">Name of tabpage to select</param>
		public void SelectTabPage(string name)
        {
            CurrentDisplay.SelectTab(name);
        }

        /// <summary>
		/// Creates a new tab on the design tab control.
		/// </summary>
		/// <param name="design">Design that is to be displayed in a tab</param>
		public void CreateDesignTab(Design design)
        {
            CurrentDisplay.AddTabComponent(design.FileName, design);
        }

        /// <summary>
        /// Closes a specific tab in the design tab control.
        /// </summary>
        /// <param name="designName">Name of the design being closed</param>
        public void CloseDesignTab(string name)
        {
            CurrentDisplay.CloseTab(name);
        }

        /// <summary>
        /// Sets the theme of edit and run tab control
        /// </summary>
        public void SetTheme()
        {
            CurrentDisplay.SetTheme();
        }

        /// <summary>
        /// Displays the provided output to the Browser.
        /// </summary>
        /// <param name="output">Output of the parsed design</param>
        /// <param name="position">Scroll position of the Browser</param>
		public void DisplayOutput(List<IObjectCodeElement> output, int position = 0)
        {
            if (CurrentDisplay is DisplayEdit)
            {
                MainWindowController.LoadDisplay(DisplayType.RUN);
                InstantiationClicks = new TreeNode();
            }
            CurrentDisplay.AddTabComponent(DesignController.ActiveDesign.FileName, HtmlBuilder.GetHTML(output));

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
        /// Handles the event that occurs when the user ticks.
        /// </summary>
        /// <param name="count">Number of times to tick</param>
        public void Tick(int count)
        {
            for (int i = 0; i < count; i++)
            {
                DisplayOutput(MainWindowController.Tick());
            }
        }

        /// <summary>
        /// Handles the event that occurs when the user clicks on an independent variable.
        /// </summary>
        /// <param name="variableName">The name of the variable that was clicked by the user</param>
        /// <param name="value">Value for formatter click</param>
        public void Variable_Click(string variableName, string value = null)
        {
            DisplayOutput(MainWindowController.Variable_Click(variableName, value));
            if (InstantiationClicks.Nodes.Count > 0)
            {
                foreach (TreeNode node in InstantiationClicks.Nodes)
                {
                    Instantiation_Click(node.Text, false);
                }
            }
            MainWindowController.RetrieveFocus();
        }

        /// <summary>
        /// Handles the event that occurs when the user clicks on an instantiation.
        /// </summary>
        /// <param name="instantiation">The instantiation that was clicked by the user</param>
        public void Instantiation_Click(string instantiation, bool addNode = true)
        {
            if (addNode)
            {
                if (InstantiationClicks.Nodes.Count != 0)
                {
                    foreach (TreeNode node in InstantiationClicks.Nodes)
                    {
                        if (node.Text.Split('.')[0] == DesignController.ActiveDesign.FileName)
                        {
                            node.Nodes.Add(instantiation);
                        }
                    }
                }
                else
                {
                    InstantiationClicks.Nodes.Add(instantiation);
                }
            }

            List<IObjectCodeElement> output = MainWindowController.RunSubdesign(instantiation);
            if (output == null)
            {
                return;
            }

            CurrentDisplay.AddTabComponent(instantiation.Split('.')[0], HtmlBuilder.GetHTML(output, true));
        }
    }
}
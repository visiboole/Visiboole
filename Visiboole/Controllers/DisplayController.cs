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
using System.Linq;

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
        private string LastOutput;

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
            designTabControl.TabSwapped += (sender, e) => {
                MainWindowController.SwapDesignNodes(e.SourceTabPageIndex, e.DestinationTabPageIndex);
            };

            var browserTabControl = new NewTabControl();
            browserTabControl.Font = new Font("Segoe UI", 10.75F);
            browserTabControl.SelectedTabColor = Color.DodgerBlue;
            browserTabControl.TabBoundaryColor = Color.Black;
            browserTabControl.SelectedTabTextColor = Color.White;

            // Create html builder
            HtmlBuilder = new HtmlBuilder();

            // Init edit display
            EditDisplay = editDisplay;
            EditDisplay.AttachTabControl(designTabControl);
            EditDisplay.DisplayTabChanged += (tabName) => {
                // Select the tab associated with the tab
                MainWindowController.SelectFile(tabName);
                if (tabName != null)
                {
                    MainWindowController.LoadDisplay(DisplayType.EDIT);
                }
            };
            EditDisplay.DisplayTabClosing += (tabName) => {
                // Close file associated with the tab
                MainWindowController.CloseFile(tabName, true);
            };

            // Init run display
            RunDisplay = runDisplay;
            RunDisplay.AttachTabControl(browserTabControl);
            RunDisplay.DisplayTabChanged += (tabName) => {
                if (tabName != null)
                {
                    // Select the tab associated with the tab
                    MainWindowController.SelectFile(tabName);
                }
            };
            RunDisplay.DisplayTabClosed += (tabName, tabCount) => {
                // If the tab is an instantiation parser
                if (tabName.Contains("."))
                {
                    // Close parser associated with the tab
                    MainWindowController.CloseInstantiationParser(tabName);
                    var treeNodes = Collect(InstantiationClicks.Nodes).ToList();
                    // For each 
                    foreach (TreeNode treeNode in treeNodes)
                    {
                        if (treeNode.Text == tabName)
                        {
                            treeNode.Parent.Nodes.Remove(treeNode);
                            break;
                        }
                    }
                }
                else
                {
                    if (tabCount > 0)
                    {
                        MainWindowController.SuspendRunDisplay();
                    }

                    // Select the tab associated with the tab
                    MainWindowController.SelectFile(tabName);
                }

                if (tabCount == 0)
                {
                    if (CurrentDisplay is DisplayRun)
                    {
                        MainWindowController.LoadDisplay(DisplayType.EDIT);
                    }
                }
            };

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

        public void RetrieveFocus()
        {
            MainWindowController.RetrieveFocus();
        }

        /// <summary>
		/// Selects the specified design tab in the design tab control.
		/// </summary>
		/// <param name="name">Name of tabpage to select</param>
		public void SelectDesignTab(string name)
        {
            CurrentDisplay.SelectTab(name);
        }

        /// <summary>
		/// Creates a new design tab on the design tab control.
		/// </summary>
		/// <param name="design">Design that is to be displayed in a tab</param>
		public void CreateDesignTab(Design design)
        {
            CurrentDisplay.AddTabComponent(design.FileName, design);
        }

        /// <summary>
        /// Closes the specified design tab in the design tab control.
        /// </summary>
        /// <param name="name">Name of design tab to close</param>
        public void CloseDesignTab(string name)
        {
            CurrentDisplay.CloseTab(name);
        }

        /// <summary>
        /// Closes all parser tabs.
        /// </summary>
        public void CloseParserTabs()
        {
            CurrentDisplay.CloseTabs();
        }

        /// <summary>
        /// Sets the theme of edit and run tab control
        /// </summary>
        public void SetTheme()
        {
            CurrentDisplay.SetTheme();
        }

        /// <summary>
        /// Displays the provided html in the current display.
        /// </summary>
        /// <param name="html">HTML to display</param>
        private void DisplayHTML(string html)
        {
            if (CurrentDisplay is DisplayEdit)
            {
                MainWindowController.LoadDisplay(DisplayType.RUN);
                InstantiationClicks = new TreeNode(DesignController.ActiveDesign.FileName);
                InstantiationClicks.Name = DesignController.ActiveDesign.FileName;
            }

            // Add html to the current display
            CurrentDisplay.AddTabComponent(DesignController.ActiveDesign.FileName, html);
            // Save html for last output
            LastOutput = html;
        }

        /// <summary>
        /// Displays the provided output to the Browser.
        /// </summary>
        /// <param name="output">Output of the parsed design</param>
        /// <param name="position">Scroll position of the Browser</param>
		public void DisplayOutput(List<IObjectCodeElement> output)
        {
            // Display the built html
            DisplayHTML(HtmlBuilder.GetHTML(output));
        }

        /// <summary>
        /// Handles the event that occurs when the Browser needs to be refreshed.
        /// </summary>
        public void RefreshOutput()
        {
            DisplayHTML(LastOutput);
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

        private IEnumerable<TreeNode> Collect(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                yield return node;

                foreach (var child in Collect(node.Nodes))
                    yield return child;
            }
        }

        /// <summary>
        /// Handles the event that occurs when the user clicks on an independent variable.
        /// </summary>
        /// <param name="variableName">The name of the variable that was clicked by the user</param>
        /// <param name="value">Value for formatter click</param>
        public void Variable_Click(string variableName, string value = null)
        {
            var currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            //var currentDesignName = DesignController.ActiveDesign.FileName;
            DisplayOutput(MainWindowController.Variable_Click(variableName, value));
            // If an instantiation is opened
            if (InstantiationClicks.Nodes.Count > 0)
            {
                // For each instantiation open
                foreach (TreeNode node in Collect(InstantiationClicks.Nodes))
                {
                    MainWindowController.SelectFile(node.Parent.Name);
                    // Run instantiation for new values
                    Instantiation_Click(node.Text, false);
                }
            }

            MainWindowController.RetrieveFocus();
            Cursor.Current = currentCursor;
        }

        /// <summary>
        /// Handles the event that occurs when the user clicks on an instantiation.
        /// </summary>
        /// <param name="instantiation">The instantiation that was clicked by the user</param>
        public void Instantiation_Click(string instantiation, bool addNode = true)
        {
            var currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            if (addNode)
            {
                string name = instantiation.TrimEnd('(');
                if (DesignController.ActiveDesign.FileName == InstantiationClicks.Text)
                {
                    if (!InstantiationClicks.Nodes.ContainsKey(name))
                    {
                        var newNode = new TreeNode(name);
                        newNode.Name = name;
                        InstantiationClicks.Nodes.Add(newNode);
                    }
                }
                else
                {
                    foreach (TreeNode node in Collect(InstantiationClicks.Nodes))
                    {
                        if (node.Text.Split('.')[0] == DesignController.ActiveDesign.FileName)
                        {
                            var newNode = new TreeNode(name);
                            newNode.Name = name;
                            node.Nodes.Add(newNode);
                            break;
                        }
                    }
                }
            }

            string instantName = instantiation.Split('.').Last().TrimEnd('(');
            var output = MainWindowController.RunSubdesign(instantName);
            if (output == null)
            {
                return;
            }

            string html = HtmlBuilder.GetHTML(output, true);
            CurrentDisplay.AddTabComponent(string.Concat(string.Concat(DesignController.ActiveDesign.FileName, '.', instantName)), html, addNode);
            if (addNode)
            {
                LastOutput = html;
            }

            Cursor.Current = currentCursor;
        }
    }
}
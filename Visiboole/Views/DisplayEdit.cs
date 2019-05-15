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

using CustomTabControl;
using System;
using System.Drawing;
using System.Windows.Forms;
using VisiBoole.Controllers;
using VisiBoole.Models;

namespace VisiBoole.Views
{
    /// <summary>
    /// The no-split input display that is hosted by the MainWindow
    /// </summary>
    public partial class DisplayEdit : UserControl, IDisplay
    {
        /// <summary>
        /// Event that occurs when the display tab is changed.
        /// </summary>
        public event DisplayTabChangeEventHandler DisplayTabChanged;

        /// <summary>
        /// Event that occurs when the display tab is closing.
        /// </summary>
        public event DisplayTabClosingEventHandler DisplayTabClosing;

        /// <summary>
        /// Event that occurs when the display tab is closed.
        /// </summary>
        public event DisplayTabClosedEventHandler DisplayTabClosed;

        /// <summary>
        /// Controller for this display.
        /// </summary>
        private IDisplayController Controller;

        /// <summary>
        /// Tab control for this display.
        /// </summary>
        private NewTabControl TabControl;

        /// <summary>
        /// Type of this display.
        /// </summary>
        public DisplayType DisplayType { get { return DisplayType.EDIT; } }

        /// <summary>
        /// Constucts an instance of DisplaySingleOutput
        /// </summary>
        public DisplayEdit()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves the handle to the controller for this display
        /// </summary>
        /// <param name="controller">The handle to the controller to save</param>
        public void AttachController(IDisplayController controller)
        {
            Controller = controller;
        }

        /// <summary>
		/// Loads the given tab control into this display.
		/// </summary>
		/// <param name="tabControl">The tabcontrol that will be loaded by this display</param>
		public void AttachTabControl(NewTabControl tabControl)
        {
            TabControl = tabControl;
            tabControl.SelectedIndexChanged += (sender, eventArgs) => {
                string tabName = tabControl.SelectedIndex != -1 ? tabControl.TabPages[TabControl.SelectedIndex].Text.TrimStart('*') : null;
                DisplayTabChanged?.Invoke(tabName);
            };
            tabControl.TabXClicked += (sender, eventArgs) => {
                DisplayTabClosing?.Invoke(((TabPage)sender).Text.TrimStart('*'));
            };
            pnlMain.Controls.Add(TabControl, 0, 0);
            TabControl.Dock = DockStyle.Fill;
        }

        private TabPage FindTab(string name)
        {
            foreach (TabPage tabPage in TabControl.TabPages)
            {
                if (tabPage.Text.TrimStart('*') == name)
                {
                    return tabPage;
                }
            }

            return null;
        }

        /// <summary>
        /// Selects the tab with the provided name if present.
        /// </summary>
        /// <param name="name">Name of tab to select</param>
        public void SelectTab(string name)
        {
            for (int i = 0; i < TabControl.TabPages.Count; i++)
            {
                TabPage tabPage = TabControl.TabPages[i];
                if (tabPage.Text.TrimStart('*') == name)
                {
                    TabControl.SelectTab(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Closes the tab with the provided name if present.
        /// </summary>
        /// <param name="name"></param>
        public void CloseTab(string name)
        {
            for (int i = 0; i < TabControl.TabPages.Count; i++)
            {
                TabPage tabPage = TabControl.TabPages[i];
                if (tabPage.Text.TrimStart('*') == name)
                {
                    var closingTabPage = TabControl.TabPages[i];
                    TabControl.SelectedIndex = -1;
                    TabControl.TabPages.Remove(closingTabPage);
                    if (TabControl.TabCount > 0)
                    {
                        if (TabControl.TabCount > i)
                        {
                            TabControl.SelectedIndex = i;
                        }
                        else
                        {
                            TabControl.SelectedIndex = i - 1;
                        }
                    }
                    DisplayTabClosed?.Invoke(closingTabPage.Text, TabControl.TabCount);
                    break;
                }
            }
        }

        /// <summary>
        /// Closes all tabs.
        /// </summary>
        public void CloseTabs()
        {
            for (int i = 0; i < TabControl.TabCount; i++)
            {
                var closingTabPage = TabControl.TabPages[0];
                TabControl.TabPages.RemoveAt(0);
                DisplayTabClosed?.Invoke(closingTabPage.Text, TabControl.TabCount);
            }
        }

        /// <summary>
        /// Sets the theme of edit and run tab control
        /// </summary>
        public void SetTheme()
        {
            TabControl.BackgroundColor = Properties.Settings.Default.Theme == "Light" ? Color.AliceBlue : Color.FromArgb(66, 66, 66);
            TabControl.TabColor = Properties.Settings.Default.Theme == "Light" ? Color.White : Color.FromArgb(66, 66, 66);
            TabControl.TabTextColor = Properties.Settings.Default.Theme == "Light" ? Color.Black : Color.White;
            TabControl.Refresh();
        }

        /// <summary>
        /// Adds/updates a tab page with the provided name and the provided component.
        /// </summary>
        /// <param name="name">Name of the tab page to add or update</param>
        /// <param name="component">Component to add or update</param>
        /// <param name="swap">Whether to swap to the new component</param>
        public void AddTabComponent(string name, object component, bool swap = false)
        {
            var design = (Design)component;

            TabPage existingTabPage = FindTab(name);
            if (existingTabPage == null)
            {
                TabPage newTabPage = new TabPage(name);
                newTabPage.Text = name;
                newTabPage.ToolTipText = $"{name}.vbi";
                newTabPage.Controls.Add(design);

                design.Dock = DockStyle.Fill;
                design.DesignEdit += (designName, isDirty) => {
                    TabPage tabPage = FindTab(designName);
                    tabPage.Text = isDirty ? $"*{designName}" : designName;
                    Controller.LoadDisplay(DisplayType.EDIT);
                };

                TabControl.TabPages.Add(newTabPage);
                if (swap)
                {
                    TabControl.SelectedTab = newTabPage;
                }
            }
            else
            {
                existingTabPage.Controls.Clear();
                existingTabPage.Controls.Add(design);
                design.Dock = DockStyle.Fill;
                if (swap)
                {
                    TabControl.SelectedTab = existingTabPage;
                }
            }

            pnlMain.Focus();
        }
    }
}
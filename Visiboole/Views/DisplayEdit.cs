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

using System;
using System.Windows.Forms;
using VisiBoole.Controllers;

namespace VisiBoole.Views
{
	/// <summary>
	/// The no-split input display that is hosted by the MainWindow
	/// </summary>
	public partial class DisplayEdit : UserControl, IDisplay
	{
		/// <summary>
		/// Handle to the controller for this display
		/// </summary>
		private IDisplayController Controller;

		/// <summary>
		/// Returns the type of this display
		/// </summary>
		public Globals.DisplayType TypeOfDisplay
		{
			get
			{
				return Globals.DisplayType.EDIT;
			}
		}

		/// <summary>
		/// Constructs an instance of DisplaySingle
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
			this.Controller = controller;
		}

		/// <summary>
		/// Loads the given tabcontrol into this display
		/// </summary>
		/// <param name="tc">The tabcontrol that will be loaded by this display</param>
		public void LoadTabControl(TabControl tc)
		{
			if (!(tc == null))
			{
				this.pnlMain.Controls.Add(tc, 0, 0);
				tc.Dock = DockStyle.Fill;
			}
		}

		/// <summary>
		/// Loads the given web browser into this display
		/// </summary>
		/// <param name="browser">The browser that will be loaded by this display</param>
		public void LoadWebBrowser(WebBrowser browser)
		{

		}
	}
}
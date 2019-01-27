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
using VisiBoole.Views;

namespace VisiBoole
{
	/// <summary>
	/// The entry-point of this application
	/// </summary>
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

            DesignController designController = new DesignController();
            DisplayEdit editDisplay = new DisplayEdit();
			DisplayRun runDisplay = new DisplayRun();
			DisplayController displayController = new DisplayController(editDisplay, runDisplay);

            editDisplay.AttachController(displayController);
			runDisplay.AttachController(displayController);

			MainWindow mainWindow = new MainWindow();
			MainWindowController mainWindowController = new MainWindowController(mainWindow, displayController, designController);

            displayController.AttachMainWindowController(mainWindowController);
            designController.AttachMainWindowController(mainWindowController);

            Application.Run(mainWindow);
		}
	}
}
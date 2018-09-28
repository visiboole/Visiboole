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

			DisplayEdit edit = new DisplayEdit();
			DisplayRun run = new DisplayRun();
			DisplayController dc = new DisplayController(edit, run);

            edit.AttachController(dc);
			run.AttachController(dc);

			MainWindow mw = new MainWindow();
			MainWindowController mwc = new MainWindowController(mw, dc);

			dc.AttachMainWindowController(mwc);
			
			Application.Run(mw);
		}
	}
}

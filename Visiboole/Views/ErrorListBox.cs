using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisiBoole.Views
{
    /// <summary>
    /// Class for error prompts.
    /// </summary>
    public partial class ErrorListBox : Form
    {
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private ErrorListBox()
        {
            InitializeComponent();
            uxPanelTop.MouseDown += new MouseEventHandler(ErrorListBoxMouseDown);
            uxLabelTitle.MouseDown += new MouseEventHandler(ErrorListBoxMouseDown);
        }

        /// <summary>
        /// Handles the event when the user clicks down in the top panel to allow dragging.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorListBoxMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        /// <summary>
        /// Handles the event when the dialog box is painted to add a border.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorListBoxPaint(object sender, PaintEventArgs e)
        {
            Color color = Properties.Settings.Default.Theme.Equals("Light") ? Color.DodgerBlue : Color.FromArgb(66, 66, 66);
            Pen borderPen = new Pen(color, 4);
            e.Graphics.DrawRectangle(borderPen, DisplayRectangle);
            uxPanelTop.BackColor = Properties.Settings.Default.Theme.Equals("Light") ? Color.DodgerBlue : Color.FromArgb(66, 66, 66);

            borderPen.Dispose();
            e.Graphics.Dispose();
        }

        /// <summary>
        /// Displays the provided error log to the user.
        /// </summary>
        /// <param name="log">Log to display</param>
        public static void Display(List<string> log)
        {
            var errorBoxDialog = new ErrorListBox();
            errorBoxDialog.uxRichTextBoxLog.Text = String.Join("\n", log);
            errorBoxDialog.ShowDialog(); // Show form
        }
    }
}
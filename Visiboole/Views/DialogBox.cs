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
    public enum DialogType
    {
        YesNo, Ok
    }

    /// <summary>
    /// Class for user prompts.
    /// </summary>
    public partial class DialogBox : Form
    {
        public DialogBox()
        {
            InitializeComponent();
            this.uxPanelTop.MouseDown += new MouseEventHandler(DialogBoxMouseDown);
            this.uxLabelTitle.MouseDown += new MouseEventHandler(DialogBoxMouseDown);
        }

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        /// <summary>
        /// Handles the event when the user clicks down in the top panel to allow dragging.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DialogBoxMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }


        /// <summary>
        /// Constructs a new dialog box with the provided title, message and appropriate buttons.
        /// </summary>
        /// <param name="title">Title of dialog box</param>
        /// <param name="message">Message of dialog box</param>
        /// <param name="type">Type of dialog box</param>
        /// <returns></returns>
        public DialogResult New(string title, string message, DialogType type)
        {
            this.uxPanelTop.BackColor = Properties.Settings.Default.Theme.Equals("Light") ? Color.DodgerBlue : Color.FromArgb(66, 66, 66);
            this.uxLabelTitle.Text = title;
            this.uxLabelMessage.Text = message;
            this.uxButton1.Select();

            if (type == DialogType.YesNo)
            {
                uxButton1.Text = "Yes";
                uxButton1.DialogResult = DialogResult.Yes;
                uxButton2.Visible = true;
                uxButton2.Text = "No";
                uxButton2.DialogResult = DialogResult.No;
                uxButtonExit.DialogResult = DialogResult.No;
            }
            else
            {
                uxButton1.Text = "OK";
                uxButton1.DialogResult = DialogResult.OK;
                uxButton2.Visible = false;
                uxButtonExit.DialogResult = DialogResult.OK;
            }

            return this.ShowDialog();
        }

        /// <summary>
        /// Handles the event when the dialog box is painted to add a border.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DialogBoxPaint(object sender, PaintEventArgs e)
        {
            Color color = Properties.Settings.Default.Theme.Equals("Light") ? Color.DodgerBlue : Color.FromArgb(66, 66, 66);
            e.Graphics.DrawRectangle(new Pen(color, 4), this.DisplayRectangle);
        }
    }
}
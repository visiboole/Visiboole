using System;
using System.Drawing;
using System.Windows.Forms;

namespace VisiBoole.Views
{
    public enum DialogType
    {
        YesNoCancel, YesNo, Ok
    }

    /// <summary>
    /// Class for user prompts.
    /// </summary>
    public partial class DialogBox : Form
    {
        private DialogBox()
        {
            InitializeComponent();
            uxPanelTop.MouseDown += new MouseEventHandler(DialogBoxMouseDown);
            uxLabelTitle.MouseDown += new MouseEventHandler(DialogBoxMouseDown);
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
        public static DialogResult New(string title, string message, DialogType type)
        {
            var dialogBox = new DialogBox();

            if (message.Length < 90)
            {
                dialogBox.uxLabelMessage.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            else if (message.Length < 180)
            {
                dialogBox.uxLabelMessage.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            else
            {
                dialogBox.uxLabelMessage.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }

            dialogBox.uxLabelTitle.Text = title;
            dialogBox.uxLabelMessage.Text = message;
            dialogBox.uxButton1.Select();

            if (type == DialogType.YesNoCancel)
            {
                dialogBox.uxButton1.Visible = true;
                dialogBox.uxButton1.Text = "Cancel";
                dialogBox.uxButton1.DialogResult = DialogResult.Cancel;
                dialogBox.uxButton2.Visible = true;
                dialogBox.uxButton2.Text = "No";
                dialogBox.uxButton2.DialogResult = DialogResult.No;
                dialogBox.uxButton3.Visible = true;
                dialogBox.uxButton3.Text = "Yes";
                dialogBox.uxButton3.DialogResult = DialogResult.Yes;
                dialogBox.uxButtonExit.DialogResult = DialogResult.Cancel;
            }
            else if (type == DialogType.YesNo)
            {
                dialogBox.uxButton1.Visible = true;
                dialogBox.uxButton1.Text = "No";
                dialogBox.uxButton1.DialogResult = DialogResult.No;
                dialogBox.uxButton2.Visible = true;
                dialogBox.uxButton2.Text = "Yes";
                dialogBox.uxButton2.DialogResult = DialogResult.Yes;
                dialogBox.uxButton3.Visible = false;
                dialogBox.uxButtonExit.DialogResult = DialogResult.No;
            }
            else
            {
                dialogBox.uxButton1.Visible = true;
                dialogBox.uxButton1.Text = "OK";
                dialogBox.uxButton1.DialogResult = DialogResult.OK;
                dialogBox.uxButton2.Visible = false;
                dialogBox.uxButton3.Visible = false;
                dialogBox.uxButtonExit.DialogResult = DialogResult.OK;
            }

            return dialogBox.ShowDialog();
        }

        /// <summary>
        /// Handles the event when the dialog box is painted to add a border.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DialogBoxPaint(object sender, PaintEventArgs e)
        {
            Color color = Properties.Settings.Default.Theme.Equals("Light") ? Color.DodgerBlue : Color.FromArgb(66, 66, 66);
            Pen borderPen = new Pen(color, 4);
            e.Graphics.DrawRectangle(borderPen, DisplayRectangle);
            uxPanelTop.BackColor = Properties.Settings.Default.Theme.Equals("Light") ? Color.DodgerBlue : Color.FromArgb(66, 66, 66);

            borderPen.Dispose();
            e.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisiBoole.Views
{
    public partial class HelpWindow : Form
    {
        public HelpWindow(string name, string text)
        {
            InitializeComponent();

            this.Text = name;
            this.textBox.Text = text;

            if (Globals.Theme.Equals("light"))
            {
                textBox.BackColor = System.Drawing.Color.White;
                this.BackColor = System.Drawing.Color.LightGray;
            }
            else
            {
                this.textBox.ForeColor = System.Drawing.Color.White;
            }
        }
    }
}
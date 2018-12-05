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
    public partial class DebugWindow : Form
    {
        public DebugWindow(string name, string text)
        {
            InitializeComponent();

            this.Text = name;
            richTextBox1.Text = text;

            if (Globals.Theme.Equals("light"))
            {
                richTextBox1.BackColor = System.Drawing.Color.White;
                this.BackColor = System.Drawing.Color.LightGray;
            }
            else
            {
                this.richTextBox1.ForeColor = System.Drawing.Color.White;
            }

            /*
            foreach (string s in text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                if (s.Contains(","))
                {
                    string[] parts = s.Split(',');
                    string output = parts[0]; // Output Variable
                    if (parts.Length == 2)
                    {
                        output += Environment.NewLine;
                        richTextBox1.Text += output; // Add variable to output
                        richTextBox1.SelectedText = output;
                        if (Convert.ToBoolean(parts[1]))
                        {
                            richTextBox1.SelectionColor = System.Drawing.Color.Red;
                        }
                    }
                    else
                    {
                        string expression = String.Concat(parts[2], Environment.NewLine);
                        output += String.Concat(" Expression for Value: ", expression);
                        richTextBox1.Text += output; // Add variable to output
                        richTextBox1.SelectedText = output;
                        if (Convert.ToBoolean(parts[1]))
                        {
                            richTextBox1.SelectionColor = System.Drawing.Color.Red;
                        }
                    }
                }
                else
                {
                    richTextBox1.Text += String.Concat(s, Environment.NewLine);
                }
            }
            */
        }
    }
}
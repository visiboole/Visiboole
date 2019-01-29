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
    /// <summary>
    /// Debug Window for the program
    /// </summary>
    public partial class DebugWindow : Form
    {
        /// <summary>
        /// Constructs a debug window with the given name and containing the given text
        /// </summary>
        /// <param name="name">Name of debug window</param>
        /// <param name="text">Text of debug window</param>
        public DebugWindow(string name, string text)
        {
            InitializeComponent();

            this.Text = name;
            this.richTextBox1.Text = text;

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
                        richTextBox1.SelectedText = output;
                        if (Convert.ToBoolean(parts[1]))
                        {
                            richTextBox1.SelectionColor = System.Drawing.Color.Red;
                        }
                        else
                        {
                            richTextBox1.SelectionColor = System.Drawing.Color.Green;
                        }
                        richTextBox1.AppendText(output);
                        richTextBox1.AppendText(Environment.NewLine);
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
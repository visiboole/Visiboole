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
    /// Help window for the program
    /// </summary>
    public partial class HelpWindow : Form
    {
        /// <summary>
        /// Constructs a help window with the given name and text
        /// </summary>
        /// <param name="name">Name of help window</param>
        /// <param name="text">Text of help window</param>
        public HelpWindow(string name, string text)
        {
            InitializeComponent();

            this.Text = name;
            this.textBox.Text = text;

            if (Properties.Settings.Default.Theme.Equals("Light"))
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
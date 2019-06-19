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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisiBoole.Views
{
    public partial class DocumentWindow : Form
    {
        public DocumentWindow(string name)
        {
            InitializeComponent();

            if (name == "Syntax")
            {
                Browser.Url = new System.Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Help Documentation", "Syntax.html"), System.UriKind.Absolute);
                Text = "Syntax Guide";
            }
            else if (name == "User")
            {
                Browser.Url = new System.Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Help Documentation", "Help.html"), System.UriKind.Absolute);
                Text = "User Guide";
            }
            else if (name == "Introduction")
            {
                Browser.Url = new System.Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Help Documentation", "Introduction.html"), System.UriKind.Absolute);
                Text = "VisiBoole Introduction";
            }
        }
    }
}

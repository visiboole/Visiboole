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
using System.Text.RegularExpressions;
using VisiBoole.Controllers;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// Base class for Visiboole statements.
    /// </summary>
	public abstract class Statement
	{
        /// <summary>
        /// Ttext of the statement.
        /// </summary>
		public string Text { get; set; }

        /// <summary>
        /// List of output elements that comprise this statement.
        /// </summary>
		public List<IObjectCodeElement> Output { get; set; } = new List<IObjectCodeElement>();

        /// <summary>
        /// Constructs a Statement instance.
        /// </summary>
        /// <param name="database">Database of the parsed design</param>
        /// <param name="text">Text of the statement</param>
		protected Statement(string text)
		{
			Text = text;
		}

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
		public virtual void Parse()
        {
            OutputOperator(";");
            Output.Add(new LineFeed());
        }

        /// <summary>
        /// Outputs the provided variable to the output list.
        /// </summary>
        /// <param name="var">Variable to output</param>
        protected void OutputVariable(string var)
        {
            string name = var.TrimStart('~');

            if (!char.IsDigit(name[0]))
            {
                IndependentVariable indVar = DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(name) as IndependentVariable;
                DependentVariable depVar = DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>(name) as DependentVariable;
                if (indVar != null)
                {
                    if (!var.Contains("~"))
                    {
                        Output.Add(indVar);
                    }
                    else
                    {
                        Output.Add(new IndependentVariable(var, indVar.Value));
                    }
                }
                else if (depVar != null)
                {
                    if (!var.Contains("~"))
                    {
                        Output.Add(depVar);
                    }
                    else
                    {
                        Output.Add(new DependentVariable(var, depVar.Value));
                    }
                }
            }
            else
            {
                Output.Add(new Constant(var));
            }
        }

        /// <summary>
        /// Outputs the provided operator to the output list.
        /// </summary>
        /// <param name="text">Text of operator</param>
        protected void OutputOperator(string text)
        {
            Operator op = new Operator(text);
            Output.Add(op);
        }
    }
}
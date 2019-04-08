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

using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Text;
using VisiBoole.Controllers;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// An expression statement that assigns the value of an expression to a dependent.
    /// </summary>
	public class BooleanAssignmentStmt : ExpressionStatement
	{
        /// <summary>
        /// Constructs a BooleanAssignemntStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
        /// <param name="lineNumber">Line number of the expression statement</param>
		public BooleanAssignmentStmt(string text, int lineNumber) : base(text, lineNumber)
        {
            // Update variable value
            Evaluate();

            // Add expression to the database
            DesignController.ActiveDesign.Database.AddExpression(lineNumber, this);
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            base.Parse();
        }
    }
}
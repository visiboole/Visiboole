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
 *
 * You should have received a copy of the GNU General Public License
 * along with this program located at "\Visiboole\license.txt".
 * If not, see <http://www.gnu.org/licenses/>
 */

using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Text;
using VisiBoole.Controllers;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// An expression statement that assigns the value of an expression to a dependent.
    /// </summary>
	public class BooleanAssignmentStmt : Statement
	{
        /// <summary>
        /// Regex for getting output tokens.
        /// </summary>
        private Regex OutputRegex = new Regex($@"(~?{Parser.ConstantPattern})|(~?{Parser.ScalarPattern})|(==)|[\s{{}}()=^|+-]");

        /// <summary>
        /// Expression of the boolean statement.
        /// </summary>
        private NamedExpression Expression;

        /// <summary>
        /// Constructs a BooleanAssignemntStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
		public BooleanAssignmentStmt(string text) : base(text)
        {
            // Create expression with the provided text
            Expression = new NamedExpression(text);

            // Iterate through all dependent variables
            foreach (string dependent in Expression.Dependents)
            {
                // If the dependent isn't in the database
                if (DesignController.ActiveDesign.Database.TryGetVariable<Variable>(dependent) == null)
                {
                    // Add dependent to the database
                    DesignController.ActiveDesign.Database.AddVariable(new DependentVariable(dependent, false));
                }
                // If the dependent is in the database
                else
                {
                    // If the dependent is in the database as an independent variable
                    if (DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(dependent) as IndependentVariable != null)
                    {
                        // Make independent variable a dependent variable
                        DesignController.ActiveDesign.Database.MakeDependent(dependent);
                    }
                }
            }

            // Initialize variables in the expression
            InitVariables(Expression.Expression);

            // Evaluate the expression
            Expression.Evaluate();
            // Add expression to the database
            DesignController.ActiveDesign.Database.AddExpression(Expression);
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            // Output tokens
            MatchCollection matches = OutputRegex.Matches(Text);
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == " ")
                {
                    Output.Add(new SpaceFeed());
                }
                else if (token == "\n")
                {
                    // Output newline
                    Output.Add(new LineFeed());
                }
                else if (token == "(" || token == ")")
                {
                    Output.Add(Expression.Parentheses[match.Index]); // Output the corresponding parenthesis
                }
                else if (Parser.OperatorsList.Contains(token) || token == "=" || token == "{" || token == "}")
                {
                    OutputOperator(token);
                }
                else
                {
                    OutputVariable(token); // Variable or constant
                }
            }

            base.Parse();
        }
    }
}
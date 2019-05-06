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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.Models;
using VisiBoole.Controllers;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// An expression statement that assigns the value of an expression to a dependent on a clock tick.
    /// </summary>
    public class DffClockStmt : Statement
    {
        /// <summary>
        /// Regex for getting output tokens.
        /// </summary>
        private Regex OutputRegex = new Regex($@"(~?{Parser.ConstantPattern})|(~?{Parser.ScalarPattern})|(==)|(<=)|[\s{{}}()@^|+-]");

        /// <summary>
        /// Expression of the clock statement.
        /// </summary>
        private NamedExpression Expression;

        /// <summary>
        /// Driving clock of statement. (if any)
        /// </summary>
        public string Clock;

        /// <summary>
        /// Constructs a DffClockStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
        public DffClockStmt(string text) : base(text)
        {
            // Create expression with the provided text
            Expression = new NamedExpression(text);
            // Get clock of expression
            Clock = text.Contains("@") ? Regex.Match(text, @"(?<=@)\w+").Value : null;
            // Evaluate the expression
            Expression.Evaluate();
            // Add expression to the database
            DesignController.ActiveDesign.Database.AddExpression(Expression, Clock);
        }

        /// <summary>
        /// Ticks the statement (delay value is set to its dependent value)
        /// </summary>
        public IEnumerable<string> Tick()
        {
            if (DesignController.ActiveDesign.Database.SetValues(Expression.Delays, Expression.DependentBinary))
            {
                return Expression.Delays;
            }
            else
            {
                return null;
            }
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
                else if (token == "<=")
                {
                    // Output <= with dependent value
                    if (!Expression.IsMathExpression)
                    {
                        Output.Add(new DependentVariable("<=", Expression.DependentBinary.Contains('1')));
                    }
                    else
                    {
                        OutputOperator(token);
                    }
                }
                else if (Parser.OperatorsList.Contains(token) || token == "{" || token == "}" || token == "@")
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
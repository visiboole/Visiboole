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
    public class ClockAssignmentStmt : Statement
    {
        /// <summary>
        /// Regex for getting output tokens.
        /// </summary>
        private Regex OutputRegex = new Regex($@"(~?{Parser.ScalarPattern})|~?[01]|(==)|(<=)|[\s{{}}()@^|+-]");

        /// <summary>
        /// Expression of the clock statement.
        /// </summary>
        private NamedExpression Expression;

        /// <summary>
        /// Next value of the clock statement.
        /// </summary>
        private string NextValue;

        /// <summary>
        /// Driving clock of statement. (if any)
        /// </summary>
        public string Clock;

        /// <summary>
        /// Constructs a DffClockStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
        public ClockAssignmentStmt(string text) : base(text)
        {
            // Create expression with the provided text
            Expression = new NamedExpression(text);
            // Get clock of expression
            Clock = text.Contains("@") ? Regex.Match(text, @"(?<=@)\w+").Value : null;
            // Add expression to the database
            DesignController.ActiveDesign.Database.AddExpression(Expression, Clock);
        }

        /// <summary>
        /// Updates the next value for the clock statement.
        /// </summary>
        public void Compute()
        {
            NextValue = Expression.DependentBinary;
        }

        /// <summary>
        /// Ticks the statement (delay value is set to its dependent value)
        /// </summary>
        public void Tick()
        {
            DesignController.ActiveDesign.Database.SetValues(Expression.Delays, NextValue);
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override List<IObjectCodeElement> Parse()
        {
            // Create output list to return
            var output = new List<IObjectCodeElement>();
            MatchCollection matches = OutputRegex.Matches(Text);
            foreach (Match match in matches)
            {
                string token = match.Value;

                if (token == " ")
                {
                    output.Add(new SpaceFeed());
                }
                else if (token == "\n")
                {
                    output.Add(new LineFeed());
                }
                else if (token == "(" || token == ")")
                {
                    output.Add(Expression.Parentheses[match.Index]); // Output the corresponding parenthesis
                }
                else if (token[0] == '<')
                {
                    if (Expression.IsMathExpression)
                    {
                        output.Add(new Operator(token));
                    }
                    else
                    {
                        output.Add(new DependentVariable("<=", NextValue.Contains('1')));
                    }
                }
                else if (Parser.OperatorsList.Contains(token) || token == "{" || token == "}" || token == "@")
                {
                    output.Add(new Operator(token));
                }
                else
                {
                    string name = token.TrimStart('~');
                    if (!char.IsDigit(name[0]))
                    {
                        IndependentVariable indVar = DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(name) as IndependentVariable;
                        DependentVariable depVar = DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>(name) as DependentVariable;
                        if (indVar != null)
                        {
                            if (token[0] != '~')
                            {
                                output.Add(indVar);
                            }
                            else
                            {
                                output.Add(new IndependentVariable(token, !indVar.Value));
                            }
                        }
                        else if (depVar != null)
                        {
                            if (token[0] != '~')
                            {
                                output.Add(depVar);
                            }
                            else
                            {
                                output.Add(new DependentVariable(token, !depVar.Value));
                            }
                        }
                    }
                    else
                    {
                        output.Add(new Constant(token));
                    }
                }
            }

            // Output ending semicolon
            output.Add(new Operator(";"));
            // Output new line
            output.Add(new LineFeed());
            // Return output list
            return output;
        }
    }
}
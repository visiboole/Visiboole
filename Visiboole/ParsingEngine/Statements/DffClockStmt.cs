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
        /// Expression of the clock statement.
        /// </summary>
        private NamedExpression Expression;

        /// <summary>
        /// Alternate clock of statement. (if any)
        /// </summary>
        public string AltClock;

        /// <summary>
        /// Constructs a DffClockStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
        public DffClockStmt(string text) : base(text)
        {
            // Create expression with the provided text
            Expression = new NamedExpression(text);
            // If expression contains an alternate clock
            if (Expression.Operation.Contains("@"))
            {
                // Get alternate clock
                AltClock = Expression.Operation.Substring(Expression.Operation.IndexOf("@") + 1);
            }
            else
            {
                // Set alternate clock to null
                AltClock = null;
            }

            // Initialize delay variable
            InitVariables(Expression.Delay);

            // Iterate through all dependent variables
            for (int i = 0; i < Expression.Dependents.Length; i++)
            {
                // Get dependent
                string dependent = Expression.Dependents[i];
                // If the dependent isn't in the database
                if (DesignController.ActiveDesign.Database.TryGetVariable<Variable>(dependent) == null)
                {
                    // Add dependent to the database
                    DesignController.ActiveDesign.Database.AddVariable(new DependentVariable(dependent, false));
                }
            }

            // If there is an alternate clock
            if (AltClock != null)
            {
                // Initialize alternate clock variable
                InitVariables(AltClock);
            }

            // Initialize variables in the expression
            InitVariables(Expression.Expression);

            // Evaluate the expression
            Expression.Evaluate();
            // Add expression to the database
            DesignController.ActiveDesign.Database.AddExpression(Expression);
        }

        /// <summary>
        /// Ticks the statement (delay value is set to its dependent value)
        /// </summary>
        public void Tick()
        {
            int delayValue = Expression.GetValue(Expression.Delay);
            int dependentValue = Expression.GetValue(Expression.Dependent);
            if (delayValue != dependentValue)
            {
                for(int i = 0; i < Expression.Delays.Length; i++)
                {
                    bool val = DesignController.ActiveDesign.Database.TryGetValue(Expression.Dependents[i]) == 1;
                    DesignController.ActiveDesign.Database.SetValue(Expression.Delays[i], val);
                }
            }
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            // Output tokens
            MatchCollection matches = Regex.Matches(Text, $@"(~?{Lexer.ScalarPattern})|(~?{Lexer.ConstantPattern})|([|^(){{}};@+-])|(==)|(<=)|(\s)");
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == " ")
                {
                    Output.Add(new SpaceFeed());
                }
                else if (token == "(" || token == ")")
                {
                    Output.Add(Expression.Parentheses[match.Index]); // Output the corresponding parenthesis
                }
                else if (token == "<=")
                {
                    // Output <= with dependent value
                    Output.Add(new DependentVariable("<=", Expression.GetValue(Expression.Dependent) >= 1));
                }
                else if (Parser.OperatorsList.Contains(token) || token == "{" || token == "}" || token == "@" || token == ";")
                {
                    OutputOperator(token);
                }
                else
                {
                    OutputVariable(token); // Variable or constant
                }
            }

            // Output newline
            Output.Add(new LineFeed());
        }
    }
}
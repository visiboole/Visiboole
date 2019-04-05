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
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.ParsingEngine.Boolean;
using System;
using VisiBoole.Models;
using System.Text;

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
        /// <param name="database">Database of the parsed design</param>
        /// <param name="text">Text of the statement</param>
		public BooleanAssignmentStmt(Database database, string text) : base(database, text)
        { 
            // Add expression and dependency to the database
            Database.AddExpression(Dependent, Expression);
            Database.CreateDependenciesList(Dependent);

            // Update variable value
            Evaluate();
        }

        /// <summary>
        /// Evaluates the expression and assigns the value to the dependent.
        /// </summary>
        public void Evaluate()
        {
            int expressionValue = ExpressionSolver.Solve(Database, Expression);
            int dependentValue = GetValue(Dependent);

            if (dependentValue != expressionValue)
            {
                if (!Dependent.Contains("{"))
                {
                    Database.SetValues(Dependent, expressionValue == 1);
                }
                else
                {
                    string variables = Dependent.Substring(1, Dependent.Length - 2);
                    string[] vars = Regex.Split(variables, @"\s+");
                    if (vars.Length > 1)
                    {
                        vars = vars.Reverse().ToArray(); // a3 a2 a1 a0
                    }
                    string binary = Convert.ToString(expressionValue, 2);
                    if (binary.Length < vars.Length)
                    {
                        binary = binary.PadLeft(vars.Length - binary.Length, '0');
                    }
                    if (binary.Length > 1)
                    {
                        char[] reverseBinary = binary.ToCharArray();
                        Array.Reverse(reverseBinary);
                        binary = new string(reverseBinary); // 0 0 0 1
                    }

                    // Get binary value
                    for (int i = 0; i < vars.Length; i++)
                    {
                        string var = vars[i];
                        int val = int.Parse(binary[i].ToString());
                        Database.SetValues(var, val == 1);
                    }
                }
            }
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            // Output padding (if present)
            int start = Text.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            for (int i = 0; i < start; i++)
            {
                Output.Add(new SpaceFeed());
            }

            // Update and output dependent
            DependentVariable depVar = Database.TryGetVariable<DependentVariable>(Dependent) as DependentVariable;
            Output.Add(depVar);

            // Output equals
            OutputOperator("=");

            // Output expression
            base.Parse();

            // Output newline
            Output.Add(new LineFeed());
        }
    }
}
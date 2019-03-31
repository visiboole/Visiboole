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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Boolean
{
    /// <summary>
    /// Boolean expression object
    /// </summary>
    public static class ExpressionSolver
    {
        /// <summary>
        /// Solves an expression for its value.
        /// </summary>
        /// <param name="expression">Expression to solve</param>
        /// <returns>Value of the expression</returns>
        public static int Solve(string expression)
        {
            Stack<int> valueStack = new Stack<int>();
            Stack<string> operatorStack = new Stack<string>();

            // Obtain scalars, constants and operators
            expression = $"({expression})"; // Add () to expression
            MatchCollection matches = Regex.Matches(expression, @"(~?(?<Name>[_a-zA-Z]\w{0,19}))|(~?'[bB][0-1])|([~^()|+-])|(==)|(?<=\w|\))\s(?=[\w(~'])");
            foreach (Match match in matches)
            {
                if (match.Value == ")")
                {
                    // Perform all operations until (
                    while (operatorStack.Peek() != "(")
                    {
                        ExecuteOperation(ref valueStack, ref operatorStack);
                    }

                    // Pop (
                    operatorStack.Pop();

                    // Check for ~
                    if (operatorStack.Count > 0 && operatorStack.Peek() == "~")
                    {
                        valueStack.Push(Convert.ToInt32(!Convert.ToBoolean(valueStack.Pop())));
                        operatorStack.Pop();
                    }
                }
                else if (match.Value == "(" || Parser.OperatorsList.Contains(match.Value))
                {
                    string operation = match.Value;

                    // Check for operators that need evaluation
                    while (operatorStack.Count > 0 && (operation == "|" && operatorStack.Peek() == " "))
                    {
                        ExecuteOperation(ref valueStack, ref operatorStack);
                    }

                    // Push operation
                    operatorStack.Push(operation);
                }
                else
                {
                    // Process var
                    string var = match.Value;
                    int value = 0;
                    bool containsNot = false;

                    if (var[0] == '~')
                    {
                        containsNot = true;
                        var = var.Substring(1);
                    }
                    
                    if (var.Contains("'"))
                    {
                        value = Convert.ToInt32(var[2].ToString());
                    }
                    else
                    {
                        value = Parser.Design.Database.TryGetValue(var);
                    }

                    if (containsNot)
                    {
                        value = Convert.ToInt32(!Convert.ToBoolean(value));
                    }

                    valueStack.Push(value);
                }
            }

            return valueStack.Pop();
        }

        /// <summary>
        /// Executes an operation.
        /// </summary>
        /// <param name="valueStack"></param>
        /// <param name="operatorStack"></param>
        private static void ExecuteOperation(ref Stack<int> valueStack, ref Stack<string> operatorStack)
        {
            string operation = operatorStack.Pop();

            int rightValue = valueStack.Pop();
            int leftValue = valueStack.Pop();

            int result = 0;
            switch (operation)
            {
                case " ":
                    result = Convert.ToInt32(Convert.ToBoolean(leftValue) && Convert.ToBoolean(rightValue));
                    break;
                case "|":
                    result = Convert.ToInt32(Convert.ToBoolean(leftValue) || Convert.ToBoolean(rightValue));
                    break;
                case "^":
                    result = Convert.ToInt32(Convert.ToBoolean(leftValue) ^ Convert.ToBoolean(rightValue));
                    break;
                case "==":
                    result = Convert.ToInt32(Convert.ToBoolean(leftValue) == Convert.ToBoolean(rightValue));
                    break;
            }

            valueStack.Push(result);
        }

        /// <summary>
        /// Gets the object code output of a provided expression.
        /// </summary>
        /// <param name="expression">Expression to generate output for</param>
        /// <returns>Object code output</returns>
        public static List<IObjectCodeElement> GetOutput(string expression)
        {
            List<IObjectCodeElement> output = new List<IObjectCodeElement>();

            MatchCollection matches = Regex.Matches(expression, @"(~?(?<Name>[_a-zA-Z]\w{0,19}))|(~?'[bB][0-1])|([~^()|+-])|(==)|(?<=\w|\))\s(?=[\w(~'])");
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == "(" || token == ")")
                {
                    output.Add(new Parentheses(token));
                }
                else if (Parser.OperatorsList.Contains(token))
                {
                    output.Add(new Operator(token));
                }
                else if (token.Contains("'"))
                {
                    output.Add(new Constant(token));
                }
                else
                {
                    // Variable
                    string var = token;
                    if (var.Contains("~"))
                    {
                        var = token.Substring(1);
                    }

                    IndependentVariable indVar = Parser.Design.Database.TryGetVariable<IndependentVariable>(var) as IndependentVariable;
                    DependentVariable depVar = Parser.Design.Database.TryGetVariable<DependentVariable>(var) as DependentVariable;
                    if (indVar != null)
                    {
                        output.Add(new IndependentVariable(token, indVar.Value));
                    }
                    else if (depVar != null)
                    {
                        output.Add(new DependentVariable(token, depVar.Value));
                    }
                    else
                    {
                        // Error
                    }
                }
            }

            return output;
        }
    }
}
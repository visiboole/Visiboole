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
    /// Boolean expression solver.
    /// </summary>
    public static class ExpressionSolver
    {
        /// <summary>
        /// Solves an expression for its value.
        /// </summary>
        /// <param name="database">Database of design</param>
        /// <param name="expression">Expression to solve</param>
        /// <returns>Value of the expression</returns>
        public static int Solve(Database database, string expression)
        {
            Stack<int> valueStack = new Stack<int>();
            Stack<string> operatorStack = new Stack<string>();

            // Obtain scalars, constants and operators
            expression = $"({expression})"; // Add () to expression
            MatchCollection matches = Regex.Matches(expression, $@"{Parser.ConcatenationPattern}|(~?(?<Name>[_a-zA-Z]\w{{0,19}}))|(~?'[bB][0-1])|([~^()|+-])|(==)|((?<=[\w)}}])\s+(?=[\w({{~'])(?![^{{}}]*\}}))");
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
                    // Process variable
                    string variable = match.Value;
                    int value = 0;
                    bool containsNot = false;

                    if (variable.Contains("{"))
                    {
                        variable = variable.Substring(1, variable.Length - 2);
                        string[] vars = Regex.Split(variable, @"\s+");

                        // Get binary value
                        StringBuilder binary = new StringBuilder();
                        foreach (string var in vars)
                        {
                            binary.Append(database.TryGetValue(Parser.ScalarRegex1.Match(var).Value));
                        }
                        value = Convert.ToInt32(binary.ToString(), 2);
                    }
                    else
                    {
                        if (variable[0] == '~')
                        {
                            containsNot = true;
                            variable = variable.Substring(1);
                        }

                        if (variable.Contains("'"))
                        {
                            Match constant = Parser.ConstantRegex.Match(variable);

                            // Get binary bits from format type
                            string outputBinary;
                            if (constant.Groups["Format"].Value == "h" || constant.Groups["Format"].Value == "H")
                            {
                                outputBinary = Convert.ToString(Convert.ToInt32(constant.Groups["Value"].Value, 16), 2);
                            }
                            else if (constant.Groups["Format"].Value == "d" || constant.Groups["Format"].Value == "D")
                            {
                                outputBinary = Convert.ToString(Convert.ToInt32(constant.Groups["Value"].Value, 10), 2);
                            }
                            else
                            {
                                outputBinary = constant.Groups["Value"].Value;
                            }
                            value = Convert.ToInt32(outputBinary, 2);
                        }
                        else
                        {
                            value = database.TryGetValue(variable);
                        }

                        if (containsNot)
                        {
                            value = Convert.ToInt32(!Convert.ToBoolean(value));
                        }
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
                case "+":
                    result = leftValue + rightValue;
                    break;
                case "-":
                    result = leftValue - rightValue;
                    break;
            }

            valueStack.Push(result);
        }
    }
}
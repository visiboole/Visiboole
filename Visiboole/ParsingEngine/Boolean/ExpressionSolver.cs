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
        public static bool Solve(string expression)
        {
            Stack<bool> valueStack = new Stack<bool>();
            Stack<string> operatorStack = new Stack<string>();

            // Push start of expression parenthesis
            operatorStack.Push("(");

            // And operator: (?<=\w|\))\s(?=[\w(~'])
            // constant: (\'[bB][0-1])
            MatchCollection matches = Regex.Matches(expression, @"(\~?(?<Name>[_a-zA-Z]\w{0,19}))|([~^()|+-])|(==)|(?<=\w|\))\s(?=[\w(~'])|(\'[bB][0-1])");
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

                    // Check for not in front of (
                    if (operatorStack.Peek() == "~")
                    {
                        operatorStack.Pop();
                        valueStack.Push(!valueStack.Pop());
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
                    bool value = false;

                    if (var[0] == '\'')
                    {
                        value = Convert.ToInt32(match.Value[2].ToString()) == 1;
                    }
                    else
                    {
                        bool containsNot = false;
                        if (var[0] == '~')
                        {
                            containsNot = true;
                            var = var.Substring(1);
                        }

                        value = Globals.TabControl.SelectedTab.Design().Database.TryGetValue(var) == 1;
                        if (containsNot)
                        {
                            value = !value;
                        }
                    }

                    valueStack.Push(value);
                }
            }

            // Perform all operations until (
            while (operatorStack.Peek() != "(")
            {
                ExecuteOperation(ref valueStack, ref operatorStack);
            }

            // Pop (
            operatorStack.Pop();

            return valueStack.Pop();
        }

        /// <summary>
        /// Executes an operation.
        /// </summary>
        /// <param name="valueStack"></param>
        /// <param name="operatorStack"></param>
        private static void ExecuteOperation(ref Stack<bool> valueStack, ref Stack<string> operatorStack)
        {
            string operation = operatorStack.Pop();

            bool rightOperand = valueStack.Pop();
            bool leftOperand = false;
            if (operation != "~")
            {
                leftOperand = valueStack.Pop();
            }

            bool result = false;
            switch (operation)
            {
                case "~":
                    result = !rightOperand;
                    break;
                case " ":
                    result = leftOperand && rightOperand;
                    break;
                case "|":
                    result = leftOperand || rightOperand;
                    break;
                case "^":
                    result = leftOperand ^ rightOperand;
                    break;
                case "==":
                    result = leftOperand == rightOperand;
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
            MatchCollection matches = Regex.Matches(expression, @"(~?[_a-zA-Z]\w{0,19})|('[bB][01])|([~^()|+-])|(==)|(?<=\w|\))\s(?=[\w(~'])");
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
                    string var = token;
                    if (var.Contains("~"))
                    {
                        var = token.Substring(1);
                    }

                    // Variable
                    IndependentVariable indVar = Globals.TabControl.SelectedTab.Design().Database.TryGetVariable<IndependentVariable>(var) as IndependentVariable;
                    DependentVariable depVar = Globals.TabControl.SelectedTab.Design().Database.TryGetVariable<DependentVariable>(var) as DependentVariable;
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
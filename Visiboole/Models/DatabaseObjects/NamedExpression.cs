using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisiBoole.Controllers;
using VisiBoole.ParsingEngine;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.Models
{
    public class NamedExpression
    {
        /// <summary>
        /// Delay of the expression. (If any)
        /// </summary>
        public string Delay { get; private set; }

        /// <summary>
        /// All delays of the expression. (If any)
        /// </summary>
        public string[] Delays { get; private set; }

        /// <summary>
        /// Binary values of the delays.
        /// </summary>
        public string DelayBinary { get; private set; }

        /// <summary>
        /// Dependent of the expression.
        /// </summary>
        public string Dependent { get; private set; }

        /// <summary>
        /// All dependents of the expression.
        /// </summary>
        public string[] Dependents { get; private set; }

        /// <summary>
        /// Binary values of the dependents.
        /// </summary>
        public string DependentBinary { get; private set; }

        /// <summary>
        /// Expression for the dependent.
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        /// Index of the expression.
        /// </summary>
        private int ExpressionIndex;

        /// <summary>
        /// Returns whether the expression is a mathematical expression.
        /// </summary>
        public bool IsMathExpression { get; private set; }

        /// <summary>
        /// Dictionary of parentheses contained in the expression.
        /// </summary>
        public Dictionary<int, Parenthesis> Parentheses { get; private set; }

        /// <summary>
        /// Constructs a NamedExpression with the provided full expression.
        /// </summary>
        /// <param name="fullExpression">Full expression</param>
        public NamedExpression(string fullExpression)
        {
            int startIndex = fullExpression.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            Expression = fullExpression.Substring(startIndex); // Start expression with first non whitespace character

            if (!Expression.Contains("<"))
            {
                int seperatorIndex = Expression.IndexOf('=');
                Delay = null;
                Delays = null;
                Dependent = Expression.Substring(0, seperatorIndex).Trim();
                Dependents = DesignController.ActiveDesign.Database.GetVariables(Dependent);
                Expression = Expression.Substring(seperatorIndex + 1).Trim();
            }
            else
            {
                int seperatorIndex = Expression.IndexOf('<');
                Delay = Expression.Substring(0, seperatorIndex).Trim();
                Delays = DesignController.ActiveDesign.Database.GetVariables(Delay);
                Dependent = Delay + ".d";
                Dependents = DesignController.ActiveDesign.Database.GetVariables(Dependent);

                bool hasAltClock = false;
                for (int i = seperatorIndex + 2; i < Expression.Length; i++)
                {
                    char currentChar = Expression[i];
                    if (hasAltClock && (currentChar == ' ' || currentChar == '\n' || currentChar == '('))
                    {
                        Expression = Expression.Substring(i).Trim();
                        break;
                    }
                    else if (!hasAltClock)
                    {
                        if (currentChar != '@')
                        {
                            Expression = Expression.Substring(i).Trim();
                            break;
                        }
                        else
                        {
                            hasAltClock = true;
                        }
                    }
                }
            }

            IsMathExpression = Expression.Any(c => c == '+' || c == '-');
            ExpressionIndex = fullExpression.LastIndexOf(Expression);
        }

        /// <summary>
        /// Solves the expression for its value.
        /// </summary>
        /// <returns>Value of the expression</returns>
        private int Solve()
        {
            Stack<int> valueStack = new Stack<int>();
            Stack<string> operatorStack = new Stack<string>();
            Stack<int> parenthesisIndicesStack = new Stack<int>();
            Parentheses = new Dictionary<int, Parenthesis>();
            bool wasPreviousOperand = false;

            // Obtain scalars, constants and operators
            string expression = $"({Expression})"; // Add () to expression
            MatchCollection matches = Regex.Matches(expression, $@"{Parser.ConcatPattern}|(~?(?<Name>\w+))|(~?[0-1])|([~^()|+-])|(==)|((?<=[\w)}}])\s+(?=[\w({{~'])(?![^{{}}]*\}}))");
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
                    bool areParenthesesNegated = operatorStack.Count > 0 && operatorStack.Peek() == "~";
                    if (areParenthesesNegated)
                    {
                        valueStack.Push(Convert.ToInt32(!Convert.ToBoolean(valueStack.Pop())));
                        operatorStack.Pop();
                    }

                    // Add parentheses that aren't the beginning and closing parenthesis pair to the parentheses dictionary
                    if (match.Index != expression.Length - 1)
                    {
                        bool parenthesesValue = valueStack.Peek() == 1;
                        Parentheses.Add(match.Index + ExpressionIndex - 1, new Parenthesis(")", parenthesesValue, areParenthesesNegated));
                        Parentheses.Add(parenthesisIndicesStack.Pop(), new Parenthesis("(", parenthesesValue, areParenthesesNegated));
                    }

                    wasPreviousOperand = true;
                }
                else if (match.Value == "(" || match.Value == "~" || Lexer.OperatorsList.Contains(match.Value))
                {
                    string operation = match.Value;

                    if (operation == "(" && wasPreviousOperand)
                    {
                        operatorStack.Push(" ");
                    }

                    // Check for operators that need evaluation
                    while (operatorStack.Count > 0 && (operation == "|" && operatorStack.Peek() == " "))
                    {
                        ExecuteOperation(ref valueStack, ref operatorStack);
                    }

                    // Push operation
                    if (match.Index != 0 && operation == "(")
                    {
                        parenthesisIndicesStack.Push(match.Index + ExpressionIndex - 1);
                    }
                    operatorStack.Push(operation);

                    wasPreviousOperand = false;
                }
                else
                {
                    if (wasPreviousOperand)
                    {
                        operatorStack.Push(" ");
                    }

                    // Process variable
                    string variable = match.Value;
                    bool containsNot = false;
                    if (variable[0] == '~')
                    {
                        containsNot = true;
                        variable = variable.Substring(1);
                    }

                    int value = DesignController.ActiveDesign.Database.GetValue(variable);
                    if (containsNot)
                    {
                        value = Convert.ToInt32(!Convert.ToBoolean(value));
                    }
                    valueStack.Push(value);

                    wasPreviousOperand = true;
                }
            }

            return valueStack.Pop();
        }

        /// <summary>
        /// Executes an operation.
        /// </summary>
        /// <param name="valueStack"></param>
        /// <param name="operatorStack"></param>
        private void ExecuteOperation(ref Stack<int> valueStack, ref Stack<string> operatorStack)
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
                    result = Convert.ToInt32(Convert.ToBoolean(leftValue == rightValue));
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

        /// <summary>
        /// Evaluates the expression and returns whether the value of the dependent was changed.
        /// </summary>
        /// <returns>Whether the value of the dependent changed</returns>
        public void Evaluate()
        {
            // Get binary of expression value
            string expressionBinary = Convert.ToString(Solve(), 2);
            if (expressionBinary.Length < Dependents.Length)
            {
                expressionBinary = string.Concat(new string('0', Dependents.Length - expressionBinary.Length), expressionBinary);
            }
            else if (expressionBinary.Length > Dependents.Length)
            {
                expressionBinary = expressionBinary.Substring(expressionBinary.Length - Dependents.Length);
            }

            // Store dependent binary
            DependentBinary = expressionBinary;
            // Set values
            DesignController.ActiveDesign.Database.SetValues(Dependents, expressionBinary);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisiBoole.Controllers;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// Base for expression statements.
    /// </summary>
    public abstract class ExpressionStatement : Statement
    {
        /// <summary>
        /// Dependent of the expression.
        /// </summary>
        public string Dependent { get; private set; }

        /// <summary>
        /// Delay of the expression. (If clock)
        /// </summary>
        public string Delay { get; private set; }

        /// <summary>
        /// Operation of the expression.
        /// </summary>
        public string Operation { get; private set; }

        /// <summary>
        /// Expression for the dependent.
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        /// Line number of the expression statement.
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Index of the first non whitespace character.
        /// </summary>
        private int StartIndex;

        /// <summary>
        /// Dictionary of parentheses contained in the expression.
        /// </summary>
        public Dictionary<int, Parenthesis> Parentheses { get; private set; }

        /// <summary>
        /// Constructs an ExpressionStatement instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
        /// <param name="lineNumber">Line number of the expression statement</param>
        protected ExpressionStatement(string text, int lineNumber) : base(text)
        {
            LineNumber = lineNumber;
            StartIndex = text.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            Expression = text.Substring(StartIndex); // Start expression with first non whitespace character
            Expression = Expression.TrimEnd(';');

            // Get operation, dependent and expression values
            if (!Expression.Contains("<"))
            {
                Operation = "=";
                Delay = null;
                Dependent = Expression.Substring(0, Expression.IndexOf('=')).Trim();
            }
            else
            {
                if (!Expression.Contains("@"))
                {
                    Operation = "<=";
                }
                else
                {
                    Operation = Regex.Match(Expression, @"<=@\S+").Value;
                }
                Delay = Expression.Substring(0, Expression.IndexOf('<')).Trim();
                Dependent = Delay + ".d";
            }
            Expression = Expression.Substring(Expression.IndexOf(Operation) + Operation.Length).Trim();
            Expression = Parser.WhitespaceRegex.Replace(Expression, " "); // Replace multiple spaces
        }

        /// <summary>
        /// Parses the expression text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            // Output padding (if present)
            for (int i = 0; i < StartIndex; i++)
            {
                Output.Add(new SpaceFeed());
            }

            // Output dependent (if =) or delay (if <=)
            string variable = (Delay == null) ? Dependent : Delay;
            OutputVariable(variable);

            // Output operator
            if (Operation == "=")
            {
                OutputOperator("=");
            }
            else
            {
                if (!Operation.Contains("@"))
                {
                    Output.Add(new DependentVariable(Operation, DesignController.ActiveDesign.Database.TryGetValue(Dependent) == 1));
                }
                else
                {
                    Output.Add(new DependentVariable(Operation, DesignController.ActiveDesign.Database.TryGetValue(Operation.Substring(Operation.IndexOf("@") + 1)) == 1));
                }
            }

            // Output expression
            // Operators: ([|^()+-])|(==)|(?<=\w|\))\s(?=[\w(~'])
            MatchCollection matches = Regex.Matches(Expression, $@"({Lexer.ConcatPattern})|(~?{Lexer.ScalarPattern})|(~?{Lexer.ConstantPattern})|([|^()+-])|(==)|(?<=\w|\))\s(?=[\w(~'])");
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == "(" || token == ")")
                {
                    Output.Add(Parentheses[match.Index]); // Output the corresponding parenthesis
                }
                else if (Parser.OperatorsList.Contains(token))
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

        /// <summary>
        /// Solves the expression for its value.
        /// </summary>
        /// <returns>Value of the expression</returns>
        private int Solve()
        {
            Stack<int> valueStack = new Stack<int>();
            Stack<string> operatorStack = new Stack<string>();
            Stack parenthesisStack = new Stack();
            Parentheses = new Dictionary<int, Parenthesis>();

            // Obtain scalars, constants and operators
            string expression = $"({Expression})"; // Add () to expression
            MatchCollection matches = Regex.Matches(expression, $@"{Parser.ConcatPattern}|(~?(?<Name>[_a-zA-Z]\w{{0,19}}))|(~?'[bB][0-1])|([~^()|+-])|(==)|((?<=[\w)}}])\s+(?=[\w({{~'])(?![^{{}}]*\}}))");
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
                        Parentheses.Add(match.Index - 1, new Parenthesis(")", parenthesesValue, areParenthesesNegated));
                        Parentheses.Add((int)parenthesisStack.Pop(), new Parenthesis("(", parenthesesValue, areParenthesesNegated));
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
                    if (match.Index != 0 && operation == "(")
                    {
                        parenthesisStack.Push(match.Index - 1);
                    }
                    operatorStack.Push(operation);
                }
                else
                {
                    // Process variable
                    string variable = match.Value;
                    bool containsNot = false;
                    if (variable[0] == '~')
                    {
                        containsNot = true;
                        variable = variable.Substring(1);
                    }
                    int value = GetValue(variable);
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
        /// Gets the value of the provided token.
        /// </summary>
        /// <param name="token">Token to get value of</param>
        /// <returns>Value of the token</returns>
        protected int GetValue(string token)
        {
            if (token.Contains("{"))
            {
                token = token.Substring(1, token.Length - 2);
                string[] vars = Parser.WhitespaceRegex.Split(token);

                // Get binary value
                StringBuilder binary = new StringBuilder();
                foreach (string var in vars)
                {
                    binary.Append(GetValue(var));
                }

                return Convert.ToInt32(binary.ToString(), 2);
            }
            else if (token.Contains("'"))
            {
                Match constant = Parser.ConstantRegex.Match(token);

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

                return Convert.ToInt32(outputBinary, 2);
            }
            else
            {
                return DesignController.ActiveDesign.Database.TryGetValue(token);
            }
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

        /// <summary>
        /// Evaluates the expression and returns whether the value of the dependent was changed.
        /// </summary>
        /// <returns>Whether the value of the dependent changed</returns>
        public bool Evaluate()
        {
            int expressionValue = Solve();
            int dependentValue = GetValue(Dependent);
            if (expressionValue != dependentValue)
            {
                if (!Dependent.Contains("{"))
                {
                    DesignController.ActiveDesign.Database.SetValue(Dependent, expressionValue == 1);
                }
                else
                {
                    string variables = Dependent.Substring(1, Dependent.Length - 2);
                    string[] vars = Parser.WhitespaceRegex.Split(variables);
                    if (vars.Length > 1)
                    {
                        vars = vars.Reverse().ToArray();
                    }
                    string binary = Convert.ToString(expressionValue, 2);
                    if (binary.Length < vars.Length)
                    {
                        binary = binary.PadLeft(vars.Length, '0');
                    }
                    if (binary.Length > 1)
                    {
                        char[] reverseBinary = binary.ToCharArray();
                        Array.Reverse(reverseBinary);
                        binary = new string(reverseBinary);
                    }

                    // Get binary value
                    for (int i = 0; i < vars.Length; i++)
                    {
                        string var = vars[i];
                        int val = int.Parse(binary[i].ToString());
                        DesignController.ActiveDesign.Database.SetValue(var, val == 1);
                    }
                }

                return true;
            }

            return false;
        }
    }
}

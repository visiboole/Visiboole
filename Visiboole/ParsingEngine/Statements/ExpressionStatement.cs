using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// Base for expression statements.
    /// </summary>
    public abstract class ExpressionStatement : Statement
    {
        /// <summary>
        /// Full expression of the expression statement.
        /// </summary>
        protected string FullExpression;

        /// <summary>
        /// Dependent of the expression statement.
        /// </summary>
        protected string Dependent;

        /// <summary>
        /// Expression of the expression statement.
        /// </summary>
        protected string Expression;

        /// <summary>
        /// Constructs an ExpressionStatement instance.
        /// </summary>
        /// <param name="database">Database of the parsed design</param>
        /// <param name="text">Text of the statement</param>
        protected ExpressionStatement(Database database, string text) : base(database, text)
        {
            // Get FullExpression, Dependent, Expression
            int start = Text.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            FullExpression = Text.Substring(start); // Start expression with first non whitespace character
            FullExpression = FullExpression.TrimEnd(';');
            Dependent = Text.Contains("<")
                ? Text.Substring(start, Text.IndexOf('<') - start).Trim()
                : Text.Substring(start, Text.IndexOf('=') - start).Trim();
            Expression = Text.Substring(Text.IndexOf("=") + 1).Trim();
            Expression = Expression.TrimEnd(';');
            Expression = Regex.Replace(Expression, @"\s+", " "); // Replace multiple spaces
        }

        /// <summary>
        /// Gets the value of the provided token
        /// </summary>
        /// <param name="token">Token to evaluate</param>
        /// <returns>Value of the token</returns>
        public int GetValue(string token)
        {
            if (token.Contains("{"))
            {
                token = token.Substring(1, token.Length - 2);
                string[] vars = Regex.Split(token, @"\s+");

                // Get binary value
                StringBuilder binary = new StringBuilder();
                foreach (string var in vars)
                {
                    binary.Append(Database.TryGetValue(Parser.ScalarRegex1.Match(var).Value));
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
                return Database.TryGetValue(token);
            }
        }

        /// <summary>
        /// Parses the expression text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            MatchCollection matches = Regex.Matches(Expression, @"(~?(?<Name>[_a-zA-Z]\w{0,19}))|(~?'[bB][0-1])|([~^()|+-])|(==)|(?<=\w|\))\s(?=[\w(~'])");
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == "(" || token == ")")
                {
                    Output.Add(new Parentheses(token));
                }
                else if (Parser.OperatorsList.Contains(token))
                {
                    OutputOperator(token);
                }
                else if (token.Contains("'"))
                {
                    Output.Add(new Constant(token));
                }
                else
                {
                    OutputVariable(token); // Variable
                }
            }
        }
    }
}

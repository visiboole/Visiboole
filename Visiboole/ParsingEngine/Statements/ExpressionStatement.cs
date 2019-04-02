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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    public class ModuleDeclarationStmt : Statement
    {
        /// <summary>
        /// Regex for getting output tokens.
        /// </summary>
        private Regex OutputRegex = new Regex($@"(\w+\()|(~?{Parser.ScalarPattern})|[\s;:,{{}})]");

        /// <summary>
        /// Constructs a ModuleDeclarationStmt instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
		public ModuleDeclarationStmt(string text) : base(text)
        {
            // Initialize variables in the statement
            InitVariables(text);
        }

        public override void Parse()
        {
            MatchCollection matches = OutputRegex.Matches(Text);
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == " ")
                {
                    Output.Add(new SpaceFeed());
                }
                else if (token.Contains("("))
                {
                    Output.Add(new Comment(token));
                }
                else if (token == "," || token == "{" || token == "}" || token == ":" || token == ")" || token == ";")
                {
                    OutputOperator(token);
                }
                else
                {
                    OutputVariable(token);
                }
            }

            // Output newline
            Output.Add(new LineFeed());
        }
    }
}

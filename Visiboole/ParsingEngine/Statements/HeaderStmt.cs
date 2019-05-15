using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisiBoole.Controllers;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    public class HeaderStmt : Statement
    {
        /// <summary>
        /// Regex for getting output tokens.
        /// </summary>
        private Regex OutputRegex = new Regex($@"(\w+\()|(~?{Parser.ScalarPattern})|[\s:,{{}})]");

        /// <summary>
        /// Constructs a Header Statement instance.
        /// </summary>
        /// <param name="text">Text of the statement</param>
		public HeaderStmt(string text) : base(text)
        {
        }

        public override List<IObjectCodeElement> Parse()
        {
            List<IObjectCodeElement> output = new List<IObjectCodeElement>();
            MatchCollection matches = OutputRegex.Matches(Text);
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == " ")
                {
                    output.Add(new SpaceFeed());
                }
                else if (token == "\n")
                {
                    // Output newline
                    output.Add(new LineFeed());
                }
                else if (token.Contains("("))
                {
                    output.Add(new Comment(token));
                }
                else if (token == "," || token == "{" || token == "}" || token == ":" || token == ")")
                {
                    output.Add(new Operator(token));
                }
                else
                {
                    IndependentVariable indVar = DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(token) as IndependentVariable;
                    DependentVariable depVar = DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>(token) as DependentVariable;
                    if (indVar != null)
                    {
                        output.Add(indVar);
                    }
                    else if (depVar != null)
                    {
                        output.Add(depVar);
                    }
                }
            }

            // Output ending semicolon
            output.Add(new Operator(";"));
            // Output new line
            output.Add(new LineFeed());
            // Return output list
            return output;
        }
    }
}

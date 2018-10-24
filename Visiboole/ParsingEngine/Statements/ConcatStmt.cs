using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    public class ConcatStmt : Statement
    {
        /// <summary>
	    /// The identifying pattern that can be used to identify and extract this statement from raw text
	    /// </summary>
        public static Regex Pattern { get; } = new Regex
            (@"^(" + regexArrayVariables + @"|" + regexStepArrayVariables + @")\s*\=\s*\{("
                    + regexVariable + @"|" + regexArrayIndexVariable + @"|" + regexArrayVariables + @"|" + regexStepArrayVariables + @")"
                    + @"(\,\s*(" + regexVariable + @"|" + regexArrayIndexVariable + @"|" + regexArrayVariables + @"|" + regexStepArrayVariables + @"))*\}\;$");

        /// <summary>
        /// The list of boolean statements in the concat
        /// </summary>
        public List<BooleanAssignmentStmt> Concats = new List<BooleanAssignmentStmt>();

        /// <summary>
        /// Constructs an instance of CommentStmt
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
        public ConcatStmt(int lnNum, string txt) : base(lnNum, txt)
        {
            Parse();
        }

        /// <summary>
        /// Parses the Text of this statement into a list of discrete IObjectCodeElement elements
        /// to be used by the html parser to generate formatted output to be displayed in simulation mode.
        /// </summary>
        public override void Parse()
        {
            try
            {
                Regex regex = new Regex(@"^(([a-zA-Z]+\[[0-9]+\.\.[0-9]+\])|([a-zA-Z]+\[[0-9]+\.[0-9]+\.[0-9]+\]))", RegexOptions.None); // Get left side
                string leftSide = regex.Match(Text).Value; // Left side of equal sign
                List<string> leftVars = ExpandVariables(leftSide); // Expand left side to get all left variables

                regex = new Regex(@"\{(([a-zA-Z]+)|([a-zA-Z]+[0-9]+)|([a-zA-Z]+\[[0-9]+\.\.[0-9]+\])|([a-zA-Z]+\[[0-9]+\.[0-9]+\.[0-9]+\]))(\,\s*(([a-zA-Z]+)|([a-zA-Z]+[0-9]+)|([a-zA-Z]+\[[0-9]+\.\.[0-9]+\])|([a-zA-Z]+\[[0-9]+\.[0-9]+\.[0-9]+\])))*\}", RegexOptions.None); // Get everything inside braces
                string rightSide = regex.Match(Text).Value; // Right side of equal sign

                regex = new Regex(@"[{\s*}]", RegexOptions.None); // Remove whitespace and braces
                rightSide = regex.Replace(rightSide, string.Empty);

                string[] parts = rightSide.Split(','); // Split variables by commas
                if (!rightSide.Contains(",")) parts[0] = rightSide;

                List<string> rightVars = new List<string>();
                foreach (string s in parts)
                {
                    regex = new Regex(@"(([a-zA-Z]+\[[0-9]+\.\.[0-9]+\])|([a-zA-Z]+\[[0-9]+\.[0-9]+\.[0-9]+\]))", RegexOptions.None);
                    if (regex.Match(s).Success)
                    {
                        List<string> expand = ExpandVariables(s);
                        foreach (string v in expand) rightVars.Add(v);
                    }
                    else rightVars.Add(s);
                }

                if (leftVars.Count != rightVars.Count) Globals.DisplayException(new Exception());

                /* Create new statements */
                foreach (string var in leftVars)
                {
                    string newLine = String.Concat(var, " = ", rightVars[leftVars.IndexOf(var)], ";");
                    Concats.Add(new BooleanAssignmentStmt((LineNumber + Concats.Count), newLine));
                }
            }
            catch (Exception ex)
            {
                Globals.DisplayException(ex);
            }
        }
    }
}
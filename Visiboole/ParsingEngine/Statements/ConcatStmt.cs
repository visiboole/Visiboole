using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    public class ConcatStmt : Statement
    {
        /// <summary>
	    /// The identifying pattern that can be used to identify and extract this statement from raw text
	    /// </summary>
        public static Regex Pattern { get; } = new Regex
            (@"^(" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")\s*\=\s*\{("
                    + Globals.regexVariable + @"|" + Globals.regexArrayIndexVariable + @"|" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")"
                    + @"(\,\s*(" + Globals.regexVariable + @"|" + Globals.regexArrayIndexVariable + @"|" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @"))*\}\;$");

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
            /* Get left side variables */
            Regex regex = new Regex(@"^(" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")", RegexOptions.None); // Get left side
            string leftSide = regex.Match(Text).Value; // Left side of equal sign
            List<string> leftVars = ExpandVariables(leftSide); // Expand left side to get all left variables

            /* Get right side */
            regex = new Regex(@"\{("
                + Globals.regexVariable + @"|" + Globals.regexArrayIndexVariable + @"|" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")"
                + @"(\,\s*(" + Globals.regexVariable + @"|" + Globals.regexArrayIndexVariable + @"|" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @"))*\}", RegexOptions.None); // Get everything inside braces
            string rightSide = regex.Match(Text).Value; // Right side of equal sign

            /* Remove whitespace and braces from right side */
            regex = new Regex(@"[{\s*}]", RegexOptions.None); // Remove whitespace and braces
            rightSide = regex.Replace(rightSide, string.Empty);

            /* Split right side variables */
            string[] parts = rightSide.Split(','); // Split variables by commas
            if (!rightSide.Contains(",")) parts[0] = rightSide;

            /* Get all right side variables */
            List<string> rightVars = new List<string>();
            foreach (string s in parts)
            {
                regex = new Regex(@"(" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")", RegexOptions.None);
                if (regex.Match(s).Success)
                {
                    List<string> expand = ExpandVariables(s);
                    foreach (string v in expand) rightVars.Add(v);
                }
                else rightVars.Add(s);
            }

            if (leftVars.Count != rightVars.Count)
            {
                MessageBox.Show("Number of variables on the left side is not equal to the number of variables on the right side. Line: " + (LineNumber + 1), "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /* Create new statements */
            foreach (string var in leftVars)
            {
                string newLine = String.Concat(var, " = ", rightVars[leftVars.IndexOf(var)], ";");
                Concats.Add(new BooleanAssignmentStmt((LineNumber + Concats.Count), newLine));
            }
        }
    }
}
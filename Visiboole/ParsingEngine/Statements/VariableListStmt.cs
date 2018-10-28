using System.Text.RegularExpressions;
using VisiBoole.ParsingEngine.ObjectCode;
using System;
using System.Collections.Generic;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// A list of visiboole independent variables that can be interacted with by the user
    /// </summary>
	public class VariableListStmt : Statement
	{
        /// <summary>
        /// The identifying pattern that can be used to identify and extract this statement from raw text
        /// </summary>
        public static Regex Pattern { get; } = new Regex
            (@"^(\*?" + Globals.regexVariable + @"|\*?" + Globals.regexArrayIndexVariable + @"|\*?" + Globals.regexArrayVariables + @"|\*?" + Globals.regexStepArrayVariables + @")"
                + @"(\s*(\*?" + Globals.regexVariable + @"|\*?" + Globals.regexArrayIndexVariable + @"|\*?" + Globals.regexArrayVariables + @"|\*?" + Globals.regexStepArrayVariables + @"))*\;$");

        /// <summary>
        /// Constructs an instance of VariableListStmt
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
		public VariableListStmt(int lnNum, string txt) : base(lnNum, txt)
		{
		}

	    /// <summary>
	    /// Parses the Text of this statement into a list of discrete IObjectCodeElement elements
	    /// to be used by the html parser to generate formatted output to be displayed in simulation mode.
	    /// </summary>
        public override void Parse()
		{
            /* Remove semicolon */
            Regex regex = new Regex(@"[;]", RegexOptions.None);
            string content = regex.Replace(Text, string.Empty);

            /* Split variables by whitespace */
            regex = new Regex(@"\s+", RegexOptions.None);
            string[] contents = regex.Split(content);

            /* Output all variables */
            foreach (string c in contents)
            {
                List<string> vars = new List<string>(); // List of variables

                /* Expand variable in necessary */
                regex = new Regex(@"(" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")", RegexOptions.None);
                if (regex.Match(c).Success)
                {
                    List<string> variables = ExpandVariables(c);
                    foreach (string var in variables)
                    {
                        vars.Add(var);
                    }
                }
                else vars.Add(c);

                /* Add output of each variable */
                foreach (string var in vars)
                {
                    bool val = (var[0] == '*') ? true : false;
                    string v = (var[0] == '*') ? var.Substring(1) : var;

                    IndependentVariable indVar = Database.TryGetVariable<IndependentVariable>(v) as IndependentVariable;
                    DependentVariable depVar = Database.TryGetVariable<DependentVariable>(v) as DependentVariable;
                    if (indVar != null)
                    {
                        //Database.SetValue(v, val);
                        Output.Add(indVar);
                    }
                    else if (depVar != null)
                    {
                        //Database.SetValue(v, val);
                        Output.Add(depVar);
                    }
                    else
                    {
                        IndependentVariable newVar = new IndependentVariable(v, val);
                        Database.AddVariable<IndependentVariable>(newVar);
                        Output.Add(newVar);
                    }
                }
            }

            LineFeed lf = new LineFeed();
            Output.Add(lf);
		}
	}
}
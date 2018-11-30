using System.Text.RegularExpressions;
using VisiBoole.ParsingEngine.ObjectCode;
using System;
using System.Collections.Generic;
using VisiBoole.Models;

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
            (@"^\s*(\*?" + Globals.regexVariable + @"|\*?" + Globals.regexArrayVariables + @"|\*?" + Globals.regexStepArrayVariables + @")"
                + @"(\s*(\*?" + Globals.regexVariable + @"|\*?" + Globals.regexArrayVariables + @"|\*?" + Globals.regexStepArrayVariables + @"))*\;$");

        /// <summary>
        /// Constructs an instance of VariableListStmt
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
		public VariableListStmt(SubDesign sd, int lnNum, string txt) : base(sd, lnNum, txt)
		{
		}

	    /// <summary>
	    /// Parses the Text of this statement into a list of discrete IObjectCodeElement elements
	    /// to be used by the html parser to generate formatted output to be displayed in simulation mode.
	    /// </summary>
        public override void Parse()
		{
            /* Clean content and make format string */
            string content = Regex.Replace(Text, @"[;]", string.Empty);
            string format = Regex.Replace(content, @"[\s]", "#");
            content = ReplaceVectors(content);
            format = ReplaceVectors(format);
            format = Regex.Replace(format, Globals.regexVariable, "X");
            format = Regex.Replace(format, @"[\s]", string.Empty);

            /* Split variables by whitespace */
            string[] variables = Regex.Split(content.Trim(), @"\s+");

            /* Output all variables */
            int i = 0;
            foreach (char c in format)
            {
                if (c == '#')
                {
                    SpaceFeed sf = new SpaceFeed();
                    Output.Add(sf);
                }
                else
                {
                    string var = variables[i++]; // Variable to be created

                    bool val = (var[0] == '*');
                    var = (var[0] == '*') ? var.Substring(1) : var;

                    IndependentVariable indVar = SubDesign.Database.TryGetVariable<IndependentVariable>(var) as IndependentVariable;
                    DependentVariable depVar = SubDesign.Database.TryGetVariable<DependentVariable>(var) as DependentVariable;
                    if (indVar != null)
                    {
                        //SubDesign.Database.SetValue(var, val);
                        Output.Add(indVar);
                    }
                    else if (depVar != null)
                    {
                        //SubDesign.Database.SetValue(var, val);
                        Output.Add(depVar);
                    }
                    else
                    {
                        IndependentVariable newVar = new IndependentVariable(var, val);
                        SubDesign.Database.AddVariable<IndependentVariable>(newVar);
                        Output.Add(newVar);
                    }
                }
            }

            LineFeed lf = new LineFeed();
            Output.Add(lf);
		}
	}
}
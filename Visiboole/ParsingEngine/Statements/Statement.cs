using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.Boolean;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// The base class for Visiboole statements. Visiboole statements represent the various
    /// different expressions that one can encounter within Visiboole HDL syntax.
    /// </summary>
	public abstract class Statement
	{
        /// <summary>
        /// The line number that this statement is located on within edit mode - not simulation mode
        /// </summary>
		public int LineNumber { get; set; }

        /// <summary>
        /// The raw, unparsed text of this statement
        /// </summary>
		public string Text { get; set; }

        /// <summary>
        /// A list of discrete output elements that comprise this statement
        /// </summary>
		public List<IObjectCodeElement> Output { get; set; } = new List<IObjectCodeElement>();

        /// <summary>
        /// Constructs an instance of this Statement with given line number and text representation
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
		protected Statement(int lnNum, string txt)
		{
			LineNumber = lnNum;
			Text = txt;
		}

        /// <summary>
        /// Parses the Text of this statement into a list of discrete IObjectCodeElement elements
        /// to be used by the html parser to generate formatted output to be displayed in simulation mode.
        /// </summary>
		public abstract void Parse();

        /// <summary>
        /// Expands vectors into its variables
        /// </summary>
        /// <param name="exp">Expression to expand</param>
        /// <returns>A list of all variables</returns>
        protected List<string> ExpandVector(string exp)
        {
            /* Get variable, bounds and step */
            string var = Regex.Match(exp, @"^\*?" + Globals.PatternVariable).Value; // Get Variable
            string nums = Regex.Match(exp, @"\[(.*?)\]").Value; // Get numbers
            MatchCollection matches = Regex.Matches(nums, @"\d+"); // Get bounds and step

            /* Assign start, step and end from num values */
            int start = Convert.ToInt32(matches[0].Value);
            int step = (matches.Count == 2) ? 1 : Convert.ToInt32(matches[1].Value);
            int end = (matches.Count == 2) ? Convert.ToInt32(matches[1].Value) : Convert.ToInt32(matches[2].Value);

            /* Create list with expanded variables */
            List<string> vars = new List<string>();
            if (start < end)
            {
                for (int i = start; i <= end; i += step)
                    vars.Add(String.Concat(var, i.ToString()));
            }
            else
            {
                for (int i = start; i >= end; i -= step)
                    vars.Add(String.Concat(var, i.ToString()));
            }
            return vars;
        }

        /// <summary>
        /// Replace vectors with its variables
        /// </summary>
        /// <param name="exp">Expression to replace vectors</param>
        /// <returns>Expression with vector replaced by its variables</returns>
        protected string ReplaceVectors(string exp)
        {
            MatchCollection matches = Regex.Matches(exp, @"\*?" + Globals.PatternAnyVectorType);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    List<string> variables = ExpandVector(match.Value); // Expand variables to list

                    /* Create expanded variables string */
                    string expanded = "";
                    for (int i = 0; i < variables.Count; i++)
                    {
                        if (i != (variables.Count - 1)) expanded = String.Concat(expanded + variables[i] + " ");
                        else expanded = String.Concat(expanded + variables[i]);
                    }
                    exp = exp.Replace(match.Value, expanded); // Replace vector with expanded variables
                }
            }

            return exp;
        }
    }
}
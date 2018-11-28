using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        /// Expands variables
        /// </summary>
        /// <param name="exp">Expression to expand</param>
        /// <returns>A list of all variables</returns>
        protected List<string> ExpandVariables(string exp)
        {
            #region Regex expansion
            /* Get variable */
            Regex regex = new Regex(@"^\*?[a-zA-Z0-9]+", RegexOptions.None);
            string var = regex.Match(exp).Value;

            /* Get everything inside brackets */
            regex = new Regex(@"\[(.*?)\]", RegexOptions.None);
            string nums = regex.Match(exp).Value;

            /* Remove brackets */
            regex = new Regex(@"[\[\]]", RegexOptions.None);
            nums = regex.Replace(nums, string.Empty);

            /* Get num values */
            regex = new Regex(@"[0-9]+", RegexOptions.None);
            MatchCollection matches = regex.Matches(nums);
            #endregion

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
    }
}
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VisiBoole.ErrorHandling;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// A formatted field is a way of displaying multiple Boolean variables as a single numeric
    /// value. A formatted field begins with a percent sign followed by a radix specifier followed by the
    /// list of variables enclosed in braces. The supported radix specifiers are: b, h, d, and u
    /// </summary>
	public class FormatSpecifierStmt : Statement
	{
        /// <summary>
        /// Format Specifier Pattern 
        /// </summary>
        public static Regex Pattern { get; } = new Regex
            (@"^\%[ubhdUBHD]\{(" + Globals.regexVariable + @"|" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")"
            + @"(\s*(" + Globals.regexVariable + @"|" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @"))*\}\;$");

        /// <summary>
        /// Constructs an instance of FormatSpecifierStmt
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
        public FormatSpecifierStmt(SubDesign sd, int lnNum, string txt) : base(sd, lnNum, txt)
		{
		}

	    /// <summary>
	    /// Parses the Text of this statement into a list of discrete IObjectCodeElement elements
	    /// to be used by the html parser to generate formatted output to be displayed in simulation mode.
	    /// </summary>
        public override void Parse()
		{
            #region Identify format, remove syntax and tokenize the variables
            /* Get format type */
            Regex regex = new Regex(@"^\%[ubhdUBHD]", RegexOptions.None);
            string format = regex.Match(Text).Value.Substring(1);

            /* Remove syntax */
            regex = new Regex(@"[{};]", RegexOptions.None);
            string content = regex.Replace(Text.Substring(2), string.Empty);

            /* Replace vectors if any */
            content = ReplaceVectors(content);

            /* Split variables by whitespace */
            regex = new Regex(@"\s+", RegexOptions.None);
            string[] variables = regex.Split(content);
            #endregion

            /* Get output values for each variable */
            List<int> valueList = new List<int>(); // List of output values
            foreach (string var in variables)
            {
                /* Add value of each variable to output values */
                int value = SubDesign.Database.TryGetValue(var);
                if (value != -1)
                {
                    valueList.Add(value);
                }
                else
                {
                    IndependentVariable newVar = new IndependentVariable(var, false);
                    SubDesign.Database.AddVariable<IndependentVariable>(newVar);
                    valueList.Add(0);
                }
            }

            /* Calculate output */
            string final = Calculate(format, valueList);
            Operator val = new Operator(final);
            Output.Add(val);
            LineFeed lf = new LineFeed();
            Output.Add(lf);
        }

        /// <summary>
        /// Converts the list of boolean values into a string representation of the 
        /// given format (specifier) token; binary, hex, signed, or unsigned.
        /// </summary>
        /// <param name="specifier">Format that the values should be converted to; binary, hex, signed, or unsigned.</param>
        /// <param name="values">The list of boolean (binary) values for this statement</param>
        /// <returns></returns>
        private string Calculate(string specifier, List<int> values)
        {
            switch (specifier.ToUpper())
            {
                case "B":
                    return ToBinary(values);
                case "H":
                    return ToHex(ToBinary(values));
                case "D":
                    return ToSigned(ToBinary(values));
                case "U":
                    return ToUnsigned(ToBinary(values));
                default:
                    return string.Empty;
            }
        }

        #region THIRD Possibly obsolete conversions - TODO
        /// <summary>
        /// Converts the given list to its string binary equivalent
        /// </summary>
        /// <param name="_vals">A list of 0 or 1 values</param>
        /// <returns>Returns a binary string representation</returns>
        private string ToBinary(List<int> _vals)
        {
            string binary = "";
            foreach (var variable in _vals)
            {
                binary += variable.ToString();
            }
            return binary;
        }

        /// <summary>
        /// Converts the given binary to its string unsigned decimal equivalent
        /// </summary>
        /// <param name="binary">A binary string representation</param>
        /// <returns>Returns an unsigned decimal string representation</returns>
        public string ToUnsigned(string binary) // decimal
        {
            int dec = 0;
            for (int i = 0; i < binary.Length; i++)
            {
                if (binary[binary.Length - i - 1] == '0') continue;
                dec += (int)Math.Pow(2, i);
            }
            return dec.ToString();
        }

        /// <summary>
        /// Converts the given binary to its string hex equivalent
        /// </summary>
        /// <param name="binary">A binary string representation</param>
        /// <returns>Returns a hexadecimal string representation</returns>
        private string ToHex(string binary)
        {
            //string binary = ToBinary(new List<int>());
            return Convert.ToInt32(binary, 2).ToString("X");
        }

        /// <summary>
        /// Converts the given binary to its string signed decimal equivalent
        /// </summary>
        /// <param name="binary">A binary string representation</param>
        /// <returns>Returns a signed decimal string representation</returns>
        private string ToSigned(string binary)
        {
            int index = binary.IndexOf("1", StringComparison.Ordinal);
            if (index == 0)
            {
                int num = -1 * Convert.ToInt32(ToUnsigned(binary.Substring(1)));
                return num.ToString();
            }
            return ToUnsigned(binary);
        }
        #endregion
    }
}
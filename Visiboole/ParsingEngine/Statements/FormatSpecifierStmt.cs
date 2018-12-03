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
        /// Regex for getting the format of the FormatSpecifier
        /// </summary>
        private static readonly Regex RegexFormat = new Regex (
            @"\%"                                       // %
            + @"[ubhdUBHD]"                             // u or b or h or d or U or B or H or D
        );

        /// <summary>
        /// Regex for getting the data of the FormatSpecifier
        /// </summary>
        private static readonly Regex RegexData = new Regex (
            @"\{"                                               // {
            + Globals.PatternAnyVariableType                    // Any Variable Type
            + @"("                                              // Being Optional Group
                + @"\s*"                                        // Any Number of Whitespace
                + Globals.PatternAnyVariableType                // Any Variable
            + @")*"                                             // End Optional Group
            + @"\}"                                             // }
        );

        /// <summary>
        /// Regex for a Format Specifier
        /// </summary>
        private static readonly Regex RegexFormatSpecifier = new Regex (
            RegexFormat.ToString()                          // Format of FormatSpecifier
            + RegexData.ToString()                          // Data of FormatSpecifier
        );

        /// <summary>
        /// Regex for a Format Specifier Statement
        /// </summary>
        public static readonly Regex Regex = new Regex (
            RegexFormatSpecifier.ToString()                 // FormatSpecifier
            + @"("                                          // Being Optional Group
                + @"\s*"                                    // Any Number of Whitespace
                + RegexFormat.ToString()                    // Format of FormatSpecifier
                + RegexData.ToString()                      // Data of FormatSpecifier
            + @")*"                                         // End Optional Group
        );

        /// <summary>
        /// Constructs an instance of FormatSpecifierStmt
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
        public FormatSpecifierStmt(int lnNum, string txt) : base(lnNum, txt)
		{
		}

	    /// <summary>
	    /// Parses the Text of this statement into a list of discrete IObjectCodeElement elements
	    /// to be used by the html parser to generate formatted output to be displayed in simulation mode.
	    /// </summary>
        public override void Parse()
		{
            /* Get output format */
            string outputFormat = RegexFormatSpecifier.Replace(Text, "X"); // Get output format
            outputFormat = Regex.Replace(outputFormat, @"[;]", string.Empty); // Remove syntax
            outputFormat = Regex.Replace(outputFormat, @"\s", "_"); // Get output format with spacing
            outputFormat = Regex.Replace(outputFormat, @"_X", "X"); // Remove one extra space

            /* Output format specifiers */
            MatchCollection matches = RegexFormatSpecifier.Matches(Text); // All Format Specifiers
            int index = 0; // Format Specifier Index
            foreach (char c in outputFormat)
            {
                if (c == '_')
                {
                    /* Add space to output feed */
                    SpaceFeed sf = new SpaceFeed();
                    Output.Add(sf);
                }
                else
                {
                    Match match = matches[index++]; // Get Format Specifier Match

                    /* Get format specifier and replace vectors if necessary */
                    string formatSpecifier = match.Value; // Get format specifier
                    string format = RegexFormat.Match(formatSpecifier).Value; // Get format of format specifier
                    string data = RegexData.Match(formatSpecifier).Value; // Get data of format specifier
                    data = Regex.Replace(data, @"[{}]", string.Empty); // Remove brackets
                    data = ReplaceVectors(data); // Replace vectors

                    /* Get variables and values */
                    string[] variables = Regex.Split(data, @"\s+"); // Split variables by whitespace
                    List<int> values = new List<int>(); // Values of variables
                    foreach (string var in variables)
                    {
                        /* Add value of each variable to output values */
                        int value = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetValue(var);
                        if (value != -1)
                        {
                            values.Add(value);
                        }
                        else
                        {
                            IndependentVariable newVar = new IndependentVariable(var, false);
                            Globals.tabControl.SelectedTab.SubDesign().Database.AddVariable<IndependentVariable>(newVar);
                            values.Add(0);
                        }
                    }

                    /* Output Format Specifier */
                    string output = Calculate(format.Substring(1), values); // Output values with format
                    Operator val = new Operator(output); // Operator of outpute values
                    Output.Add(val); // Add operator of output to output
                }
            }

            /* Add new line to output feed */
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
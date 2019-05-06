/*
 * Copyright (C) 2019 John Devore
 * Copyright (C) 2019 Chance Henney, Juwan Moore, William Van Cleve
 * Copyright (C) 2017 Matthew Segraves, Zachary Terwort, Zachary Cleary
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program located at "\Visiboole\license.txt".
 * If not, see <http://www.gnu.org/licenses/>
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VisiBoole.Controllers;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// A format statement that outputs variables in binary, decimal, unsigned and hex.
    /// </summary>
	public class FormatSpecifierStmt : Statement
	{
        /// <summary>
        /// Regex for getting output tokens.
        /// </summary>
        private Regex OutputRegex = new Regex($@"{Parser.FormatSpecifierPattern}|[\s]");

        /// <summary>
        /// Constructs a FormatSpecifierStmt instance.
        /// </summary>
        /// <param name="database">Database of the parsed design</param>
        /// <param name="text">Text of the statement</param>
        public FormatSpecifierStmt(string text) : base(text)
		{
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
		{
            // Find format specifiers and extra spacing
            MatchCollection matches = OutputRegex.Matches(Text);
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token == " ")
                {
                    Output.Add(new SpaceFeed());
                }
                else if (token == "\n")
                {
                    // Output newline
                    Output.Add(new LineFeed());
                }
                else
                {
                    bool clickable = true;

                    // Get variables and values
                    string[] variables = Parser.WhitespaceRegex.Split(match.Groups["Vars"].Value); // Split variables by whitespace
                    List<int> values = new List<int>(); // Values of variables
                    foreach (string var in variables)
                    {
                        if (var == "0" || var == "1")
                        {
                            values.Add(Convert.ToInt32(var));
                            if (clickable)
                            {
                                clickable = false;
                            }
                        }
                        else
                        {
                            IndependentVariable indVar = DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(var) as IndependentVariable;
                            DependentVariable depVar = DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>(var) as DependentVariable;
                            if (indVar != null)
                            {
                                if (indVar.Value) values.Add(1);
                                else values.Add(0);
                            }
                            else
                            {
                                if (depVar.Value) values.Add(1);
                                else values.Add(0);

                                if (clickable)
                                {
                                    clickable = false;
                                }
                            }
                        }
                    }

                    // Output Format Specifier
                    char format = match.Groups["Format"].Value[0];
                    string output = Calculate(match.Groups["Format"].Value, values); // Output values with format
                    string nextOutput = null;
                    if (clickable)
                    {
                        if (format == 'd' && values[0] == 1 && output == "0")
                        {
                            nextOutput = GetNextValue($"-{output}", format, values.Count);
                        }
                        else
                        {
                            nextOutput = GetNextValue(output, format, values.Count);
                        }
                    }
                    Output.Add(new Formatter(output, $"{{{match.Groups["Vars"].Value}", nextOutput));
                }
            }

            base.Parse();
        }

        private string GetNextValue(string value, char format, int bitCount)
        {
            int decValue;
            if (format == 'h')
            {
                decValue = Convert.ToInt32(value, 16);
            }
            else if (format == 'b')
            {
                decValue = Convert.ToInt32(value, 2);
            }
            else
            {
                decValue = Convert.ToInt32(value);
            }

            if (value[0] == '-')
            {
                decValue = Math.Abs(decValue) - (int)Math.Pow(2, bitCount - 1);
            }

            string nextValue = Convert.ToString(decValue + 1, 2);
            if (nextValue.Length > bitCount)
            {
                return nextValue.Substring(nextValue.Length - bitCount);
            }
            else if (nextValue.Length < bitCount)
            {
                return nextValue = string.Concat(new string('0', bitCount - nextValue.Length), nextValue);
            }
            else
            {
                return nextValue;
            }
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
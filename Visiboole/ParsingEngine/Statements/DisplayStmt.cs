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
using System.Text;
using System.Text.RegularExpressions;
using VisiBoole.Controllers;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// A display statement that outputs clickable variables or variable values in binary, decimal, unsigned and hex.
    /// </summary>
	public class DisplayStmt : Statement
	{
        /// <summary>
        /// Regex for getting output tokens.
        /// </summary>
        private Regex OutputRegex = new Regex($@"{Parser.FormatSpecifierPattern}|{Parser.ScalarPattern}|[\s01]");

        /// <summary>
        /// Constructs a FormatSpecifierStmt instance.
        /// </summary>
        /// <param name="database">Database of the parsed design</param>
        /// <param name="text">Text of the statement</param>
        public DisplayStmt(string text) : base(text)
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
                    if (token[0] != '%')
                    {
                        OutputVariable(token);
                    }
                    else
                    {
                        bool clickable = true;
                        string variableList = match.Groups["Vars"].Value;
                        string[] variables = Parser.WhitespaceRegex.Split(variableList);

                        var binaryBuilder = new StringBuilder();
                        foreach (string variable in variables)
                        {
                            if (char.IsDigit(variable[0]))
                            {
                                binaryBuilder.Append(variable);

                                if (clickable)
                                {
                                    clickable = false;
                                }
                            }
                            else
                            {
                                IndependentVariable indVar = DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(variable) as IndependentVariable;
                                DependentVariable depVar = DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>(variable) as DependentVariable;
                                if (indVar != null)
                                {
                                    if (indVar.Value) binaryBuilder.Append(1);
                                    else binaryBuilder.Append(0);
                                }
                                else
                                {
                                    if (depVar.Value) binaryBuilder.Append(1);
                                    else binaryBuilder.Append(0);

                                    if (clickable)
                                    {
                                        clickable = false;
                                    }
                                }
                            }
                        }

                        string binary = binaryBuilder.ToString();
                        char format = char.ToUpper(match.Groups["Format"].Value[0]);
                        string output = format == 'B' ? binary : Format(format, binary);;
                        int outputWidth = format == 'B' ? output.Length : GetWidth(format, variables.Length);
                        string nextOutput = clickable ? GetNextValue(binary) : null;

                        if (output.Length < outputWidth)
                        {
                            for (int i = 0; i < outputWidth - output.Length; i++)
                            {
                                Output.Add(new SpaceFeed());
                            }
                        }
                        Output.Add(new Formatter(output, $"{{{variableList}", nextOutput));
                    }
                }
            }

            base.Parse();
        }

        /// <summary>
        /// Returns the next binary string representation for the provided binary string representation.
        /// </summary>
        /// <param name="binary">Binary string representation</param>
        /// <returns>Next binary string representation</returns>
        private string GetNextValue(string binary)
        {
            string nextValue = Convert.ToString(Convert.ToInt32(binary, 2) + 1, 2);
            if (nextValue.Length > binary.Length)
            {
                return nextValue.Substring(nextValue.Length - binary.Length);
            }
            else if (nextValue.Length < binary.Length)
            {
                return nextValue = string.Concat(new string('0', binary.Length - nextValue.Length), nextValue);
            }
            else
            {
                return nextValue;
            }
        }

        private int GetWidth(char format, int variableCount)
        {
            if (format == 'D')
            {
                return (int)Math.Pow(2, variableCount - 1).ToString().Length + 1;
            }
            else if (format == 'U')
            {
                return ((int)Math.Pow(2, variableCount) - 1).ToString().Length;
            }
            else if (format == 'H')
            {
                return (int)(double)Math.Ceiling(decimal.Divide(variableCount, 4));
            }
            else
            {
                return variableCount;
            }
        }

        /// <summary>
        /// Formats the provided binary values with the provided format.
        /// </summary>
        /// <param name="format">Format of binary values</param>
        /// <param name="binary">Binary values to format</param>
        /// <returns>Formatted binary values</returns>
        private string Format(char format, string binary)
        {
            if (format == 'D')
            {
                return FormatDecimal(binary);
            }
            else if (format == 'U')
            {
                return FormatUnsigned(binary);
            }
            else if (format == 'H')
            {
                return FormatHex(binary);
            }
            else
            {
                return binary;
            }
        }

        /// <summary>
        /// Returns a signed decimal string representation from the provided binary string representation.
        /// </summary>
        /// <param name="binary">Binary string representation</param>
        /// <returns>Signed decimal string representation</returns>
        public string FormatDecimal(string binary)
        {
            int value = 0;

            if (binary[0] == '1')
            {
                value -= (int)Math.Pow(2, binary.Length - 1);
                return (value + Convert.ToInt32(FormatUnsigned(string.Concat('0', binary.Substring(1))))).ToString();
            }
            else
            {
                return FormatUnsigned(binary);
            }
        }

        /// <summary>
        /// Returns an unsigned decimal string representation from the provided binary string representation.
        /// </summary>
        /// <param name="binary">Binary string representation</param>
        /// <returns>Unsigned decimal string representation</returns>
        private string FormatUnsigned(string binary)
        {
            int value = 0;
            char[] binaryBits = binary.ToCharArray();
            Array.Reverse(binaryBits);

            for (int i = 0; i < binaryBits.Length; i++)
            {
                char bit = binaryBits[i];
                if (bit == '1')
                {
                    value += (int)Math.Pow(2, i);
                }
            }

            return value.ToString();
        }

        /// <summary>
        /// Returns a hexadecimal string representation from the provided binary string representation.
        /// </summary>
        /// <param name="binary">Binary string representation</param>
        /// <returns>Hexadecimal string representation</returns>
        private string FormatHex(string binary)
        {
            return Convert.ToInt32(binary, 2).ToString("X");
        }
    }
}
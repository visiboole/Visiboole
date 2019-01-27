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

 * You should have received a copy of the GNU General Public License
 * along with this program located at "\Visiboole\license.txt".
 * If not, see <http://www.gnu.org/licenses/>
 */

using System;
using System.Collections.Generic;

namespace VisiBoole.Models
{
	public class FormatSpecifier
	{
		/// <summary>
		/// The line number where this format specifier is located
		/// </summary>
		public int LineNumber;

		/// <summary>
		/// The list of variables that this format specifier is composed of
		/// </summary>
		private List<int> _vals;

		/// <summary>
		/// The format that this calculated variable list should return
		/// </summary>
		private string _format;

		/// <summary>
		/// Constructs an instance of FormatSpecifier
		/// </summary>
		/// <param name="lineNumber">The line number where this format specifier is located</param>
		/// <param name="vals">The list of variables that this format specifier is composed of</param>
		/// <param name="format">The format that this calculated variable list should return</param>
		public FormatSpecifier(int lineNumber, string format, List<int> vals)
		{
			LineNumber = lineNumber;
			_vals = vals;
			_format = format;
		}

		/// <summary>
		/// Calculates the varList field and returns a string in binary, hex, signed or unsigned decimal
		/// </summary>
		/// <returns>Returns a string in binary, hex, signed or unsigned decimal</returns>
		public string Calculate()
		{
			switch (_format.ToUpper())
			{
				case "B":
					return ToBinary();
				case "H":
					return ToHex();
				case "D":
					return ToSigned(ToBinary());
				case "U":
					return ToUnsigned(ToBinary());				
				default:					
					return string.Empty;
			}
		}

        #region Potentially obsolete conversion methods, uncommented
        private string ToBinary()
		{
			string binary = "";
			foreach (var variable in _vals)
			{
				binary += variable.ToString();
			}
			return binary;
			//string[] arr = _vals.ToArray().Select(c => c.ToString()).ToArray();
			//string.Join()

		}

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

		private string ToHex()
		{
			string binary = ToBinary();
			return Convert.ToInt32(binary, 2).ToString("X");
		}

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

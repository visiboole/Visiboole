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
using VisiBoole.ParsingEngine.Statements;

namespace VisiBoole.ErrorHandling
{
	/// <summary>
	/// The exception that is thrown when an empty value is detected that corresponds to a non-nullable field in the database
	/// </summary>
	/// <remarks></remarks>
	public class StatementNotRecognizedException : Exception
	{
		/// <summary>
		/// The edit mode line number that caused this exception
		/// </summary>
		public int LineNumber { get; set; }

		/// <summary>
		/// The control that is the cause of this exception
		/// </summary>
		public Statement SourceStatement { get; set; }

		/// <summary>
		/// Initializes a new instance of the StatementNotRecognizedException class
		/// </summary>
		/// <remarks></remarks>
		public StatementNotRecognizedException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the StatementNotRecognizedException class with a specified error message
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <remarks></remarks>
		public StatementNotRecognizedException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the StatementNotRecognizedException class with a specified error message and a reference
		/// to the inner exception that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="inner">The inner Exception if one exists</param>
		/// <remarks></remarks>
		public StatementNotRecognizedException(string message, Exception inner) : base(message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the StatementNotRecognizedException class with a specified error message and a reference
		/// to the inner exception that is the cause of this exception		
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="lineNumber">The edit mode line number that caused the exception</param>
		public StatementNotRecognizedException(string message, int lineNumber) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the StatementNotRecognizedException class with a specified error message and a reference
		/// to the control that is the cause of this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="source">The control that caused the exception</param>
		/// <remarks></remarks>
		public StatementNotRecognizedException(string message, Statement source) : base(message)
		{
			SourceStatement = source;
		}

	}
}
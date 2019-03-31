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

using System.Linq;
using System.Text.RegularExpressions;
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.ParsingEngine.Boolean;
using System;
using VisiBoole.Models;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// The Boolean assignment statement is the primary type of statement used to
    /// create digital designs. Assignment statements specify the value of a Boolean variable as a
    /// (digital logic) function of other Boolean variables. Its format is a variable name followed by
    /// either an equal sign or a less-than equal pair followed by a Boolean logic expression.Each such
    /// statement represents a network of logic gates and wires.
    /// </summary>
	public class BooleanAssignmentStmt : Statement
	{
        /// <summary>
        /// The full expression of the boolean statement
        /// </summary>
        private string FullExpression;

        /// <summary>
        /// The dependent of the boolean statement
        /// </summary>
        private string Dependent;

        /// <summary>
        /// The expression of the boolean statement
        /// </summary>
        private string Expression;

        /// <summary>
        /// Constructs an instance of BooleanAssignmentStmt
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
		public BooleanAssignmentStmt(int lnNum, string txt) : base(lnNum, txt)
		{
            // Get the dependent and the expression
            int start = Text.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            FullExpression = Text.Substring(start, (Text.IndexOf(';') - start));
            Dependent = FullExpression.Substring(0, FullExpression.IndexOf('=')).Trim();
            Expression = FullExpression.Substring(FullExpression.IndexOf('=') + 1).Trim();
            Expression = Regex.Replace(Expression, @"\s+", " "); // Replace multiple spaces

            // Add expression and dependency to the database
            Parser.Design.Database.AddExpression(Dependent, Expression);
            Parser.Design.Database.CreateDependenciesList(Dependent);

            // Update variable value
            Evaluate();
        }

        public void Evaluate()
        {
            bool dependentValue = ExpressionSolver.Solve(Expression) == 1;
            bool currentValue = Parser.Design.Database.TryGetValue(Dependent) == 1;
            if (dependentValue != currentValue)
            {
                Parser.Design.Database.SetValues(Dependent, dependentValue);
            }
        }

	    /// <summary>
	    /// Parses the Text of this statement into a list of discrete IObjectCodeElement elements
	    /// to be used by the html parser to generate formatted output to be displayed in simulation mode.
	    /// </summary>
        public override void Parse()
        {
            // Get index of first non whitespace character and pad spaces in front
            int start = Text.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            for (int i = 0; i < start; i++)
            {
                SpaceFeed space = new SpaceFeed();
                Output.Add(space);
            }

            // Update variable value
            DependentVariable depVar = Parser.Design.Database.TryGetVariable<DependentVariable>(Dependent) as DependentVariable;

            //Add dependent to output
            Output.Add(depVar);

            //Add sign to output
            Operator sign = new Operator("=");
            Output.Add(sign);

            //Add expression variables to output
            Output.AddRange(ExpressionSolver.GetOutput(Expression));

            //Add linefeed to output
            LineFeed lf = new LineFeed();
            Output.Add(lf);
        }
    }
}
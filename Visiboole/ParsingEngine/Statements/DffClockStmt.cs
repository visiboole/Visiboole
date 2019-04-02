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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.ParsingEngine.Boolean;
using VisiBoole.Models;

namespace VisiBoole.ParsingEngine.Statements
{
    /// <summary>
    /// An expression statement that assigns the value of an expression to a dependent on a clock tick.
    /// </summary>
    public class DffClockStmt : ExpressionStatement
    {
        /// <summary>
        /// Delay of the clock statement.
        /// </summary>
        private string Delay;

        /// <summary>
        /// Whether the clock is being ticked.
        /// </summary>
        private bool TickClock;

        /// <summary>
        /// Constructs a DffClockStmt instance.
        /// </summary>
        /// <param name="database">Database of the parsed design</param>
        /// <param name="text">Text of the statement</param>
        /// <param name="tick">Whether the clock is being ticked</param>
        public DffClockStmt(Database database, string text, bool tick) : base(database, text)
        {
            TickClock = tick;
            Delay = Dependent + ".d";

            // Add dependency and set delay value
            // Parser.Design.Database.AddExpression(Delay, Expression);
            Database.CreateDependenciesList(Delay);
            bool delayValue = ExpressionSolver.Solve(Database, Expression) == 1;
            Database.SetValue(Delay, delayValue);
        }

        /// <summary>
        /// Ticks the statement (dependent value is set to the delay value)
        /// </summary>
        public void Tick()
        {
            DependentVariable delayVariable = Database.TryGetVariable<DependentVariable>(Delay) as DependentVariable;
            Database.SetValue(Dependent, delayVariable.Value);
        }

        /// <summary>
        /// Parses the text of this statement into a list of output elements.
        /// </summary>
        public override void Parse()
        {
            // Output padding (if present)
            int start = Text.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            for (int i = 0; i < start; i++)
            {
                Output.Add(new SpaceFeed());
            }

            // Output dependent
            DependentVariable delayVariable = Database.TryGetVariable<DependentVariable>(Delay) as DependentVariable;
            IndependentVariable dependentInd = Database.TryGetVariable<IndependentVariable>(Dependent) as IndependentVariable;
            DependentVariable dependentDep = Database.TryGetVariable<DependentVariable>(Dependent) as DependentVariable;
            if (dependentInd != null)
            {
                Output.Add(dependentInd);
            }
            else
            {
                Output.Add(dependentDep);
            }

            // Tick (if necessary) and output delay
            if (TickClock)
            {
                bool delayValue = ExpressionSolver.Solve(Database, Expression) == 1;
                if (delayValue != delayVariable.Value)
                {
                    Database.SetValue(Delay, delayValue);
                    delayVariable = Database.TryGetVariable<DependentVariable>(Delay) as DependentVariable;
                }
            }
            DependentVariable dv = new DependentVariable("<=", delayVariable.Value);
            Output.Add(dv);

            // Output expression
            base.Parse();

            // Output newline
            Output.Add(new LineFeed());
        }
    }
}
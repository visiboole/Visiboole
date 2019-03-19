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
    public class DffClockStmt : Statement
    {
        /// <summary>
        /// The full expression of the clock statement
        /// </summary>
        private string FullExpression;

        /// <summary>
        /// The dependent of the clock statement
        /// </summary>
        private string Dependent;

        /// <summary>
        /// The delay of the clock statement
        /// </summary>
        private string Delay;

        /// <summary>
        /// The expression of the clock statement
        /// </summary>
        private string Expression;

        private bool clock_tick;
        private bool initial_run;

        public DffClockStmt(int lnNum, string txt, bool tick, bool init) : base(lnNum, txt)
        {
            clock_tick = tick;
            initial_run = init;

            // Get FullExpression, Dependent, Delay and Expression
            int start = Text.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            FullExpression = Text.Substring(start); // Start expression with first non whitespace character
            if (FullExpression.Contains(';'))
            {
                FullExpression = FullExpression.Substring(0, FullExpression.IndexOf(';'));
            }
            Dependent = FullExpression.Substring(0, FullExpression.IndexOf('<')).Trim();
            Delay = Dependent + ".d";
            Expression = FullExpression.Substring(FullExpression.IndexOf('=') + 1).Trim();
            Expression = Regex.Replace(Expression, @"\s+", " "); // Replace multiple spaces

            // Add dependency and set delay value
            // Globals.TabControl.SelectedTab.Design().Database.AddExpression(Delay, Expression);
            Globals.TabControl.SelectedTab.Design().Database.CreateDependenciesList(Delay);
            bool delayValue = ExpressionSolver.Solve(Expression);
            Globals.TabControl.SelectedTab.Design().Database.SetValue(Delay, delayValue);
        }

        public void Tick()
        {
            DependentVariable delayVariable = Globals.TabControl.SelectedTab.Design().Database.TryGetVariable<DependentVariable>(Delay) as DependentVariable;
            Globals.TabControl.SelectedTab.Design().Database.SetValue(Dependent, delayVariable.Value);
        }

        public override void Parse()
        {
            // Get index of first non whitespace character and pad spaces in front 
            int start = Text.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            for (int i = 0; i < start; i++)
            {
                SpaceFeed space = new SpaceFeed();
                Output.Add(space);
            }

            // Get output variables
            DependentVariable delayVariable = Globals.TabControl.SelectedTab.Design().Database.TryGetVariable<DependentVariable>(Delay) as DependentVariable;
            IndependentVariable dependentInd = Globals.TabControl.SelectedTab.Design().Database.TryGetVariable<IndependentVariable>(Dependent) as IndependentVariable;
            DependentVariable dependentDep = Globals.TabControl.SelectedTab.Design().Database.TryGetVariable<DependentVariable>(Dependent) as DependentVariable;

            // Create output
            if (dependentInd != null)
            {
                Output.Add(dependentInd);
            }
            else
            {
                Output.Add(dependentDep);
            }

            if (clock_tick)
            {
                bool delayValue = ExpressionSolver.Solve(Expression);
                if (delayValue != delayVariable.Value)
                {
                    Globals.TabControl.SelectedTab.Design().Database.SetValue(Delay, delayValue);
                    delayVariable = Globals.TabControl.SelectedTab.Design().Database.TryGetVariable<DependentVariable>(Delay) as DependentVariable;
                }
            }
            DependentVariable dv = new DependentVariable("<=", delayVariable.Value);
            Output.Add(dv);

            Output.AddRange(ExpressionSolver.GetOutput(Expression));

            LineFeed lf = new LineFeed();
            Output.Add(lf);
        }
    }
}
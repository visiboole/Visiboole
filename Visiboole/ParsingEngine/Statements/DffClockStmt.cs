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
        private string FullExpression { get; set; }

        /// <summary>
        /// The dependent of the clock statement
        /// </summary>
        private string Dependent { get; set; }

        /// <summary>
        /// The delay of the clock statement
        /// </summary>
        private string Delay { get; set; }

        /// <summary>
        /// The expression of the clock statement
        /// </summary>
        private string Expression { get; set; }

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

            // Set dependent value
            if (clock_tick || initial_run)
            {
                Variable test = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<Variable>(Delay);
                bool delayValue = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetValue(Delay) == 1;
                Globals.TabControl.SelectedTab.SubDesign().Database.SetValue(Dependent, delayValue);
            }

            // Add dependency and set delay value
            Globals.TabControl.SelectedTab.SubDesign().Database.CreateDependenciesList(Delay);
            Expression exp = new Expression();
            bool depValue = exp.Solve(Expression);
            Globals.TabControl.SelectedTab.SubDesign().Database.SetValue(Delay, depValue);
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
            DependentVariable delayVariable = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(Delay) as DependentVariable;
            IndependentVariable dependentInd = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(Dependent) as IndependentVariable;
            DependentVariable dependentDep = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(Dependent) as DependentVariable;

            // Create output
            if (dependentInd != null)
            {
                Output.Add(dependentInd);
            }
            else
            {
                Output.Add(dependentDep);
            }
            DependentVariable dv = new DependentVariable("<=", delayVariable.Value);
            Output.Add(dv);
            MakeExpressionOutput();

            LineFeed lf = new LineFeed();
            Output.Add(lf);
        }

        private void MakeExpressionOutput()
        {
            string[] elements = Expression.Split(' ');
            foreach (string item in elements)
            {
                string variable = item.Trim();

                int closedParenCount = 0;

                if (variable.Contains('('))
                {
                    while (variable.Contains("("))
                    {
                        Parentheses openParen;
                        try
                        {
                            if (variable[variable.IndexOf('(') - 1] == '~')
                            {
                                Operator notGate = new Operator("~");
                                openParen = new Parentheses("(");
                                variable = variable.Remove(variable.IndexOf('(') - 1, 2);
                                Output.Add(notGate);
                            }
                            else
                            {
                                openParen = new Parentheses("(");
                                variable = variable.Remove(variable.IndexOf('('), 1);
                            }
                        }
                        catch (Exception e)
                        {
                            openParen = new Parentheses("(");
                            variable = variable.Remove(variable.IndexOf('('), 1);
                        }

                        Output.Add(openParen);
                    }
                }

                if (variable.Contains(')'))
                {
                    while (variable.Contains(")"))
                    {
                        variable = variable.Remove(variable.IndexOf(')'), 1);
                        closedParenCount++;
                    }
                }

                #region logic for a NOT sign, an statement end, or both
                if (variable.Contains('~'))
                {
                    string newVariable = variable.Substring(1);
                    IndependentVariable indVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(newVariable) as IndependentVariable;
                    DependentVariable depVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(newVariable) as DependentVariable;
                    if (indVar != null)
                    {
                        IndependentVariable var = new IndependentVariable(variable, indVar.Value);
                        Output.Add(var);
                    }
                    else if (depVar != null)
                    {
                        DependentVariable var = new DependentVariable(variable, depVar.Value);
                        Output.Add(var);
                    }
                    else
                    {
                        Operator op = new Operator(variable);
                        Output.Add(op);
                    }
                }
                else if (variable.Contains('~') && variable.Contains(';'))
                {
                    string newVariable = variable.Substring(1, variable.IndexOf(';'));
                    IndependentVariable indVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(newVariable) as IndependentVariable;
                    DependentVariable depVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(newVariable) as DependentVariable;
                    if (indVar != null)
                    {
                        IndependentVariable var = new IndependentVariable(variable, indVar.Value);
                        Output.Add(var);
                    }
                    else if (depVar != null)
                    {
                        DependentVariable var = new DependentVariable(variable, indVar.Value);
                        Output.Add(var);
                    }
                    else
                    {
                        Operator op = new Operator(variable);
                        Output.Add(op);
                    }
                }
                else
                {
                    if (variable.Contains(';'))
                    {
                        variable = variable.Substring(0, variable.IndexOf(';'));
                    }
                    IndependentVariable indVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(variable) as IndependentVariable;
                    DependentVariable depVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(variable) as DependentVariable;

                    if (indVar != null)
                    {
                        Output.Add(indVar);
                    }
                    else if (depVar != null)
                    {
                        Output.Add(depVar);
                    }
                    else
                    {
                        Operator op = new Operator(variable);
                        Output.Add(op);
                    }
                }
                #endregion

                for (int i = closedParenCount; i != 0; i--)
                {

                    Parentheses closedParen = new Parentheses(")");
                    Output.Add(closedParen);
                }
            }
        }
    }
}
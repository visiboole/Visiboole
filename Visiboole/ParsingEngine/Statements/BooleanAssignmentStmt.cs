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
        private string FullExpression { get; set; }

        /// <summary>
        /// The dependent of the boolean statement
        /// </summary>
        private string Dependent { get; set; }

        /// <summary>
        /// The expression of the boolean statement
        /// </summary>
        private string Expression { get; set; }

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

            // Add expression and dependency to the database
            Globals.TabControl.SelectedTab.SubDesign().Database.AddExpression(Dependent, Expression);
            Globals.TabControl.SelectedTab.SubDesign().Database.CreateDependenciesList(Dependent);

            // Update variable value
            Evaluate();
        }

        public void Evaluate()
        {
            Expression exp = new Expression();
            bool dependentValue = exp.Solve(Expression);
            bool currentValue = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetValue(Dependent) == 1;
            if (dependentValue != currentValue)
            {
                Globals.TabControl.SelectedTab.SubDesign().Database.SetValues(Dependent, dependentValue);
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
            DependentVariable depVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(Dependent) as DependentVariable;
            MakeOrderedOutput(depVar);
        }

        /// <summary>
        /// Arranges the output (IObjectCodeElement) elements to represent this statement as it is written, left to right.
        /// </summary>
        /// <param name="dependent">The dependent variable being assigned to in the given expression</param>
        private void MakeOrderedOutput(DependentVariable dependent)
        {
            //Add dependent to output
            Output.Add(dependent);

            //Add sign to output
            Operator sign = new Operator("=");
            Output.Add(sign);

            //Add expression variables to output
            string[] elements = Expression.Split(' ');
            foreach (string item in elements)
            {
                string variable = item.Trim();
                if(variable.Contains('~'))
                {
                    int closedParenCount = 0;
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

                    while (variable.Contains(")"))
                    {
                        variable = variable.Remove(variable.IndexOf(')'), 1);
                        closedParenCount++;
                    }

                    string newVariable = variable;
                    //If it STILL contains a not gate. HACK.
                    if (variable.Contains('~'))
                    {
                        newVariable = variable.Substring(1);
                    }
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

                    for (int i = closedParenCount; i != 0; i--)
                    {
                        Parentheses closedParen = new Parentheses(")");
                        Output.Add(closedParen);
                    }
                }
                else if(variable.Contains('('))
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

                    IndependentVariable indVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(variable) as IndependentVariable;
                    DependentVariable depVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(variable) as DependentVariable;
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
                else if(variable.Contains(')'))
                {
                    int closedParenCount = 0;
                    while (variable.Contains(")"))
                    {
                        variable = variable.Remove(variable.IndexOf(')'), 1);
                        closedParenCount++;
                    }

                    IndependentVariable indVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(variable) as IndependentVariable;
                    DependentVariable depVar = Globals.TabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(variable) as DependentVariable;
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

                    for (int i = closedParenCount; i != 0; i--)
                    {
                        Parentheses closedParen = new Parentheses(")");
                        Output.Add(closedParen);
                    }
                }
                else if(variable.Contains('~') && variable.Contains(';'))
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
            }

            //Add linefeed to output
            LineFeed lf = new LineFeed();
            Output.Add(lf);
        }
    }
}
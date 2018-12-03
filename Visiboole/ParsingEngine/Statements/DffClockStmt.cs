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
        private bool clock_tick;
        private bool initial_run;

        public DffClockStmt(int lnNum, string txt, bool tick, bool init) : base(lnNum, txt)
        {
            clock_tick = tick;
            initial_run = init;
        }

        public override void Parse()
        {
            /* Get index of first non whitespace character and pad spaces in front */
            int start = Text.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            for (int i = 0; i < start; i++)
            {
                SpaceFeed space = new SpaceFeed();
                Output.Add(space);
            }

            string fullExpression = Text.Substring(start); // Start expression with first non whitespace character
            if (fullExpression.Contains(';'))
            {
                fullExpression = fullExpression.Substring(0, fullExpression.IndexOf(';'));
            }

            #region Identify and format clock statement
            //get our dependent variable and expression
            string dependent = fullExpression.Substring(0, fullExpression.IndexOf('='));
            //use for Globals.tabControl.SelectedTab.SubDesign().Database calls (value)
            string delay = dependent + ".d";
            //use to make output with (name)
            string delayDisplayAs = "<";
            string expression = fullExpression.Substring(fullExpression.IndexOf('=') + 1);

            dependent = dependent.Substring(0, dependent.IndexOf('<'));
            dependent = dependent.Trim();

            //format expression
            expression = expression.Trim();

            //dependent = delay;
            bool dependentValue;
            if (clock_tick || initial_run)
            {
                dependentValue = GetVariable(delay);
                Globals.tabControl.SelectedTab.SubDesign().Database.SetValue(dependent, dependentValue);
            }
            #endregion

            #region Make dependencies, solve for delay
            //make dependencies list
            Globals.tabControl.SelectedTab.SubDesign().Database.CreateDependenciesList(delay);
            //solve for delay;
            bool delayValue;
            Expression exp = new Expression();
            delayValue = exp.Solve(expression);
            //get the delay variable
            IndependentVariable delayVariable = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(delay) as IndependentVariable;
            if (delayVariable != null)
            {
                Globals.tabControl.SelectedTab.SubDesign().Database.SetValue(delay, delayValue);
                //Globals.tabControl.SelectedTab.SubDesign().Database.SetDepVar(delay, delayValue);
            }
            else
            {
                delayVariable = new IndependentVariable(delay, delayValue);
                Globals.tabControl.SelectedTab.SubDesign().Database.AddVariable<IndependentVariable>(delayVariable);
            }
            #endregion

            #region output
            //make the output
            IndependentVariable dependentInd = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(dependent) as IndependentVariable;
            DependentVariable dependentDep = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(dependent) as DependentVariable;
            if (dependentInd != null)
            {
                MakeOrderedOutputInd(dependentInd, delayVariable, delayDisplayAs, expression);
            }
            else if (dependentDep != null)
            {
                MakeOrderedOutputDep(dependentDep, delayVariable, delayDisplayAs, expression);
            }
            #endregion
        }

        #region Ordering output for both variable types
        private void MakeOrderedOutputInd(IndependentVariable independentVar, IndependentVariable delay, string displayAs, string expression)
        {
            //Add independentVar to output
            Output.Add(independentVar);

            DependentVariable dv = new DependentVariable("<=", delay.Value);
            Output.Add(dv);

            //Add expression variables to output
            MakeExpressionOutput(expression);

            //Add linefeed to output
            LineFeed lf = new LineFeed();
            Output.Add(lf);
        }

        private void MakeOrderedOutputDep(DependentVariable dependentVar, IndependentVariable delay, string displayAs, string expression)
        {
            //Add independentVar to output
            Output.Add(dependentVar);

            DependentVariable dv = new DependentVariable("<=", delay.Value);
            Output.Add(dv);

            //Add expression variables to output
            MakeExpressionOutput(expression);

            //Add linefeed to output
            LineFeed lf = new LineFeed();
            Output.Add(lf);
        }
        #endregion

        private void MakeExpressionOutput(string expression)
        {
            string exp = expression;
            string[] elements = exp.Split(' ');
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
                    IndependentVariable indVar = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(newVariable) as IndependentVariable;
                    DependentVariable depVar = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(newVariable) as DependentVariable;
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
                    IndependentVariable indVar = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(newVariable) as IndependentVariable;
                    DependentVariable depVar = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(newVariable) as DependentVariable;
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
                    IndependentVariable indVar = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(variable) as IndependentVariable;
                    DependentVariable depVar = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(variable) as DependentVariable;

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

        /// <summary>
        /// Returns the value of the variable matching the given name. If there is no match,
        /// a new variable initialized to false is inserted into the Globals.tabControl.SelectedTab.SubDesign().Database
        /// </summary>
        /// <param name="variableName">The name of the variable to search for</param>
        /// <returns>Returns the value of the variable matching the given name</returns>
        private bool GetVariable(string variableName)
        {
            #region If a variable was found, return val
            //See if variable was already declared in IndependentVariables
            IndependentVariable indVariable = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<IndependentVariable>(variableName) as IndependentVariable;

            //See if variable was already declared in DependentVariables
            DependentVariable depVariable = Globals.tabControl.SelectedTab.SubDesign().Database.TryGetVariable<DependentVariable>(variableName) as DependentVariable;

            //If variable was found in IndependentVariables
            if (indVariable != null)
            {
                //add variable to Output
                //Output.Add(indVariable);

                //return the value of the independent variable
                return indVariable.Value;
            }

            //If variable was found in DependentVariables
            else if (depVariable != null)
            {
                //add variable to Output
                //Output.Add(depVariable);

                //return the value of the dependent variable
                return depVariable.Value;
            }
            #endregion

            //Else the variable was not found
            else
            {
                //create a variable with a false value since it was not declared
                indVariable = new IndependentVariable(variableName, false);

                //Now add the variable to the Globals.tabControl.SelectedTab.SubDesign().Database
                Globals.tabControl.SelectedTab.SubDesign().Database.AddVariable<IndependentVariable>(indVariable);

                //Add variable to Output
                //Output.Add(indVariable);

                //return the value of the independent variable
                return indVariable.Value;
            }
        }
    }
}

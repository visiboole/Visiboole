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
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Boolean
{
    /// <summary>
    /// Boolean expression object
    /// </summary>
    public class Expression
    {
        /// <summary>
        /// Constructs a boolean expression object
        /// </summary>
        public Expression()
        {
        }

        /// <summary>
        /// Solves the given boolean expression
        /// </summary>
        /// <param name="dependent">The dependent variable assigned the value of the given expression</param>
        /// <param name="expression">The boolean expression associated with the given dependent variable</param>
        /// <returns>Returns the value of the given expression assigned to the dependent variable</returns>
        public bool Solve(string expression)
        {
            string fullExp = expression;
            string exp = "";
            string value = "";
            while (!GetInnerMostExpression(fullExp).Equals(fullExp))
            {
                exp = GetInnerMostExpression(fullExp);
                value = SolveBasicBooleanExpression(exp);
                exp = "(" + exp + ")";
                fullExp = fullExp.Replace(exp, value);
            }
            fullExp = SolveBasicBooleanExpression(fullExp);
            if (fullExp.Equals("TRUE"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Parses out the content contained within the innermost parenthesis of the expression
        /// </summary>
        /// <param name="expression">The expression to parse</param>
        /// <returns>Returns the content contained within the innermost parenthesis of the expression</returns>
        private string GetInnerMostExpression(string expression)
        {
            // this variable keeps track of the ('s in the expression.
            int innerStart;
            // this variable makes sure to keep the farthest inward  (  before hitting a  )  .
            int lastStart = 0;
            // this variable finds the index innermost  )  .
            int innerEnd = expression.IndexOf(')');
            // this will be the final expression if there are  ()  within the starting expression.
            string exp;
            // check to see if any  )'s  were found.
            if (innerEnd != -1)
            {
                // chop off the right side of the expression where the  )  starts.
                exp = expression.Substring(0, innerEnd);
                // chop off all  ('s  until there is only one left.
                do
                {
                    innerStart = exp.IndexOf('(');
                    // if there was a  (  found chop off the left side of expression where the  ( starts.
                    if (innerStart != -1)
                    {
                        lastStart = innerStart;
                        exp = exp.Substring(lastStart + 1);
                    }
                } while (innerStart != -1);
                // now return the inner most expression with no  ()'s  .
                return exp;
            }
            return expression;
        }

        /// <summary>
        /// Solves a boolean expression that has been simplified to only ands, ors, and nots
        /// </summary>
        /// <param name="dependent">The dependent variable that is assigned the given expression</param>
        /// <param name="expression">The expression that is associated with the given dependent variable</param>
        /// <returns>Returns a string of 'TRUE' and 'FALSE'</returns>
        private string SolveBasicBooleanExpression(string expression)
        {
            // set basicExpression variable
            string basicExpression = expression;

            if(basicExpression.Contains("^"))
            {
                // look for [xor] gates
                basicExpression = ParseXOrs(basicExpression);
            }
            else
            {
                // look for [not] gates
                basicExpression = ParseNots(basicExpression);

                // look for [eqaulto] gates
                //basicExpression = ParseEqualTo(basicExpression);

                // look for [and] gates
                basicExpression = ParseAnds(basicExpression);

                // look for [or] gates
                basicExpression = ParseOrs(basicExpression);
            }

            // return the end result ("TRUE" or "FALSE")
            return basicExpression;
        }

        /// <summary>
        /// Parses the negated subexpressions within the given expression
        /// </summary>
        /// <param name="dependent">The dependent variable that is assigned the given expression</param>
        /// <param name="expression">The expression that is associated with the given dependent variable</param>
        /// <returns>Return expression with [not] gates replaced with values</returns>
        private string ParseNots(string expression)
        {
            // set basicExpression variable
            string basicExpression = expression;

            //get first not gate's index (if there is one)
            int notGate = basicExpression.IndexOf('~');

            while (notGate != -1)
            {
                // eleminating everything but the varible
                string oldVariable = basicExpression.Substring(notGate);
                if (!oldVariable.IndexOf(' ').Equals(-1))
                {
                    oldVariable = oldVariable.Substring(0, oldVariable.IndexOf(' '));
                }

                // get rid of the ~ so we can check for the variable in the dictionary
                string newVariable = oldVariable.Substring(1);

                bool variableValue = Globals.TabControl.SelectedTab.Design().Database.TryGetValue(newVariable) == 1;

                // Might have to switch around
                if (variableValue)
                {
                    basicExpression = basicExpression.Replace(oldVariable, "FALSE");
                }
                else
                {
                    basicExpression = basicExpression.Replace(oldVariable, "TRUE");
                }

                // Add the variable to the Dependencies
                //Database.AddDependencies(dependent, newVariable);

                // find the next not gate
                notGate = basicExpression.IndexOf('~');
            }

            // return expression with [not] gates replaced with values
            return basicExpression;
        }

        /// <summary>
        /// Parses the "and" subexpressions within the given expression
        /// </summary>
        /// <param name="dependent">The dependent variable that is assigned the given expression</param>
        /// <param name="expression">The expression that is associated with the given dependent variable</param>
        /// <returns>Return expression with [not] gates replaced with values</returns>
        private string ParseAnds(string expression)
        {
            // set basicExpression variable
            string basicExpression = expression;

            // split into a string array off of the [or] gate
            string[] andExpression = basicExpression.Split('|');

            // format the expression
            for (int i = 0; i < andExpression.Length; i++)
            {
                andExpression[i] = andExpression[i].Trim();
            }

            // loop through each element
            foreach (string exp in andExpression)
            {
                // break element up to see if it has multiple variables
                string[] elements = exp.Split(' ');

                // make a new array to store int's instead of string's
                int[] inputs = new int[elements.Length];

                // loop through each element to get their boolean value
                for (int i = 0; i < elements.Length; i++)
                {
                    // check for TRUE
                    if (elements[i].Equals("TRUE"))
                    {
                        inputs[i] = 1;
                    }
                    // check for FALSE
                    else if (elements[i].Equals("FALSE"))
                    {
                        inputs[i] = 0;
                    }
                    // check independent and dependent variables
                    else
                    {
                        bool variableValue = Globals.TabControl.SelectedTab.Design().Database.TryGetValue(elements[i]) == 1;
                        if (variableValue)
                        {
                            inputs[i] = 1;
                        }
                        else
                        {
                            inputs[i] = 0;
                        }

                        // Add the variable to the Dependencies
                        //Database.AddDependencies(dependent, elements[i]);
                    }
                }
                // applies [and] gate to each input/expression
                if (And(inputs) == 1)
                {
                    // replace variable with TRUE
                    basicExpression = basicExpression.Replace(exp, "TRUE");
                }
                else
                {
                    // replace variable with FALSE
                    basicExpression = basicExpression.Replace(exp, "FALSE");
                }
            }

            // return expression with [and] gates replaced with values
            return basicExpression;
        }

        /// <summary>
        /// Parses the "or" subexpressions within the given expression
        /// </summary>
        /// <param name="dependent">The dependent variable that is assigned the given expression</param>
        /// <param name="expression">The expression that is associated with the given dependent variable</param>
        /// <returns>Return expression with [not] gates replaced with values</returns>
        private string ParseOrs(string expression)
        {
            // set basicExpression variable
            string basicExpression = expression;

            // split into a string array off of the [or] gate
            string[] elements = basicExpression.Split('|');

            // format the expression
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = elements[i].Trim();
            }

            // make a new array to store int's instead of string's
            int[] inputs = new int[elements.Length];

            // loop through each element of get their boolean value
            for (int i = 0; i < elements.Length; i++)
            {
                // check for TRUE
                if (elements[i].Equals("TRUE"))
                {
                    inputs[i] = 1;
                }
                // check for FALSE
                else if (elements[i].Equals("FALSE"))
                {
                    inputs[i] = 0;
                }
                // check independent and dependent variables
                else
                {
                    bool variableValue = Globals.TabControl.SelectedTab.Design().Database.TryGetValue(elements[i]) == 1;
                    if (variableValue)
                    {
                        inputs[i] = 1;
                    }
                    else
                    {
                        inputs[i] = 0;
                    }

                    // Add the variable to the Dependencies
                    //Database.AddDependencies(dependent, elements[i]);
                }
            }
            // compute the whole value of the expression
            int finalValue = Or(inputs);

            // return the result as a string 
            if (finalValue == 1)
            {
                return "TRUE";
            }
            else
            {
                return "FALSE";
            }
        }

        /// <summary>
        /// Parses the "xor" subexpressions within the given expression
        /// </summary>
        /// <param name="dependent">The dependent variable that is assigned the given expression</param>
        /// <param name="expression">The expression that is associated with the given dependent variable</param>
        /// <returns>Return expression with [not] gates replaced with values</returns>
        private string ParseXOrs(string expression)
        {
            // set basicExpression variable
            string basicExpression = expression;

            // split into a string array off of the [or] gate
            string[] elements = basicExpression.Split('^');

            // format the expression
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = elements[i].Trim();
            }

            // make a new array to store int's instead of string's
            int[] inputs = new int[elements.Length];

            for (int i = 0; i < elements.Length; i++)
            {
                // check for TRUE
                if (elements[i].Equals("TRUE"))
                {
                    inputs[i] = 1;
                }
                // check for FALSE
                else if (elements[i].Equals("FALSE"))
                {
                    inputs[i] = 0;
                }
                // check independent and dependent variables
                else
                {
                    bool variableValue = Globals.TabControl.SelectedTab.Design().Database.TryGetValue(elements[i]) == 1;
                    if (variableValue)
                    {
                        inputs[i] = 1;
                    }
                    else
                    {
                        inputs[i] = 0;
                    }

                    // Add the variable to the Dependencies
                    //Database.AddDependencies(dependent, elements[i]);
                }
            }
            // applies [and] gate to each input/expression
            if (XOr(inputs) == 1)
            {
                // replace variable with TRUE
                //basicExpression = basicExpression.Replace(exp, "TRUE");
                return "TRUE";
            }
            else
            {
                // replace variable with FALSE
                //basicExpression = basicExpression.Replace(exp, "FALSE");
                return "FALSE";
            }
        }

        /// <summary>
        /// Parses the "and" subexpressions within the given expression
        /// </summary>
        /// <param name="dependent">The dependent variable that is assigned the given expression</param>
        /// <param name="expression">The expression that is associated with the given dependent variable</param>
        /// <returns>Return expression with [not] gates replaced with values</returns>
        /*
        private string ParseEqualTo(string expression)
        {
            // set basicExpression variable
            string basicExpression = expression;

            // split into a string array off of the [or] gate
            string[] equalToExpression = basicExpression.Split(new string[] { "==" }, StringSplitOptions.None);

            // format the expression
            for (int i = 0; i < equalToExpression.Length; i++)
            {
                equalToExpression[i] = equalToExpression[i].Trim();
            }

            // loop through each element
            foreach (string exp in equalToExpression)
            {
                // break element up to see if it has multiple variables
                string[] elements = exp.Split(' ');

                // make a new array to store int's instead of string's
                int[] inputs = new int[elements.Length];

                // loop through each element to get their boolean value
                for (int i = 0; i < elements.Length; i++)
                {
                    // check for TRUE
                    if (elements[i].Equals("TRUE"))
                    {
                        inputs[i] = 1;
                    }
                    // check for FALSE
                    else if (elements[i].Equals("FALSE"))
                    {
                        inputs[i] = 0;
                    }
                    // check independent and dependent variables
                    else
                    {
                        bool variableValue = Globals.TabControl.SelectedTab.Design().Database.TryGetValue(elements[i]) == 1;
                        if (variableValue)
                        {
                            inputs[i] = 1;
                        }
                        else
                        {
                            inputs[i] = 0;
                        }

                        // Add the variable to the Dependencies
                        //Database.AddDependencies(dependent, elements[i]);
                    }
                }
                // applies [and] gate to each input/expression
                if (Equals(inputs) == 1)
                {
                    // replace variable with TRUE
                    basicExpression = basicExpression.Replace(exp, "TRUE");
                }
                else
                {
                    // replace variable with FALSE
                    basicExpression = basicExpression.Replace(exp, "FALSE");
                }
            }

            // return expression with [and] gates replaced with values
            return basicExpression;
        }
        */

        /// <summary>
        /// Negates the given value
        /// </summary>
        /// <param name="value">The value to negate</param>
        /// <returns>Returns the negated value</returns>
        private int Negate(int value)
        {
            if (value == 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// "Ands" the given values
        /// </summary>
        /// <param name="values">The values to "And"</param>
        /// <returns>Returns the "And'ed" values</returns>
        private int And(int[] values)
        {
            foreach (int value in values)
            {
                if (value == 0)
                {
                    return 0;
                }
            }
            return 1;
        }

        private int Equals(int[] values)
        {
            int equalValue = values[0];
            foreach (int value in values)
            {
                if (value != equalValue)
                {
                    return 0;
                }
            }

            return 1;
        }

        /// <summary>
        /// "Ors" the given values
        /// </summary>
        /// <param name="values">The values to "Or"</param>
        /// <returns>Returns the "Or'ed" values</returns>
        private int Or(int[] values)
        {
            foreach (int value in values)
            {
                if (value == 1)
                {
                    return 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// "XOrs" the given values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private int XOr(int[] values)
        {
            int count = 0;
            foreach (int value in values)
            {
                count += value;
            }
            if(count%2 != 0)
            {
                return 1;
            }
            return 0;
        }
    }
}
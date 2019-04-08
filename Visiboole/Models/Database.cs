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
using System.Text.RegularExpressions;
using VisiBoole.ParsingEngine.ObjectCode;
using System.Linq;
using VisiBoole.ParsingEngine.Statements;

namespace VisiBoole.ParsingEngine
{
    /// <summary>
    /// The database containing useful data that is parsed by the parsing engine along with their corresponding accessor methods
    /// </summary>
	public class Database
	{
        /// <summary>
        /// All independent variables parsed by the parsing engine
        /// </summary>
		private Dictionary<string, IndependentVariable> IndVars;

        /// <summary>
        /// All dependent variables parsed by the parsing engine
        /// </summary>
        private Dictionary<string, DependentVariable> DepVars;

	    /// <summary>
	    /// All variables parsed by the parsing engine
	    /// </summary>
        public Dictionary<string, Variable> AllVars;

        /// <summary>
        /// List of variable namespaces.
        /// </summary>
        private Dictionary<string, List<string>> Namespaces;

        /// <summary>
        /// Dictionary of the expressions in the design.
        /// </summary>
        private Dictionary<int, ExpressionStatement> Expressions;

        /// <summary>
        /// Index of the last expression in the design.
        /// </summary>
        private int LastExpressionIndex;

	    #region Accessor methods

        public Database()
        {
            IndVars = new Dictionary<string, IndependentVariable>();
            DepVars = new Dictionary<string, DependentVariable>();
            AllVars = new Dictionary<string, Variable>();
            Namespaces = new Dictionary<string, List<string>>();
            Expressions = new Dictionary<int, ExpressionStatement>();
            LastExpressionIndex = -1;
        }

        /// <summary>
        /// Checks whether a variable namespace can be created.
        /// </summary>
        /// <param name="name">Namespace of the variable</param>
        /// <returns>Whether the namespace already exists or not</returns>
        public bool HasNamespace(string name)
        {
            return Namespaces.ContainsKey(name);
        }

        private string PadNumbers(string input)
        {
            return Regex.Replace(input, @"\d+", match => match.Value.PadLeft(2, '0'));
        }

        /// <summary>
        /// Adds a variable namespace that doesn't already exist or appends new components to the existing namespace.
        /// </summary>
        /// <param name="name">Namespace to create</param>
        /// <param name="components">Expanded components</param>
        public void AddNamespace(string name, IEnumerable<string> components)
        {
            if (!HasNamespace(name))
            {
                // Add Namespace and its values to the dictionary
                if (components != null)
                {
                    Namespaces.Add(name, components.ToList());
                }
                else
                {
                    Namespaces.Add(name, null);
                }
            }
            else
            {
                // Check to add new components
                bool wasComponentAdded = false;
                foreach (string component in components)
                {
                    if (!Namespaces[name].Contains(component))
                    {
                        Namespaces[name].Add(component);
                        wasComponentAdded = true;
                    }
                }

                // Order components from MSB to LSB
                if (wasComponentAdded)
                {
                    Namespaces[name] = Namespaces[name].OrderByDescending(b => PadNumbers(b)).ToList();

                    wasComponentAdded = false;
                    for (int i = 0; i < Namespaces[name].Count; i++)
                    {
                        if (i < Namespaces[name].Count - 1)
                        {
                            Match currentMatch = Parser.ScalarRegex.Match(Namespaces[name][i]);
                            Match nextMatch = Parser.ScalarRegex.Match(Namespaces[name][i+1]);

                            int currentBit = Convert.ToInt32(currentMatch.Groups["Bit"].Value);
                            int nextBit = Convert.ToInt32(nextMatch.Groups["Bit"].Value);

                            // Check to add missing bits
                            if (currentBit - 1 != nextBit)
                            {
                                for (int newBit = currentBit - 1; newBit > nextBit; newBit--)
                                {
                                    Namespaces[name].Add(String.Concat(name, newBit));
                                }
                                wasComponentAdded = true;
                            }
                        }
                    }

                    // Resort components
                    if (wasComponentAdded)
                    {
                        Namespaces[name] = Namespaces[name].OrderByDescending(b => PadNumbers(b)).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of components for the specified namespace.
        /// </summary>
        /// <param name="name">Namespace of the variable</param>
        /// <returns>List of components that belong to the namespace</returns>
        public List<string> GetComponents(string name)
        {
            if (HasNamespace(name))
            {
                return Namespaces[name];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Adds a variable to the collection of variables of the given type
        /// </summary>
        /// <typeparam name="T">The type matching the target collection of variables</typeparam>
        /// <param name="v">The variable to add to the collection of matching type</param>
        /// <returns>Returns true if the variable was successfully added</returns>
        public bool AddVariable<T>(T v)
        {
            Type varType = typeof(T);
            if (varType == typeof(IndependentVariable))
            {
                IndependentVariable iv = (IndependentVariable)Convert.ChangeType(v, typeof(IndependentVariable));
                if (!IndVars.ContainsKey(iv.Name))
                {
                    IndVars.Add(iv.Name, iv);
                }
                if (!AllVars.ContainsKey(iv.Name))
                {
                    AllVars.Add(iv.Name, iv);
                }
            }
            else
            {
                DependentVariable dv = (DependentVariable)Convert.ChangeType(v, typeof(DependentVariable));

                if (!DepVars.ContainsKey(dv.Name))
                {
                    DepVars.Add(dv.Name, dv);
                }
                if (!AllVars.ContainsKey(dv.Name))
                {
                    AllVars.Add(dv.Name, dv);
                }
            }
            return true;
        }

        /// <summary>
        /// Fetches a variable from the collection of variables matching the given type
        /// </summary>
        /// <typeparam name="T">The type of the collection of variables to search</typeparam>
        /// <param name="name">The string representation of the given variable to search for</param>
        /// <returns>Returns the variable if it was found, else returns null</returns>
		public Variable TryGetVariable<T>(string name) where T : Variable
		{
			Type varType = typeof(T);
			if (varType == typeof(IndependentVariable))
			{
				if (IndVars.ContainsKey(name))
					return IndVars[name];
			}
			else if (varType == typeof(DependentVariable))
			{
				if (DepVars.ContainsKey(name))
					return DepVars[name];
			}
			else if (varType == typeof(Variable))
			{
				if (AllVars.ContainsKey(name))
					return AllVars[name];
			}
			return null;
		}

        /// <summary>
        /// Converts an independent variable to a dependent variable
        /// </summary>
        /// <param name="name">Variable to convert</param>
        public void MakeDependent(string name)
        {
            bool value = IndVars[name].Value;
            IndVars.Remove(name);
            AllVars.Remove(name);
            AddVariable(new DependentVariable(name, value));
        }

        /// <summary>
        /// Tries to get the value of a variable
        /// </summary>
        /// <param name="var">The name of a variable</param>
        /// <returns>The value or -1 for no value</returns>
        public int TryGetValue(string var)
        {
            IndependentVariable indVar = TryGetVariable<IndependentVariable>(var) as IndependentVariable;
            DependentVariable depVar = TryGetVariable<DependentVariable>(var) as DependentVariable;

            if (indVar != null)
            {
                if (indVar.Value) return 1;
                else return 0;
            }
            else if (depVar != null)
            {
                if (depVar.Value) return 1;
                else return 0;
            }
            else
            {
                return -1; // If variable doesn't exist
            }
        }

        #endregion

        /// <summary>
        /// Toggles the value of the given variable in its corresponding collections
        /// </summary>
        /// <param name="variableName">The name of the variable to search for</param>
        public void FlipValue(string variableName)
        {
            if(IndVars.ContainsKey(variableName))
            {
                IndVars[variableName].Value = !IndVars[variableName].Value;
            }
        }

        /// <summary>
        /// Sets specific value
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        public void SetValue(string variableName, bool value)
        {
            if (IndVars.ContainsKey(variableName))
            {
                IndVars[variableName].Value = value;
            }
            else if (DepVars.ContainsKey(variableName))
            {
                DepVars[variableName].Value = value;
            }
            else
            {
                IndependentVariable Ind = new IndependentVariable(variableName, value);
                IndVars.Add(variableName, Ind);
            }
        }

        /// <summary>
        /// Reevaluates all expressions till all variables are the correct value.
        /// </summary>
        public void ReevaluateExpressions()
        {
            List<int> noReevaluation = new List<int>();
            for (int i = 0; i <= LastExpressionIndex; i++)
            {
                if (Expressions.ContainsKey(i))
                {
                    if (!noReevaluation.Contains(i))
                    {
                        var expression = Expressions[i];
                        if (expression.Evaluate())
                        {
                            if (expression.Expression.Contains("==") || expression.Expression.Contains("+") || expression.Expression.Contains("-"))
                            {
                                noReevaluation.Add(i);
                            }
                            i = -1; // Reset loop if reevaluated
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates all expression values then adds the expression to the expressions dictionary.
        /// </summary>
        /// <param name="lineNumber">Line number of expression statement</param>
        /// <param name="expression">Expression to add</param>
        public void AddExpression(int lineNumber, ExpressionStatement expression)
        {
            ReevaluateExpressions();
            Expressions.Add(lineNumber, expression);
            if (lineNumber > LastExpressionIndex)
            {
                LastExpressionIndex = lineNumber;
            }
            //ReevaluateExpressions();
        }

        /// <summary>
        /// Returns the expression statement at the provided line number.
        /// </summary>
        /// <param name="lineNumber">Line number of the expression statement</param>
        /// <returns>Expression statement at the provided line number</returns>
        public ExpressionStatement GetExpression(int lineNumber)
        {
            return Expressions[lineNumber];
        }
    }
}
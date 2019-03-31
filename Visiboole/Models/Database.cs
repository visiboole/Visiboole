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
using VisiBoole.ParsingEngine.Boolean;
using System.Text.RegularExpressions;
using VisiBoole.ParsingEngine.ObjectCode;
using System.Linq;

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
        /// List of vector namespaces
        /// </summary>
        private Dictionary<string, List<string>> VectorNamespaces;

        // Dependencies - List of all variables in the expression that 
        //                relates to the dependent variable for the expression

        /// <summary>
        /// List of all variables in the expression that relates to the dependent variable for the expression
        /// </summary>
        public Dictionary<string, List<string>> Dependencies;

        /// <summary>
        /// expression that relates to the dependent variable for the expression
        /// </summary>
        public Dictionary<string, string> Expressions;

        /// <summary>
        /// list of "compiled" VisiBoole Object Code. Each item has text and value to be interpreted by the HTML parser
        /// </summary>
		private List<IObjectCodeElement> ObjectCode { get; set; }

	    #region Accessor methods

        public Dictionary<string, DependentVariable> GetDepVars()
        {
            return DepVars;
        }

        public Dictionary<string, IndependentVariable> GetIndVars()
        {
            return IndVars;
        }

        public Dictionary<string, List<string>> GetVectorNamespaces()
        {
            return VectorNamespaces;
        }

        public void SetOutput(List<IObjectCodeElement> list)
        {
            ObjectCode = list;
        }

        public List<IObjectCodeElement> GetOutput()
        {
            return ObjectCode;
        }

        public void SetValues(string variableName, bool value)
        {
            IndependentVariable indVar = Parser.Design.Database.TryGetVariable<IndependentVariable>(variableName) as IndependentVariable;
            DependentVariable depVar = Parser.Design.Database.TryGetVariable<DependentVariable>(variableName) as DependentVariable;
            if (indVar != null)
            {
                IndVars[variableName].Value = value;
            }
            else
            {
                DepVars[variableName].Value = value;
            }

            foreach (KeyValuePair<string,string> kv in Expressions.Reverse())
            {
                string dependent = kv.Key;
                string expression = kv.Value;
                foreach (Match match in Regex.Matches(expression, @"[_a-zA-Z]\w{0,19}"))
                {
                    if (match.Value.Equals(variableName))
                    {
                        bool dependentValue = ExpressionSolver.Solve(expression) == 1;
                        bool currentValue = TryGetValue(dependent) == 1;
                        if (dependentValue != currentValue)
                        {
                            SetValues(dependent, dependentValue);
                        }
                    }
                }
            }
        }

        public Database()
        {
            IndVars = new Dictionary<string, IndependentVariable>();
            DepVars = new Dictionary<string, DependentVariable>();
            AllVars = new Dictionary<string, Variable>();
            VectorNamespaces = new Dictionary<string, List<string>>();
            Dependencies = new Dictionary<string, List<string>>();
            Expressions = new Dictionary<string, string>();
        }

        /// <summary>
        /// Checks whether a vector namespace can be created.
        /// </summary>
        /// <param name="name">Namespace of the vector</param>
        /// <returns>Whether the namespace already exists or not</returns>
        public bool HasVectorNamespace(string name)
        {
            return VectorNamespaces.ContainsKey(name);
        }

        /// <summary>
        /// Adds a vector namespace that doesn't already exist.
        /// </summary>
        /// <param name="name">Namespace to create</param>
        /// <param name="components">Expanded vector</param>
        /// <returns>Whether the vector namespace was created</returns>
        public bool AddVectorNamespace(string name, List<string> components)
        {
            if (!HasVectorNamespace(name))
            {
                // Add Namespace and its values to the dictionary
                VectorNamespaces.Add(name, components);
                return true;
            }
            else
            {
                return false; // Namespace already exists
            }
        }

        /// <summary>
        /// Returns a list of components for the specified vector namespace.
        /// </summary>
        /// <param name="name">Namepsace of the vector</param>
        /// <returns>List of components that belong to the vector namespace</returns>
        public List<string> GetVectorComponents(string name)
        {
            if (HasVectorNamespace(name))
            {
                return VectorNamespaces[name];
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
            AddVariable<DependentVariable>(new DependentVariable(name, value));
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
            else return -1; // If variable doesn't exist
        }

        #endregion

        /// <summary>
        /// Toggles the value of the given variable in its corresponding collections
        /// </summary>
        /// <param name="variableName">The name of the variable to search for</param>
        public void VariableClicked(string variableName)
        {
            if(IndVars.ContainsKey(variableName))
            {
                if(IndVars[variableName].Value.Equals(true))
                {
                    IndVars[variableName].Value = false;
                    return;
                }
                else
                {
                    IndVars[variableName].Value = true;
                    return;
                }
            }
            if(DepVars.ContainsKey(variableName))
            {
                if (DepVars[variableName].Value.Equals(true))
                {
                    DepVars[variableName].Value = false;
                    return;
                }
                else
                {
                    DepVars[variableName].Value = true;
                    return;
                }
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
                return;
            }
            else if (DepVars.ContainsKey(variableName))
            {
                DepVars[variableName].Value = value;
                return;
            }
            else
            {
                IndependentVariable Ind = new IndependentVariable(variableName, value);
                IndVars.Add(variableName, Ind);
            }
        }

        /// <summary>
        /// Creates a list containing the expression associated with the dependent variable
        /// </summary>
        /// <param name="dependentName"></param>
        public void CreateDependenciesList(string dependentName)
        {
            if(!Dependencies.ContainsKey(dependentName))
            {
                Dependencies.Add(dependentName, new List<string>());
            }
        }

        /// <summary>
        /// Adds the given variable name to the list of dependencies it is associated with
        /// </summary>
        /// <param name="dependentName">The name of the dependent variable containing the expression</param>
        /// <param name="ExpressionVariableName">The name of the variable to add to the dependency list</param>
        public void AddDependencies(string dependentName, string ExpressionVariableName)
        {
            if(!Dependencies[dependentName].Contains(ExpressionVariableName))
            {
                Dependencies[dependentName].Add(ExpressionVariableName);
            }
        }

        /// <summary>
        /// Adds the expression to the collection of expressions associated with the given dependent variable
        /// </summary>
        /// <param name="dependentName">The name of the variable containing the expression</param>
        /// <param name="expressionValue">The expression to add to the collection of expressions associated with the given dependent variable</param>
        public void AddExpression(string dependentName, string expressionValue)
        {
            if (Expressions.ContainsKey(dependentName))
            {
                if (!Expressions[dependentName].Contains(expressionValue))
                {
                    Expressions[dependentName] = expressionValue;
                }
            }
            else
            {
                Expressions.Add(dependentName, expressionValue);
            }
        }
    }
}
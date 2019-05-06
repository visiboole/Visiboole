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
using VisiBoole.Models;
using System.Text;

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
        /// All module inputs of the design being parsed by the parsing engine.
        /// </summary>
        private List<string> Inputs;

        /// <summary>
        /// Dictionary of variable namespaces.
        /// </summary>
        private Dictionary<string, List<int>> Namespaces;

        /// <summary>
        /// List of the expressions in the design.
        /// </summary>
        private Dictionary<string, NamedExpression> Expressions;

        /// <summary>
        /// Dictionary of variable dependency lists.
        /// </summary>
        private Dictionary<string, List<string>> DependencyLists;

        /// <summary>
        /// Dictionary of alternating clocks in the design.
        /// </summary>
        public Dictionary<string, bool> AltClocks;

	    #region Accessor methods

        public Database()
        {
            IndVars = new Dictionary<string, IndependentVariable>();
            DepVars = new Dictionary<string, DependentVariable>();
            AllVars = new Dictionary<string, Variable>();
            Inputs = new List<string>();
            Namespaces = new Dictionary<string, List<int>>();
            Expressions = new Dictionary<string, NamedExpression>();
            DependencyLists = new Dictionary<string, List<string>>();
            AltClocks = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Returns whether the provided namespace exists.
        /// </summary>
        /// <param name="name">Namespace to check for existance</param>
        /// <returns>Whether the provided namespace exists</returns>
        public bool NamespaceExists(string name)
        {
            return Namespaces.ContainsKey(name);
        }

        /// <summary>
        /// Checks whether the provided namespace belongs to a vector.
        /// </summary>
        /// <param name="name">Namespace to check</param>
        /// <returns>Whether the provided namespace belongs to a vector</returns>
        public bool NamespaceBelongsToVector(string name)
        {
            return NamespaceExists(name) && Namespaces[name] != null;
        }

        /// <summary>
        /// Updates/adds the provided namespace with the provided/not provided bit.
        /// </summary>
        /// <param name="name">Namepsace to update/add</param>
        /// <param name="bit">Bit to add</param>
        public void UpdateNamespace(string name, int bit)
        {
            if (!NamespaceExists(name))
            {
                if (bit != -1)
                {
                    Namespaces.Add(name, new List<int>());
                }
                else
                {
                    Namespaces.Add(name, null);
                }
            }

            if (bit != -1 && !Namespaces[name].Contains(bit))
            {
                int componentCount = Namespaces[name].Count;
                if (componentCount == 0)
                {
                    Namespaces[name].Add(bit);
                }
                else
                {
                    int currentMaxBit = Namespaces[name][0];
                    if (bit > currentMaxBit)
                    {
                        for (int i = currentMaxBit + 1; i <= bit; i++)
                        {
                            Namespaces[name].Insert(0, i);
                        }
                    }
                    else
                    {
                        int currentMinBit = Namespaces[name][componentCount - 1];
                        for (int i = currentMinBit - 1; i >= bit; i--)
                        {
                            Namespaces[name].Add(i);
                        }
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
            if (NamespaceBelongsToVector(name))
            {
                List<string> components = new List<string>();
                foreach (int bit in Namespaces[name])
                {
                    components.Add($"{name}{bit}");
                }
                return components;
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
        public bool AddVariable<T>(T v, bool IsInput = false)
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
                if (IsInput && !Inputs.Contains(iv.Name))
                {
                    Inputs.Add(iv.Name);
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
        /// Attemps to convert an independent variable to a dependent variable.
        /// </summary>
        /// <param name="name">Variable to convert</param>
        /// <returns>Whether the conversion was successful</returns>
        public bool MakeDependent(string name)
        {
            if (Inputs.Contains(name))
            {
                return false;
            }

            bool value = IndVars[name].Value;
            IndVars.Remove(name);
            AllVars.Remove(name);
            AddVariable(new DependentVariable(name, value));
            return true;
        }

        /// <summary>
        /// Returns an array of variables from the provided token.
        /// </summary>
        /// <param name="token">String of variables</param>
        /// <returns>An array of variables</returns>
        public string[] GetVariables(string token)
        {
            if (!token.Contains("{"))
            {
                return new string[] { token };
            }
            else if (!token.Contains(".d"))
            {
                token = token.Substring(1, token.Length - 2);
                return Parser.WhitespaceRegex.Split(token);
            }
            else
            {
                token = token.Substring(1, token.Length - 4);
                string[] variables = Parser.WhitespaceRegex.Split(token);
                for (int i = 0; i < variables.Length; i++)
                {
                    variables[i] = variables[i] + ".d";
                }
                return variables;
            }
        }

        /// <summary>
        /// Returns the value of the provided token.
        /// </summary>
        /// <param name="token">String of variables</param>
        /// <returns>Value of the provided token</returns>
        public int GetValue(string token)
        {
            if (char.IsDigit(token[0]))
            {
                // Return int converted value of token
                return Convert.ToInt32(token);
            }
            else if (token[0] != '{')
            {
                IndependentVariable indVar = TryGetVariable<IndependentVariable>(token) as IndependentVariable;
                DependentVariable depVar = TryGetVariable<DependentVariable>(token) as DependentVariable;
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
            else
            {
                // Create binary string builder
                StringBuilder binary = new StringBuilder();
                // For each variable in the token
                foreach (string var in GetVariables(token))
                {
                    // Add variable's value to binary string
                    binary.Append(GetValue(var));
                }
                // Return int converted binary string as value
                return Convert.ToInt32(binary.ToString(), 2);
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
                SetValue(variableName, !IndVars[variableName].Value);
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
        }

        /// <summary>
        /// Sets each binary value to the corresponding variable.
        /// </summary>
        /// <param name="variables">Variables to set</param>
        /// <param name="binary">Values to set</param>
        public bool SetValues(IList<string> variables, string binary)
        {
            bool valueChanged = false;
            for (int i = 0; i < binary.Length; i++)
            {
                if (GetValue(variables[i]) != char.GetNumericValue(binary[i]))
                {
                    SetValue(variables[i], binary[i] == '1');
                    valueChanged = true;
                }
            }
            return valueChanged;
        }

        /// <summary>
        /// Updates all expression values then adds the expression to the expressions dictionary.
        /// </summary>
        /// <param name="expression">Expression to add</param>
        public void AddExpression(NamedExpression expression, string clock = null)
        {
            // Add expression to expressions list
            foreach (string dependent in expression.Dependents)
            {
                Expressions.Add(dependent, expression);
            }

            // If expression has an alternate clock and that alternate clock isn't in the alternate clocks dictionary
            if (clock != null && !AltClocks.ContainsKey(clock))
            {
                // Add new alternate clock to alternate clocks dictionary
                AltClocks.Add(clock, GetValue(clock) == 1);
            }
        }

        public void ProcessUpdate(List<string> variables)
        {
            bool varChanged;
            do
            {
                varChanged = false;
                foreach (string dependent in DependencyLists.Keys)
                {
                    if (DependencyLists[dependent].Intersect(variables).Any())
                    {
                        if (Expressions[dependent].Evaluate())
                        {
                            if (varChanged == false)
                            {
                                varChanged = true;
                            }
                        }
                    }
                }
            } while (varChanged);
        }

        /// <summary>
        /// Update all alternate clock previous values.
        /// </summary>
        public void UpdateAltClocks()
        {
            // Update alt clock values
            foreach (string altClock in AltClocks.Keys)
            {
                AltClocks[altClock] = GetValue(altClock) == 1;
            }
        }

        /// <summary>
        /// Returns whether the provided variable has an existing dependency list.
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns>Whether the provided variable has an existing dependency list</returns>
        public bool HasDependencyList(string variable)
        {
            return DependencyLists.ContainsKey(variable);
        }

        /// <summary>
        /// Tries to add a dependent and its dependencies to the database.
        /// </summary>
        /// <param name="dependent">Dependent</param>
        /// <param name="dependencyList">Dependent's dependencies</param>
        /// <returns>Whether the dependent and its dependencies were added to the database</returns>
        public bool TryAddDependencyList(string dependent, List<string> dependencyList)
        {
            List<string> variablesToRemove = new List<string>();
            List<string> variablesToAdd = new List<string>();
            // Iterate through the dependency list
            foreach (string variable in dependencyList)
            {
                // If varaible in the dependency list has dependencies
                if (HasDependencyList(variable))
                {
                    // Get variable's dependency list
                    List<string> additionalDependencyList = DependencyLists[variable];
                    // If variable's dependency list contains the dependent
                    if (additionalDependencyList.Contains(dependent))
                    {
                        // Return false (cycle)
                        return false;
                    }

                    // Add variable to removal list
                    variablesToRemove.Add(variable);
                    // Add new variables to addition list
                    variablesToAdd.AddNew(additionalDependencyList);
                }
            }

            foreach (string variable in variablesToRemove)
            {
                dependencyList.Remove(variable);
            }
            dependencyList.AddNew(variablesToAdd);

            foreach (KeyValuePair<string, List<string>> dependecy in DependencyLists)
            {
                if (dependecy.Value.Contains(dependent))
                {
                    if (dependencyList.Contains(dependecy.Key))
                    {
                        return false;
                    }

                    // Remove new dependent from existing dependency list
                    dependecy.Value.Remove(dependent);
                    // Add dependent's dependencies to the existing dependency
                    dependecy.Value.AddNew(dependencyList);
                }
            }

            DependencyLists.Add(dependent, dependencyList);
            return true;
        }
    }
}
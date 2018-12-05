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

        public void SetDepVar(string name, bool value)
        {
            DepVars[name].Value = value;
            foreach (KeyValuePair<string,string> kv in Expressions.Reverse())
            {
                string dependent = kv.Key;
                string expression = kv.Value;
                foreach (Match match in Regex.Matches(expression, Globals.PatternVariable))
                {
                    if (match.Value.Equals(name))
                    {
                        Expression exp = new Expression();
                        bool dependentValue = exp.Solve(expression);
                        bool currentValue = TryGetValue(dependent) == 1;
                        if (dependentValue != currentValue)
                        {
                            SetDepVar(dependent, dependentValue);
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
        /// Checks whether a vector namespace can be created
        /// </summary>
        /// <param name="name">Namespace to create</param>
        /// <param name="expanded">Expanded vector</param>
        /// <returns>Whether the vector namespace was created</returns>
        public bool AddVectorNamespace(string name, string expanded)
        {
            if (!VectorNamespaces.ContainsKey(name))
            {
                /* Add Namespace and its values to the dictionary */
                VectorNamespaces.Add(name, new List<string>(expanded.Split(' ')));
                return true;
            }
            else
            {
                return false; // Namespace already exists
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

        /// <summary>
        /// Gets the debugger information for the variables in this database.
        /// </summary>
        /// <returns>String containing the information</returns>
        public string GetVariableDebugInformation()
        {
            string debugInfo = String.Empty;

            debugInfo += String.Concat("Independent Variables:", Environment.NewLine);
            foreach (KeyValuePair<string, IndependentVariable> kv in IndVars)
            {
                debugInfo += String.Concat(kv.Key, ",", kv.Value.Value, Environment.NewLine);
            }
            debugInfo += String.Concat(Environment.NewLine, "Dependent Variables:", Environment.NewLine);
            foreach (KeyValuePair<string, DependentVariable> kv in DepVars)
            {
                if (Expressions.ContainsKey(kv.Key))
                {
                    debugInfo += String.Concat(kv.Key, ",", kv.Value.Value, ",", Expressions[kv.Key], Environment.NewLine);
                }
                else
                {
                    debugInfo += String.Concat(kv.Key, ",", kv.Value.Value, Environment.NewLine);
                }
            }

            return debugInfo;
        }
    }
}
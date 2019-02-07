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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VisiBoole.ErrorHandling;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.ParsingEngine.Statements;

namespace VisiBoole.ParsingEngine
{
    /// <summary>
    /// The main class of the parsing engine. This class is the brains of the parsing engine and 
    /// communicates with the calling classes.
    /// </summary>
	public class Parser
	{
        private static Regex RegexExpansion = new Regex(Globals.PatternAnyVectorType);

        /// <summary>
        /// The entry method of the parsing engine. This method acts as "main" for the parsing engine.
        /// </summary>
        /// <param name="sd">The subdesign containing the text to parse</param>
        /// <param name="variableName">The clicked variable if it exists, else the empty string</param>
        /// <returns>Returns a list of parsed elements containing the text and value of each unit in the given expression</returns>
		public List<IObjectCodeElement> Parse(SubDesign sd, string variableName, bool tick)
		{
            //initial run
            if(string.IsNullOrEmpty(variableName) && tick.Equals(false))
            {
                sd.Database = new Database();
                string expandedSourceCode = GetExpandedSourceCode(sd, true);
                if (expandedSourceCode == null)
                {
                    return null;
                }
                List<Statement> stmtList = ParseStatements(expandedSourceCode, false, true);
                if(stmtList == null)
                {
                    return null;
                }
                foreach (Statement stmt in stmtList)
                {
                    stmt.Parse();
                }
                List<IObjectCodeElement> output = new List<IObjectCodeElement>();
                foreach (Statement stmt in stmtList)
                {
                    output.AddRange(stmt.Output);
                }
                return output;
            }
            //variable clicked
			else if(!string.IsNullOrEmpty(variableName) && tick.Equals(false))
            {
                string expandedSourceCode = GetExpandedSourceCode(sd, false);
                if (expandedSourceCode == null)
                {
                    return null;
                }
                sd.Database.VariableClicked(variableName);
                List<Statement> stmtList = ParseStatements(expandedSourceCode, false, false);
                if (stmtList == null)
                {
                    return null;
                }
                foreach (Statement stmt in stmtList)
                {
                    stmt.Parse();
                }
                List<IObjectCodeElement> output = new List<IObjectCodeElement>();
                foreach (Statement stmt in stmtList)
                {
                    output.AddRange(stmt.Output);
                }
                return output;
            }
            //clock tick
            else
            {
                string expandedSourceCode = GetExpandedSourceCode(sd, false);
                if (expandedSourceCode == null)
                {
                    return null;
                }
                List<Statement> stmtList = ParseStatements(expandedSourceCode, true, false);
                // Set delay values
                foreach (Statement stmt in stmtList)
                {
                    if (stmt.GetType() == typeof(DffClockStmt))
                    {
                        ((DffClockStmt)stmt).Tick();
                    }
                }
                foreach (Statement stmt in stmtList)
                {
                    if (stmt.GetType() == typeof(BooleanAssignmentStmt))
                    {
                        ((BooleanAssignmentStmt)stmt).Evaluate();
                    }
                }
                /*
                foreach (Statement stmt in stmtList)
                {
                    if (stmt.GetType() != typeof(DffClockStmt))
                    {
                        stmt.Parse();
                    }
                }
                foreach (Statement stmt in stmtList)
                {
                    if (stmt.GetType() == typeof(DffClockStmt))
                    {
                        stmt.Parse();
                    }
                }
                */
                foreach (Statement stmt in stmtList)
                {
                    stmt.Parse();
                }
                List<IObjectCodeElement> output = new List<IObjectCodeElement>();
                foreach (Statement stmt in stmtList)
                {
                    output.AddRange(stmt.Output);
                }
                return output;
            }
		}

        /// <summary>
        /// Expands the source of a given SubDesign, intializes variables and performs erorr checking
        /// </summary>
        /// <param name="sd">Subdesign to create expanded source code for</param>
        /// <param name="init">Whether variables need to be initialized</param>
        /// <returns>The expanded source code</returns>
        private string GetExpandedSourceCode(SubDesign sd, bool init)
        {
            string expandedSourceCode = String.Empty;
            byte[] byteArr = Encoding.UTF8.GetBytes(sd.Text);
            MemoryStream stream = new MemoryStream(byteArr);
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                int lineNum = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNum++;

                    if (String.IsNullOrEmpty(line.Trim()))
                    {
                        expandedSourceCode += String.Concat(line, "\n");
                        continue;
                    }

                    line = line.Replace("\t", "            ");

                    if (CommentStmt.Regex.Match(line).Success)
                    {
                        Match match = CommentStmt.Regex.Match(line);
                        if (match.Groups["DoInclude"].Value.Equals("+"))
                        {
                            expandedSourceCode += String.Concat(match.Groups["Spacing"].Value, match.Groups["Comment"].Value, "\n");
                            continue;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // Check for ;
                    if (!line.Contains(";"))
                    {
                        MessageBox.Show("Missing ';'. Line: " + lineNum, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                    // Check for matching (), [] and {}
                    Stack<char> stack = new Stack<char>();
                    foreach (char c in line)
                    {
                        if (c == '(' || c == '[' || c == '{')
                        {
                            stack.Push(c);
                        }
                        if (c == ')' || c == ']' || c == '}')
                        {
                            if (stack.Count > 0)
                            {
                                char c2 = stack.Peek();
                                if ((c == ')' && c2 == '(') || (c == ']' && c2 == '[') || (c == '}' && c2 == '{'))
                                {
                                    stack.Pop();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Unmatching '" + c + "'. Line: " + lineNum, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return null;
                            }
                        }
                    }
                    if (stack.Count > 0)
                    {
                        MessageBox.Show("Unmatching '" + stack.Peek() + "'. Line: " + lineNum, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                    // Perform expansions and error checking
                    if (RegexExpansion.IsMatch(line) && line.Contains("="))
                    {
                        if (line.Contains("*"))
                        {
                            MessageBox.Show("You cannot use a '*' with a variable in an assignment statement. Line: " + lineNum, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }

                        string expansion = ExpandVertically(line);

                        if (expansion != null)
                        {
                            /*
                            MatchCollection matches = Regex.Matches(line, Globals.PatternAnyVectorType);
                            foreach (Match match in matches)
                            {
                                if (!sd.Database.AddVectorNamespace(match.Groups["Name"].Value, ExpandHorizontally(match)))
                                {
                                    MessageBox.Show("Vector Namespace " + match.Groups["Name"].Value + " already exists. Line: " + lineNum, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return null;
                                }
                            }
                            */

                            if (init && !InitVariables(sd, expansion))
                            {
                                return null;
                            }
                            expandedSourceCode += String.Concat(expansion, "\n");
                        }
                        else
                        {
                            MessageBox.Show("Vector and/or Concatenation element counts must be consistent across entire expression. Line: " + lineNum, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                    }
                    else if (RegexExpansion.IsMatch(line))
                    {
                        string output = line;
                        while (RegexExpansion.IsMatch(output))
                        {
                            Match match = RegexExpansion.Matches(output)[0]; // Get match
                            string expanded = ExpandHorizontally(match);
                            output = output.Substring(0, match.Index) + expanded + output.Substring(match.Index + match.Length);
                            sd.Database.AddVectorNamespace(match.Groups["Name"].Value, expanded);
                            /*
                            if (!sd.Database.AddVectorNamespace(match.Groups["Name"].Value, expanded))
                            {
                                MessageBox.Show("Vector Namespace " + match.Groups["Name"].Value + " already exists. Line: " + lineNum, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return null;
                            }
                            */
                        }

                        if (init && !InitVariables(sd, output))
                        {
                            return null;
                        }
                        expandedSourceCode += String.Concat(output, "\n");
                    }
                    else
                    {
                        if (init && !InitVariables(sd, line))
                        {
                            return null;
                        }
                        expandedSourceCode += String.Concat(line, "\n");
                    }
                }
            }

            return expandedSourceCode;
        }

		/// <summary>
		/// Parses the source code into discrete statements of their respective visiboole type
		/// </summary>
		/// <param name="sd">The subdesign containing the user source code to be parsed</param>
		/// <returns>Returns a list of visiboole statements, indexed by line number</returns>
		private List<Statement> ParseStatements(string sourceCode, bool tick, bool init)
		{
			List<Statement> stmtList = new List<Statement>();
			byte[] byteArr = Encoding.UTF8.GetBytes(sourceCode);
			MemoryStream stream = new MemoryStream(byteArr);
            using (StreamReader reader = new StreamReader(stream))
            {
                string line; // Current line
                int lineNum = 0;
                bool flag = false;    // flag is set to true after the first non-empty/comment is found
                while ((line = reader.ReadLine()) != null)
                {
                    // check for an empty statement
                    if (string.IsNullOrEmpty(line.Trim()))
                    {
                        stmtList.Add(new EmptyStmt(lineNum, line));
                        lineNum++;
                        continue;
                    }

                    // check for a comment
                    bool success = CommentStmt.Regex.Match(line).Success;
                    if (success)
                    {
                        stmtList.Add(new CommentStmt(lineNum, line));
                        lineNum++;
                        continue;
                    }

                    // check for a format specifier statement
                    success = FormatSpecifierStmt.Regex.Match(line).Success;
                    if (success)
                    {
                        stmtList.Add(new FormatSpecifierStmt(lineNum, line));
                        flag = true;
                        lineNum++;
                        continue;
                    }

                    // check for a variable list statement
                    success = VariableListStmt.Regex.Match(line).Success;
                    if (success)
                    {
                        stmtList.Add(new VariableListStmt(lineNum, line));
                        flag = true;
                        lineNum++;
                        continue;
                    }

                    /*
                    success = ConstantStmt.BinPattern.Match(line).Success || ConstantStmt.DecPattern.Match(line).Success || ConstantStmt.HexPattern.Match(line).Success;
                    if (success)
                    {
                        List<string> lines = ExpandLine(line);
                        foreach (string line in lines)
                        {
                            ConstantStmt stmt = new ConstantStmt(postLnNum++, line);

                            if (stmt.VariableStmt != null)
                            {
                                stmtList.Add(stmt.VariableStmt);
                            }
                            else return null;
                        }
                            
                        flag = true;
                        
                        
                        continue;
                    }
                    */

                    // check for a module declaration statement
                    success = ModuleDeclarationStmt.Regex.Match(line).Success;
                    if (flag == false && success)
                    {
                        stmtList.Add(new ModuleDeclarationStmt(lineNum, line));
                        flag = true;
                        lineNum++;
                        continue;
                    }

                    // check for a submodule instantiation statement
                    success = SubmoduleInstantiationStmt.Regex.Match(line).Success;
                    if (success)
                    {
                        stmtList.Add(new SubmoduleInstantiationStmt(lineNum, line));
                        flag = true;
                        lineNum++;
                        continue;
                    }

                    // Check for clock statement
                    if (line.Contains("<"))
                    {
                        stmtList.Add(new DffClockStmt(lineNum, line, tick, init));
                        flag = true;
                        lineNum++;
                        continue;
                    }

                    // Check for boolean statement
                    if (!line.Contains("<") || line.Contains("^"))
                    {
                        stmtList.Add(new BooleanAssignmentStmt(lineNum, line));
                        flag = true;
                        lineNum++;
                        continue;
                    }

                    // if we have reached this point with no match then there is a user syntax error
                    // TODO: add more validation checks for augmented error-checking granularity
                    success = ModuleDeclarationStmt.Regex.Match(line).Success;
					if (flag == true && success)
						// module declaration must be on the first line, throw an exception
						throw new ModuleDeclarationPlacementException("Module declarations must be at the top of the file. Did you mean to use a submodule declaration instead?");
					// we are past specific error checks - throw a general exception stating the given statement is unrecognized
					throw new StatementNotRecognizedException("Statement not recognized as visiboole source code.");
				}
			}
			return stmtList;
		}

        /// <summary>
        /// Expands vector to its variable list
        /// </summary>
        /// <param name="match">The Vector Match</param>
        /// <returns>The expanded string</returns>
        private string ExpandHorizontally(Match match)
        {
            string expanded = String.Empty;

            // Get vector name, bounds and step
            string name = match.Groups["Name"].Value;
            int leftBound = Convert.ToInt32(match.Groups["LeftBound"].Value);
            int rightBound = Convert.ToInt32(match.Groups["RightBound"].Value);
            int step = (leftBound < rightBound)
                    ? (String.IsNullOrEmpty(match.Groups["Step"].Value) ? 1 : Convert.ToInt32(match.Groups["Step"].Value))
                    : (String.IsNullOrEmpty(match.Groups["Step"].Value) ? -1 : (Convert.ToInt32(match.Groups["Step"].Value) * -1));

            // Expand vector
            for (int i = leftBound; i != rightBound; i+=step)
            {
                expanded += String.Concat(name, i, " ");
            }
            expanded += String.Concat(name, rightBound);

            return expanded;
        }

        /// <summary>
        /// Expands line into lines
        /// </summary>
        /// <param name="line">Line to expand</param>
        /// <returns>Expanded line</returns>
        private string ExpandVertically(string line)
        {
            string expanded = String.Empty;

            Regex regex = new Regex (
                @"("                                            // Begin Group
                    + Globals.PatternAnyVectorType              // Any Vector Type
                    + @"(?![^{}]*\})"                           // Not Inside {}
                + @")"                                          // End Group
                + @"|"                                          // Or
                + @"("                                          // Begin Group
                    + @"\{"                                     // {
                    + Globals.PatternAnyVariableType            // Any Variable Type
                    + @"("                                      // Begin Optional Group
                        + @"\,\s*"                              // Comma & Any Number of Whitespace
                        + Globals.PatternAnyVariableType        // Any Variable Type
                    + @")*"                                     // End Optional Group
                    + @"\}"                                     // }
                + @")"                                          // End Group
                + @"|"                                          // Or
                + Globals.PatternConstant                       // Constant
            );
            MatchCollection matches = regex.Matches(line);

            // Expand all variables
            List<List<string>> variables = new List<List<string>>();
            foreach (Match match in matches)
            {
                if (!match.Value.Contains("{"))
                {
                    variables.Add(new List<string>(ExpandHorizontally(match).Split(' ')));
                }
                else
                {
                    // Get concat and split into vars
                    string concat = Regex.Replace(match.Value, @"[{\s*}]", string.Empty);
                    string[] vars = concat.Split(','); // Split variables by commas

                    List<string> concatVars = new List<string>();
                    foreach (string var in vars)
                    {
                        if (Regex.IsMatch(var, Globals.PatternAnyVectorType))
                        {
                            string expansion = ExpandHorizontally(Regex.Match(var, Globals.PatternAnyVectorType));
                            foreach (string v in expansion.Split(' '))
                            {
                                concatVars.Add(v);
                            }
                        }
                        else
                        {
                            concatVars.Add(var);
                        }
                    }

                    variables.Add(concatVars);
                }
            }

            // Error checking
            foreach (List<string> list in variables)
            {
                if (list.Count != variables[0].Count)
                {
                    return null;
                }
            }

            // Expand lines 
            for (int i = 0; i < variables[0].Count; i++)
            {
                string newLine = line;
                int j = 0;
                foreach (Match match in matches)
                {
                    newLine = newLine.Replace(match.Value, variables[j++][i]);
                }
                expanded += String.Concat(newLine, "\n");
            }
            return expanded;
        }

        /// <summary>
        /// Initializes variables inside source line(s)
        /// </summary>
        /// <param name="sd">Subdesign to initialize variables</param>
        /// <param name="source">Source line(s)</param>
        /// <returns>Whether the operation was successful</returns>
        private bool InitVariables(SubDesign sd, string source)
        {
            string[] lines = source.Split('\n');
            foreach (string line in lines)
            {
                MatchCollection matches = Regex.Matches(line, Globals.PatternVariable);
                foreach (Match match in matches)
                {
                    string var = match.Value;
                    if (line.Contains("="))
                    {
                        string dependent = line.Contains("<")
                            ? line.Substring(0, line.IndexOf('<')).Trim()
                            : line.Substring(0, line.IndexOf('=')).Trim();

                        if (dependent.Equals(var))
                        {
                            if (line.Contains("<"))
                            {
                                var += ".d";
                            }

                            if (sd.Database.TryGetVariable<Variable>(var) == null)
                            {
                                DependentVariable depVar = new DependentVariable(var, false);
                                sd.Database.AddVariable<DependentVariable>(depVar);
                            }
                            else
                            {
                                if (sd.Database.TryGetVariable<IndependentVariable>(var) as IndependentVariable != null)
                                {
                                    sd.Database.MakeDependent(var);
                                }
                            }
                        }
                        else
                        {
                            if (sd.Database.TryGetVariable<Variable>(var) == null)
                            {
                                IndependentVariable indVar = new IndependentVariable(var, false);
                                sd.Database.AddVariable<IndependentVariable>(indVar);
                            }
                        }
                    }
                    else
                    {
                        if (sd.Database.TryGetVariable<Variable>(var) == null)
                        {
                            // Create Variable
                            bool val = var.Contains("*");
                            if (val && FormatSpecifierStmt.Regex.IsMatch(line))
                            {
                                return false; // You cannot use * in a FormatSpecifier
                            }
                            var = val ? var.Substring(1) : var; // Remove * if present
                            IndependentVariable indVar = new IndependentVariable(var, val);
                            sd.Database.AddVariable<IndependentVariable>(indVar);
                        }
                        else
                        {
                            if (VariableListStmt.Regex.IsMatch(line))
                            {
                                return false; // You cannot declare a variable twice
                            }
                        }
                    }

                }
            }

            return true;
        }
    }
}
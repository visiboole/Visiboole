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
                List<Statement> stmtList = ParseStatements(sd, false, true);
                if(stmtList == null)
                {
                    return null;
                }
                foreach (Statement stmt in stmtList)
                {
                    stmt.Parse();
                }

                List<string> vars = new List<string>();
                foreach (KeyValuePair<string, IndependentVariable> kv in sd.Database.GetIndVars())
                {
                    foreach (KeyValuePair<string, DependentVariable> kv1 in sd.Database.GetDepVars())
                    {
                        if (kv.Key.Equals(kv1.Key))
                        {
                            vars.Add(kv.Key + ": IValue = " + kv.Value.Value + " DValue = " + kv1.Value.Value);
                        }
                    }
                }

                using (StreamWriter sw = new StreamWriter(@"U:\Documents\vars.txt"))
                {
                    foreach (string s in vars)
                    {
                        sw.WriteLine(s);
                    }
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
                sd.Database.VariableClicked(variableName);
                List<Statement> stmtList = ParseStatements(sd, false, false);
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
                List<Statement> stmtList = ParseStatements(sd, true, false);
                foreach (Statement stmt in stmtList)
                {
                    if (stmt.GetType() == typeof(DffClockStmt))
                    {
                        stmt.Parse();
                    }
                }
                foreach (Statement stmt in stmtList)
                {
                    if (stmt.GetType() != typeof(DffClockStmt))
                    {
                        stmt.Parse();
                    }
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
		/// Parses the source code into discrete statements of their respective visiboole type
		/// </summary>
		/// <param name="sd">The subdesign containing the user source code to be parsed</param>
		/// <returns>Returns a list of visiboole statements, indexed by line number</returns>
		private List<Statement> ParseStatements(SubDesign sd, bool tick, bool init)
		{
			List<Statement> stmtList = new List<Statement>();
			string txt = sd.Text;
			byte[] byteArr = Encoding.UTF8.GetBytes(txt);
			MemoryStream stream = new MemoryStream(byteArr);
            using (StreamReader reader = new StreamReader(stream))
            {
                string nextLine;
                int preLnNum = 0;     // the line number in edit mode, before the text is parsed
                int postLnNum = 0;    // the line number in simulation mode, after the text is parsed
                bool flag = false;    // flag is set to true after the first non-empty/comment is found
                while ((nextLine = reader.ReadLine()) != null)
                {
                    nextLine = nextLine.TrimEnd();

                    // check for an empty statement
                    if (string.IsNullOrEmpty(nextLine.Trim()))
                    {
                        stmtList.Add(new EmptyStmt(postLnNum, nextLine));
                        preLnNum++;
                        postLnNum++;
                        continue;
                    }

                    // check for a comment
                    bool success = CommentStmt.Regex.Match(nextLine).Success;
                    if (success)
                    {
                        stmtList.Add(new CommentStmt(postLnNum, nextLine));
                        preLnNum++;
                        postLnNum++;
                        continue;
                    }

                    if (!nextLine.Contains(";"))
                    {
                        MessageBox.Show("Missing ';'. Line: " + (postLnNum + 1), "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                    /* Check for matching (), [] and {} */
                    Stack<char> stack = new Stack<char>();
                    foreach (char c in nextLine)
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
                                MessageBox.Show("Unmatching '" + c + "'. Line: " + (postLnNum + 1), "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return null;
                            }
                        }
                    }
                    if (stack.Count > 0)
                    {
                        MessageBox.Show("Unmatching '" + stack.Peek() + "'. Line: " + (postLnNum + 1), "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                    // check for a format specifier statement
                    success = FormatSpecifierStmt.Regex.Match(nextLine).Success;
                    if (success)
                    {
                        stmtList.Add(new FormatSpecifierStmt(postLnNum, nextLine));
                        flag = true;
                        preLnNum++;
                        postLnNum++;
                        continue;
                    }

                    // check for a variable list statement
                    success = VariableListStmt.Regex.Match(nextLine).Success;
                    if (success)
                    {
                        stmtList.Add(new VariableListStmt(postLnNum, nextLine));
                        flag = true;
                        preLnNum++;
                        postLnNum++;
                        continue;
                    }

                    success = ConstantStmt.BinPattern.Match(nextLine).Success || ConstantStmt.DecPattern.Match(nextLine).Success || ConstantStmt.HexPattern.Match(nextLine).Success;
                    if (success)
                    {
                        List<string> lines = ExpandLine(nextLine);
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
                        preLnNum++;
                        continue;
                    }

                    // check for a module declaration statement
                    success = ModuleDeclarationStmt.Regex.Match(nextLine).Success;
                    if (flag == false && success)
                    {
                        stmtList.Add(new ModuleDeclarationStmt(postLnNum, nextLine));
                        flag = true;
                        preLnNum++;
                        postLnNum++;
                        continue;
                    }

                    // check for a submodule instantiation statement
                    success = SubmoduleInstantiationStmt.Regex.Match(nextLine).Success;
                    if (success)
                    {
                        stmtList.Add(new SubmoduleInstantiationStmt(postLnNum, nextLine));
                        flag = true;
                        preLnNum++;
                        postLnNum++;
                        continue;
                    }

                    /* Check for clock statement */
                    if (nextLine.Contains("<"))
                    {
                        List<string> lines;
                        if (nextLine.Contains("[") || nextLine.Contains("{"))
                        {
                            lines = ExpandLine(nextLine);
                            if (lines == null)
                            {
                                MessageBox.Show("Inconsistent number of variables. Line: " + (postLnNum + 1), "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return null;
                            }
                        }
                        else
                        {
                            lines = new List<string>();
                            lines.Add(nextLine);
                        }

                        foreach (string line in lines)
                        {
                            stmtList.Add(new DffClockStmt(postLnNum++, line, tick, init));
                        }

                        flag = true;
                        preLnNum++;
                        continue;
                    }

                    /* Check for boolean statement */
                    if (!nextLine.Contains("<") || nextLine.Contains("^"))
                    {
                        List<string> lines;
                        if (nextLine.Contains("[") || nextLine.Contains("{"))
                        {
                            lines = ExpandLine(nextLine);
                            if (lines == null)
                            {
                                MessageBox.Show("Inconsistent number of variables. Line: " + (postLnNum + 1), "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return null;
                            }
                        }
                        else
                        {
                            lines = new List<string>();
                            lines.Add(nextLine);
                        }

                        foreach (string line in lines)
                        {
                            stmtList.Add(new BooleanAssignmentStmt(postLnNum++, line));
                        }

                        flag = true;
                        preLnNum++;
                        continue;
                    }

                    // if we have reached this point with no match then there is a user syntax error
                    // TODO: add more validation checks for augmented error-checking granularity
                    success = ModuleDeclarationStmt.Regex.Match(nextLine).Success;
					if (flag == true && success)
						// module declaration must be on the first line, throw an exception
						throw new ModuleDeclarationPlacementException("Module declarations must be at the top of the file. Did you mean to use a submodule declaration instead?", preLnNum);
					// we are past specific error checks - throw a general exception stating the given statement is unrecognized
					throw new StatementNotRecognizedException("Statement not recognized as visiboole source code.", preLnNum);
				}
			}
			return stmtList;
		}

        /// <summary>
        /// Expands vector to a list of its variables
        /// </summary>
        /// <param name="exp">Expression to expand</param>
        /// <returns>A list of all variables</returns>
        private List<string> ExpandVector(string exp)
        {
            List<string> vars = new List<string>();

            if (exp.Contains("'"))
            {
                string format = Regex.Match(exp, @"\'[bBhHdD]").Value; // Get format

                if (!format.Equals("'d"))
                {
                    foreach (char c in exp.Substring(2))
                    {
                        vars.Add(string.Concat(format, c));
                    }
                }
                else
                {
                    string binary = Convert.ToString(Convert.ToInt32(exp.Substring(2), 10), 2);
                    foreach (char c in binary)
                    {
                        vars.Add(string.Concat(format, c));
                    }
                }

                return vars;
            }

            /* Get variable, bounds and step */
            string var = Regex.Match(exp, @"^\*?[a-zA-Z0-9]+").Value; // Get Variable
            string nums = Regex.Match(exp, @"\[(.*?)\]").Value; // Get numbers
            MatchCollection matches = Regex.Matches(nums, @"\d+"); // Get bounds and step

            /* Assign start, step and end from num values */
            int start = Convert.ToInt32(matches[0].Value);
            int step = (matches.Count == 2) ? 1 : Convert.ToInt32(matches[1].Value);
            int end = (matches.Count == 2) ? Convert.ToInt32(matches[1].Value) : Convert.ToInt32(matches[2].Value);

            /* Create list with expanded variables */
            if (start < end)
            {
                for (int i = start; i <= end; i += step)
                    vars.Add(String.Concat(var, i.ToString()));
            }
            else
            {
                for (int i = start; i >= end; i -= step)
                    vars.Add(String.Concat(var, i.ToString()));
            }
            return vars;
        }

        /// <summary>
        /// Expands line into lines
        /// </summary>
        /// <param name="exp">Line to expand</param>
        /// <returns>List of all lines</returns>
        private List<string> ExpandLine(string exp)
        {
            List<string> lines = new List<string>();
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
            MatchCollection matches = regex.Matches(exp);

            /* Expand all variables */
            List<List<string>> variables = new List<List<string>>();
            foreach (Match match in matches)
            {
                if (!match.Value.Contains("{"))
                {
                    variables.Add(ExpandVector(match.Value));
                }
                else
                {
                    /* Get concat and split into vars */
                    string concat = Regex.Replace(match.Value, @"[{\s*}]", string.Empty);
                    string[] vars = concat.Split(','); // Split variables by commas

                    List<string> varsList = new List<string>();
                    foreach (string var in vars)
                    {
                        if (Regex.IsMatch(var, Globals.PatternAnyVectorType))
                        {
                            List<string> expand = ExpandVector(var);
                            foreach (string v in expand) varsList.Add(v);
                        }
                        else
                        {
                            varsList.Add(var);
                        }
                    }

                    variables.Add(varsList);
                }
            }

            /* Error checking */
            foreach (List<string> list in variables)
            {
                if (list.Count != variables[0].Count)
                {
                    return null;
                }
            }

            /* Expand lines */
            for (int i = 0; i < variables[0].Count; i++)
            {
                string line = exp;
                int j = 0;
                foreach (Match match in matches)
                {
                    line = line.Replace(match.Value, variables[j++][i]);
                }
                lines.Add(line);
            }
            return lines;
        }
    }
}
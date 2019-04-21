﻿/*
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.ParsingEngine.Statements;
using VisiBoole.Views;

namespace VisiBoole.ParsingEngine
{
    /// <summary>
    /// The main class of the parsing engine. This class is the brains of the parsing engine and 
    /// communicates with the calling classes.
    /// </summary>
	public class Parser : Lexer
	{
        #region Parsing Patterns & Regular Expressions

        /// <summary>
        /// Pattern for identifying format specifiers.
        /// </summary>
        public static readonly string FormatSpecifierPattern = $@"({FormatterPattern}{ConcatPattern})";
        
        /// <summary>
        /// Regex for identifying scalars. (Optional *)
        /// </summary>
        public static Regex ScalarRegex { get; } = new Regex(ScalarPattern2, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying entire format specifiers.
        /// </summary>
        public static Regex FormatSpecifierRegex { get; } = new Regex(FormatSpecifierPattern);

        /// <summary>
        /// Regex for identifying comment statements.
        /// </summary>
        public static Regex CommentStmtRegex { get; } = new Regex(@"^(?<FrontSpacing>\s*)(?<DoInclude>[+-])?""(?<Comment>.*)""(?<BackSpacing>\s*);$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying library statements.
        /// </summary>
        private static Regex LibraryStmtRegex = new Regex(@"^(?<FrontSpacing>\s*)#library\s(?<Name>\S+)(?<BackSpacing>\s*);$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying module declarations.
        /// </summary>
        private Regex ModuleRegex;

        /// <summary>
        /// Regex for identifying submodule statements.
        /// </summary>
        private static Regex SubmoduleStmtRegex = new Regex($@"(?<FrontSpacing>\s*)(?<Instantiation>{InstantiationPattern})\({ModulePattern}\)(?<BackSpacing>\s*);", RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// List of operators.
        /// </summary>
        public static readonly IList<string> OperatorsList = new ReadOnlyCollection<string>(new List<string> { "^", "|", "+", "-", "==", " ", "~" });
        public static readonly IList<string> ExclusiveOperatorsList = new ReadOnlyCollection<string>(new List<string>{"^", "+", "-", "=="});

        /// <summary>
        /// Statements generated by the parser.
        /// </summary>
        private List<Statement> Statements;

        /// <summary>
        /// Constructs a parser to parse designs.
        /// </summary>
        /// <param name="design">Design to parse</param>
        public Parser(Design design) : base(design)
        {
            ModuleRegex = new Regex($@"^\s*{Design.FileName}\({ModulePattern}\);$");
        }

        #region Parsing Methods

        /// <summary>
        /// Gets the parsed output from the statement list.
        /// </summary>
        /// <returns>Parsed output</returns>
        private List<IObjectCodeElement> GetParsedOutput()
        {
            // Parse statements for output
            List<IObjectCodeElement> output = new List<IObjectCodeElement>();
            foreach (Statement statement in Statements)
            {
                statement.Parse(); // Parse output
                output.AddRange(statement.Output); // Add output
                statement.Output = new List<IObjectCodeElement>(); // Clear output
            }

            return output;
        }

        /// <summary>
        /// Ticks statements with alt clocks that go from off to on.
        /// </summary>
        private void TickAltClocks()
        {
            if (Design.Database.AltClocks.Count > 0)
            {
                foreach (KeyValuePair<string, AltClock> kv in Design.Database.AltClocks)
                {
                    if ((Design.Database.TryGetValue(kv.Key) == 1) && !(bool)kv.Value.ObjCodeValue)
                    {
                        foreach (Statement stmt in Statements)
                        {
                            if (stmt.GetType() == typeof(DffClockStmt))
                            {
                                DffClockStmt clockStmt = ((DffClockStmt)stmt);
                                if (clockStmt.AltClock == kv.Key)
                                {
                                    clockStmt.Tick();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the stored values of all alternate clocks.
        /// </summary>
        private void UpdateAltClocks()
        {
            if (Design.Database.AltClocks.Count > 0)
            {
                // Update alt clock values
                foreach (KeyValuePair<string, AltClock> kv in Design.Database.AltClocks)
                {
                    kv.Value.UpdateValue(Design.Database.TryGetValue(kv.Key) == 1);
                }
            }
        }

        /// <summary>
        /// Parsers the current design text into output.
        /// </summary>
        public List<IObjectCodeElement> Parse()
        {
            // Get statements for parsing
            Design.Database = new Database();
            Statements = ParseStatements();
            if (Statements == null)
            {
                ErrorListBox.Display(ErrorLog);
                return null;
            }

            // Get output
            UpdateAltClocks();
            return GetParsedOutput();
        }

        /// <summary>
        /// Parses the current design text and clicks the provided variable.
        /// </summary>
        /// <param name="variableName">Variable clicked</param>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseClick(string variableName)
        {
            // Flip value of variable clicked and reevlaute expressions
            Design.Database.FlipValue(variableName);
            Design.Database.ReevaluateExpressions();
            TickAltClocks();
            UpdateAltClocks();

            // Get output
            return GetParsedOutput();
        }

        /// <summary>
        /// Parsers the provided design text and ticks.
        /// </summary>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseTick()
        {
            // Tick clock statements and reevaluate expressions
            foreach (Statement stmt in Statements)
            {
                if (stmt.GetType() == typeof(DffClockStmt))
                {
                    DffClockStmt clockStmt = ((DffClockStmt)stmt);
                    if (clockStmt.AltClock == null)
                    {
                        clockStmt.Tick();
                    }
                }
            }
            Design.Database.ReevaluateExpressions();
            TickAltClocks();
            UpdateAltClocks();

            // Get output
            return GetParsedOutput();
        }

        /// <summary>
        /// Parsers the current design text with input variables.
        /// </summary>
        /// <param name="inputs">Input variables</param>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseWithInput(List<Variable> inputs)
        {
            // Get statements for parsing
            Design.Database = new Database();
            foreach (Variable input in inputs)
            {
                Design.Database.AddVariable(new IndependentVariable(input.Name, input.Value));
            }

            Statements = ParseStatements();
            if (Statements == null)
            {
                ErrorListBox.Display(ErrorLog);
                return null;
            }
            UpdateAltClocks();

            // Get output
            return GetParsedOutput();
        }

        /// <summary>
        /// Parses the design as a module with the provided input values.
        /// </summary>
        /// <param name="inputValues">Values of input variables in the module declaration</param>
        /// <returns>Values of output variables in the module declaration</returns>
        public List<bool> ParseAsModule(List<bool> inputValues)
        {
            // Init database and get module match
            Design.Database = new Database();

            // Check design for valid module declaration
            if (Design.ModuleDeclaration == null || !Regex.IsMatch(Design.ModuleDeclaration, ModulePattern))
            {
                ErrorListBox.Display(new List<string>(new string[] { $"Unable to locate a valid module declaration inside design '{Design.FileName}'. Please check your source file for errors." }));
                return null;
            }

            Match moduleMatch = Regex.Match(Design.ModuleDeclaration, ModulePattern);

            // Set input values
            int inputValuesIndex = 0;
            foreach (string inputList in Regex.Split(moduleMatch.Groups["Inputs"].Value, @",\s+"))
            {
                foreach (string input in WhitespaceRegex.Split(inputList))
                {
                    foreach (string inputVar in GetExpansion(AnyTypeRegex.Match(input)))
                    {
                        Design.Database.AddVariable(new IndependentVariable(inputVar, inputValues[inputValuesIndex++]));
                    }
                }
            }

            // Parse statements
            Statements = ParseStatements();
            if (Statements == null)
            {
                ErrorListBox.Display(new List<string>(new string[] { $"Error parsing design '{Design.FileName}'. Please check/run your source file for errors." }));
                return null;
            }
            UpdateAltClocks();

            // Get output values
            List<bool> outputValues = new List<bool>();
            foreach (string outputList in Regex.Split(moduleMatch.Groups["Outputs"].Value, @",\s+"))
            {
                // Output each output var in the output list
                foreach (string output in WhitespaceRegex.Split(outputList))
                {
                    foreach (string outputVar in GetExpansion(AnyTypeRegex.Match(output)))
                    {
                        outputValues.Add(Design.Database.TryGetValue(outputVar) == 1);
                    }
                }
            }
            return outputValues;
        }

        /// <summary>
        /// Returns a list of inputs from the specified instantiation and module.
        /// </summary>
        /// <param name="instantiationName">Instantiation of module</param>
        /// <param name="moduleDeclaration">Module declaration</param>
        /// <returns>List of input variables</returns>
        public List<Variable> GetModuleInputs(string instantiationName, string moduleDeclaration)
        {
            string instantiation = Instantiations[instantiationName];
            string inputLists = Regex.Match(instantiation, ModuleInstantiationPattern).Groups["Inputs"].Value;

            // Get input values
            List<bool> inputValues = new List<bool>();
            foreach (string input in Regex.Split(inputLists, @",\s+"))
            {
                string[] vars = GetExpansion(AnyTypeRegex.Match(input)).ToArray();
                foreach (string var in vars)
                {
                    inputValues.Add(Design.Database.TryGetValue(var) == 1);
                }
            }

            // Get input variables
            List<string> inputNames = new List<string>();
            foreach (string input in Regex.Split(Regex.Match(moduleDeclaration, ModulePattern).Groups["Inputs"].Value, @",\s+"))
            {
                inputNames.AddRange(GetExpansion(AnyTypeRegex.Match(input)));
            }

            List<Variable> inputVariables = new List<Variable>();
            for (int i = 0; i < inputValues.Count; i++)
            {
                inputVariables.Add(new IndependentVariable(inputNames[i], inputValues[i]));
            }

            return inputVariables;
        }

        /// <summary>
        /// Exports the independent variables
        /// </summary>
        /// <returns></returns>
        public List<Variable> ExportState()
        {
            List<Variable> variables = new List<Variable>();
            foreach (Variable var in Design.Database.AllVars.Values)
            {
                if (var.GetType() == typeof(IndependentVariable))
                {
                    variables.Add(var);
                }
            }
            return variables;
        }

        /// <summary>
        /// Creates a list of statements from parsed source code.
        /// </summary>
        /// <returns>List of statements</returns>
        private List<Statement> ParseStatements()
        {
            List<Statement> statements = new List<Statement>();
            bool valid = true;
            byte[] bytes = Encoding.UTF8.GetBytes(Design.Text);
            MemoryStream stream = new MemoryStream(bytes);
            using (StreamReader reader = new StreamReader(stream))
            {
                LineNumber = 0;
                Design.ModuleDeclaration = null;
                string source;

                while ((source = reader.ReadLine()) != null)
                {
                    LineNumber++;
                    StatementType? type = null;

                    // Trim end of source
                    source = source.TrimEnd();
                    // If source is only whitespace
                    if (String.IsNullOrWhiteSpace(source))
                    {
                        // If current execution is valid
                        if (valid)
                        {
                            // Add empty statement to statement list
                            statements.Add(new EmptyStmt(source));
                        }
                        // Continue loop
                        continue;
                    }
                    // If source is a comment statement
                    else if (CommentStmtRegex.IsMatch(source))
                    {
                        // If current execution is valid
                        if (valid)
                        {
                            // Get comment match
                            Match commentMatch = CommentStmtRegex.Match(source);
                            // If comment should be displayed
                            if (commentMatch.Groups["DoInclude"].Value != "-" && (Properties.Settings.Default.SimulationComments || commentMatch.Groups["DoInclude"].Value == "+"))
                            {
                                // Add comment statement to statement list
                                statements.Add(new CommentStmt(source));
                            }
                        }
                        // Continue loop
                        continue;
                    }
                    // If source is a library statement
                    else if (LibraryStmtRegex.IsMatch(source))
                    {
                        // If library statement doesn't contain a valid library
                        if (!VerifyLibraryStatement(source))
                        {
                            // Set valid to false
                            valid = false;
                        }
                        // Continue loop (Libraries don't get added to statement list)
                        continue;
                    }
                    // If source is none of the above
                    else
                    {
                        // Get statement type
                        type = GetStatementType(source);

                        if (type == null)
                        {
                            // Set valid to false
                            valid = false;
                            // Continue loop
                            continue;
                        }
                    }

                    // If source is a submodule statement
                    if (SubmoduleStmtRegex.IsMatch(source))
                    {
                        // If submodule instantiation isn't valid
                        if (!VerifySubmoduleStatement(source))
                        {
                            // Set valid to false
                            valid = false;
                            // Continue loop
                            continue;
                        }
                    }
                    // If source is a module statement
                    else if (ModuleRegex.IsMatch(source))
                    {
                        // If design doesn't have a module declaration
                        if (Design.ModuleDeclaration == null)
                        {
                            // Set the design's module declaration to the source
                            Design.ModuleDeclaration = source;
                        }
                        // If design has a module module declaration
                        else
                        {
                            // Add invalid module statement error to error list
                            ErrorLog.Add("Designs can only have one module statement.");
                            // Set valid to false
                            valid = false;
                            // Continue loop
                            continue;
                        }
                    }
                    else
                    {
                        // Verify line from its statement type
                        if (!VerifyStatement(source, type))
                        {
                            // Set valid to false
                            valid = false;
                            // Continue loop
                            continue;
                        }
                    }

                    // Remove double **
                    source = source.Replace("**", "");
                    // Remove double ~~
                    source = source.Replace("~~", "");

                    bool needsExpansion = ExpansionRegex.IsMatch(source) || type == StatementType.Submodule;
                    if (needsExpansion && source.Contains("=") && !source.Contains("+") && !source.Contains("-"))
                    {
                        // Vertical expansion needed
                        source = ExpandVertically(source);
                        if (source == null)
                        {
                            valid = false;
                            continue;
                        }
                    }
                    else if (needsExpansion)
                    {
                        // Horizontal expansion needed
                        if (type != StatementType.Submodule && type != StatementType.Module)
                        {
                            source = ExpandHorizontally(source);
                        }
                        else
                        {
                            // Get text that shouldn't be expanded
                            string frontText = source.Substring(0, source.IndexOf("(") + 1);
                            // Get text that needs to be expanded
                            string restOfText = source.Substring(frontText.Length);
                            // Combine front text with the expanded form of the rest of the text
                            source = $"{frontText}{ExpandHorizontally(restOfText)}";
                        }
                    }

                    string[] lines = source.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        if (type == StatementType.Boolean)
                        {
                            statements.Add(new BooleanAssignmentStmt(line));
                        }
                        else if (type == StatementType.Clock)
                        {
                            statements.Add(new DffClockStmt(line));
                        }
                        else if (type == StatementType.FormatSpecifier)
                        {
                            statements.Add(new FormatSpecifierStmt(line));
                        }
                        else if (type == StatementType.VariableList)
                        {
                            statements.Add(new VariableListStmt(line));
                        }
                        else if (type == StatementType.Submodule)
                        {
                            Match match = ModuleInstantiationRegex.Match(line);
                            statements.Add(new SubmoduleInstantiationStmt(line, Subdesigns[match.Groups["Design"].Value]));
                        }
                        else if (type == StatementType.Module)
                        {
                            statements.Add(new ModuleDeclarationStmt(line));
                        }
                    }
                }
            }

            // If module declaration verify
            if (!String.IsNullOrEmpty(Design.ModuleDeclaration) && !VerifyModuleDeclarationStatement())
            {
                valid = false;
            }

            // If valid return statement list
            // Otherwise return null
            return valid ? statements : null;
        }

        #endregion

        #region Statement Verifications

        /// <summary>
        /// Verifies the statement from the provided line
        /// </summary>
        /// <param name="line">Line of the statement</param>
        /// <param name="type">Type of statement</param>
        /// <returns>Whether the statement is valid or not</returns>
        private bool VerifyStatement(string line, StatementType? type)
        {
            if (type == StatementType.Boolean || type == StatementType.Clock)
            {
                // Verify expressions
                int start = line.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
                string dependent;
                string operation;
                string expression;
                if (!line.Contains("<"))
                {
                    operation = "=";
                    dependent = line.Substring(start, line.IndexOf('=') - start).Trim();
                }
                else
                {
                    if (!line.Contains("@"))
                    {
                        operation = "<=";
                    }
                    else
                    {
                        operation = Regex.Match(line, @"<=@\S+").Value;
                    }
                    dependent = line.Substring(start, line.IndexOf('<') - start).Trim();
                }
                expression = line.Substring(line.IndexOf(operation) + operation.Length).Trim();
                expression = expression.TrimEnd(';');
                expression = String.Concat("(", expression, ")"); 

                if (!VerifyExpressionStatement(expression))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Verifies an expression statement
        /// </summary>
        /// <param name="expression">Expression to verify</param>
        /// <returns>Whether the expression is valid or not</returns>
        private bool VerifyExpressionStatement(string expression)
        {
            bool wasPreviousOperator = true;
            bool mathematicalExpression = false;
            List<StringBuilder> expressions = new List<StringBuilder>();
            List<List<string>> expressionOperators = new List<List<string>>();
            List<string> expressionExclusiveOperators = new List<string>();

            // matches contains:
            // Name: ([_a-zA-Z]\w{0,19})
            // Operators (~, ^, (, ), |, +, -): ([~^()|+-])
            // Equal To Operator: (==)
            // And Operator: ((?<=[\w)}])\s+(?=[\w({~'])(?![^{}]*\}))
            MatchCollection matches = Regex.Matches(expression, @"([_a-zA-Z]\w{0,19})|([~^()|+-])|(==)|((?<=[\w)}])\s+(?=[\w({~'])(?![^{}]*\}))");
            string token = "";
            foreach (Match match in matches)
            {
                token = match.Value;
                if (token == "(")
                {
                    // Add new level
                    if (expressions.Count > 0)
                    {
                        expressions[expressions.Count - 1].Append("(");
                    }
                    expressions.Add(new StringBuilder());
                    expressionOperators.Add(new List<string>());
                    expressionExclusiveOperators.Add("");
                    wasPreviousOperator = true;
                }
                else if (token == ")")
                {
                    string innerExpression = expressions[expressions.Count - 1].ToString();

                    if (innerExpression.Length == 0)
                    {
                        ErrorLog.Add($"{LineNumber}: Empty ().");
                        return false;
                    }

                    // Remove previous level
                    expressions.RemoveAt(expressions.Count - 1);
                    expressionOperators.RemoveAt(expressionOperators.Count - 1);
                    expressionExclusiveOperators.RemoveAt(expressionExclusiveOperators.Count - 1);
                    if (expressions.Count > 0)
                    {
                        expressions[expressions.Count - 1].Append(")");
                    }
                    wasPreviousOperator = false;
                }
                else if (OperatorsList.Contains(token) || String.IsNullOrWhiteSpace(token))
                {
                    // Check operator for possible errors
                    if (wasPreviousOperator && token != "~")
                    {
                        ErrorLog.Add($"{LineNumber}: An operator is missing its operands.");
                        return false;
                    }

                    // Check operator and mathematical expression status for error
                    if (mathematicalExpression && !(token == "+" || token == "-"))
                    {
                        ErrorLog.Add($"{LineNumber}: '+' and '-' can't appear with boolean operators.");
                        return false;
                    }

                    // Check for mathematical expression
                    if (!mathematicalExpression && (token == "+" || token == "-"))
                    {
                        mathematicalExpression = true;
                    }

                    // Check exclusive operator for errors
                    if (!mathematicalExpression)
                    {
                        string exclusiveOperator = expressionExclusiveOperators[expressionExclusiveOperators.Count - 1];
                        if (exclusiveOperator == "")
                        {
                            // Currently no exclusive operator, check to add one
                            if (ExclusiveOperatorsList.Contains(token))
                            {
                                List<string> pastOperators = expressionOperators[expressionOperators.Count - 1];

                                // Check previous operators
                                if (token == "^")
                                {
                                    pastOperators = pastOperators.Where(o => o != "^").ToList();
                                }
                                else if (token == "==")
                                {
                                    pastOperators = pastOperators.Where(o => o != "==").ToList();
                                }

                                if (pastOperators.Count > 0)
                                {
                                    ErrorLog.Add($"{LineNumber}: '{token}' must be the only operator in its parentheses level.");
                                    return false;
                                }

                                expressionExclusiveOperators[expressionExclusiveOperators.Count - 1] = token;
                            }
                        }
                        else
                        {
                            if (token != exclusiveOperator)
                            {
                                ErrorLog.Add($"{LineNumber}: '{exclusiveOperator}' must be the only operator in its parentheses level.");
                                return false;
                            }
                        }
                    }

                    expressions[expressions.Count - 1].Append(token);
                    expressionOperators[expressionOperators.Count - 1].Add(token);
                    wasPreviousOperator = true;
                }
                else
                {
                    // Append non operator to current parentheses level
                    expressions[expressions.Count - 1].Append(token);
                    wasPreviousOperator = false;
                }
            }

            return true;
        }

        /// <summary>
        /// Verifies a library statement
        /// </summary>
        /// <param name="line">Line to verify</param>
        /// <returns>Whether the line is valid or not</returns>
        private bool VerifyLibraryStatement(string line)
        {
            string library = LibraryStmtRegex.Match(line).Groups["Name"].Value;
            try
            {
                // Insert slash if not present
                if (library[0] != '.' && library[0] != '\\' && library[0] != '/')
                {
                    library = library.Insert(0, "\\");
                }

                string path = Path.GetFullPath(Design.FileSource.DirectoryName + library);
                if (Directory.Exists(path))
                {
                    Libraries.Add(path);
                    return true;
                }
                else
                {
                    ErrorLog.Add($"{LineNumber}: Library '{path}' doesn't exist or is invalid.");
                    return false;
                }
            }
            catch (Exception)
            {
                ErrorLog.Add($"{LineNumber}: Invalid library name '{library}'.");
                return false;
            }
        }

        /// <summary>
        /// Verifies a module declaration statement
        /// </summary>
        /// <param name="declaration">Declaration to verifiy</param>
        /// <returns>Whether the declaration is valid or not</returns>
        private bool VerifyModuleDeclarationStatement()
        {
            // Get input and output variables
            Match module = ModuleRegex.Match(Design.ModuleDeclaration);

            // Check input variables
            foreach (string input in Regex.Split(module.Groups["Inputs"].Value, @",\s+"))
            {
                List<string> vars;
                if (!input.Contains("{") && !input.Contains("["))
                {
                    vars = new List<string>();
                    vars.Add(input);
                }
                else
                {
                    vars = GetExpansion(AnyTypeRegex.Match(input));
                }

                foreach (string var in vars)
                {
                    if (Design.Database.TryGetVariable<IndependentVariable>(var) == null)
                    {
                        ErrorLog.Insert(0, $"'{var}' must be an independent variable to be used as an input in a module declaration statement.");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Verifies the instantiation with its matching declaration.
        /// </summary>
        /// <param name="instantiation">Instantiation to verify</param>
        /// <returns>Whether the instantiation is valid</returns>
        private bool VerifySubmoduleStatement(string instantiation)
        {
            Match instantiationMatch = ModuleInstantiationRegex.Match(instantiation);
            string[] instantiationInputVars = Regex.Split(instantiationMatch.Groups["Inputs"].Value, @",\s+");
            string[] instantiationOutputVars = Regex.Split(instantiationMatch.Groups["Outputs"].Value, @",\s+");
            List<List<string>> instantiationVars = new List<List<string>>();
            foreach (string var in instantiationInputVars)
            {
                instantiationVars.Add(GetExpansion(AnyTypeRegex.Match(var)));
            }
            foreach (string var in instantiationOutputVars)
            {
                instantiationVars.Add(GetExpansion(AnyTypeRegex.Match(var)));
            }

            FileInfo fileInfo = new FileInfo(Subdesigns[instantiationMatch.Groups["Design"].Value]);
            string name = fileInfo.Name.Split('.')[0];
            Regex declarationRegex = new Regex($@"^\s*{name}\({ModulePattern}\);$");
            using (StreamReader reader = fileInfo.OpenText())
            {
                string nextLine = string.Empty;
                while ((nextLine = reader.ReadLine()) != null)
                {
                    Match match = declarationRegex.Match(nextLine);
                    if (match.Success)
                    {
                        int i = 0;

                        string[] declarationInputVars = Regex.Split(match.Groups["Inputs"].Value, @",\s+");
                        if (instantiationInputVars.Length != declarationInputVars.Length)
                        {
                            ErrorLog.Add($"{LineNumber}: Instantiation doesn't have the same number of input variables as the matching module declaration.");
                            return false;
                        }
                        string[] declarationOutputVars = Regex.Split(match.Groups["Outputs"].Value, @",\s+");
                        if (instantiationOutputVars.Length != declarationOutputVars.Length)
                        {
                            ErrorLog.Add($"{LineNumber}: Instantiation doesn't have the same number of output variables as the matching module declaration.");
                            return false;
                        }

                        foreach (string inputVar in declarationInputVars)
                        {
                            if (instantiationVars[i++].Count != GetExpansion(AnyTypeRegex.Match(inputVar)).Count)
                            {
                                ErrorLog.Add($"{LineNumber}: Instantiation doesn't have the same number of input variables as the matching module declaration.");
                                return false;
                            }
                        }

                        foreach (string outputVar in declarationOutputVars)
                        {
                            if (instantiationVars[i++].Count != GetExpansion(AnyTypeRegex.Match(outputVar)).Count)
                            {
                                ErrorLog.Add($"{LineNumber}: Instantiation doesn't have the same number of output variables as the matching module declaration.");
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
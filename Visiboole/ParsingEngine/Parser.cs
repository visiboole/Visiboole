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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        /// <summary>
        /// Class that contains the statement text and type.
        /// </summary>
        private class SourceCode
        {
            /// <summary>
            /// Text of the statement.
            /// </summary>
            public string Text { get; private set; }

            /// <summary>
            /// Type of the statement.
            /// </summary>
            public StatementType Type { get; private set; }

            /// <summary>
            /// Constructs a source code element with the specified text and type.
            /// </summary>
            /// <param name="text">Text of statement</param>
            /// <param name="type">Type of statement</param>
            public SourceCode(string text, StatementType type)
            {
                // Save text of statement
                Text = text;
                // Save type of statement
                Type = type;
            }
        }

        #region Parsing Patterns & Regular Expressions

        /// <summary>
        /// Pattern for identifying scalars. (Optional *)
        /// </summary>
        private static readonly string ScalarPattern2 = $@"(?<!([%.']))(\*?{ScalarPattern})(?!\.)";

        /// <summary>
        /// Pattern for identifying vectors. (Optional *)
        /// </summary>
        private static readonly string VectorPattern2 = $@"\*?{VectorPattern}";

        /// <summary>
        /// Pattern for identifying scalars, vectors and constants. (Optional *)
        /// </summary>
        private static readonly string VariablePattern2 = $@"({VectorPattern2}|{ConstantPattern}|{ScalarPattern2})";

        /// <summary>
        /// Pattern for identifying format specifiers.
        /// </summary>
        public static readonly string FormatSpecifierPattern = $@"({FormatterPattern}{ConcatPattern})";

        /// <summary>
        /// Pattern for identifying module instantiations.
        /// </summary>
        public static readonly string ModuleInstantiationPattern = $@"((?<Padding>\s*)?(?<Instantiation>{InstantiationPattern})\({ModulePattern}\))";

        /// <summary>
        /// Pattern for identifying whitespace.
        /// </summary>
        public static Regex WhitespaceRegex { get; } = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalars. (Optional *)
        /// </summary>
        public static Regex ScalarRegex { get; } = new Regex(ScalarPattern2, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying comma seperating.
        /// </summary>
        public static Regex CommaSeperatingRegex { get; } = new Regex(@",\s*", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vectors that need to be expanded.
        /// </summary>
        private static Regex VectorRegex2 { get; } = new Regex(VectorPattern2, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying constants that need to be expanded.
        /// </summary>
        private static Regex ConstantRegex2 { get; } = new Regex($@"((?<=^|\W){ConstantPattern})", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalars, vectors and constants.
        /// </summary>
        public static Regex VariableRegex { get; } = new Regex(VariablePattern2, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying concatenations.
        /// </summary>
        private static Regex ConcatRegex = new Regex($@"((?<!{FormatterPattern}){ConcatPattern})", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying concatenations of any type or any type.
        /// </summary>
        private static Regex AnyTypeRegex = new Regex(AnyTypePattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying module instantiations.
        /// </summary>
        private static Regex ModuleInstantiationRegex = new Regex(ModuleInstantiationPattern);

        private static Regex EqualToRegex = new Regex($@"{AnyTypePattern}\s*==\s*{AnyTypePattern}", RegexOptions.Compiled);

        /// <summary>
        /// Regex for determining whether expansion is required.
        /// </summary>
        private static Regex ExpansionRegex { get; } = new Regex($@"(({AnyTypePattern}\s*==\s*{AnyTypePattern})|(?<!{FormatterPattern}){ConcatPattern})|{VectorPattern}|((?<=^|\W){ConstantPattern})", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying comment statements.
        /// </summary>
        public static Regex CommentStmtRegex = new Regex(@"^(?<FrontSpacing>\s+)?""(?<Comment>[\s\S]+)""\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying library statements.
        /// </summary>
        public static Regex LibraryStmtRegex = new Regex(@"^\s*#library\s+(?<Name>\S[^;]*)\s*;$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying module declarations.
        /// </summary>
        private Regex ModuleRegex;

        /// <summary>
        /// Regex for identifying submodule statements.
        /// </summary>
        private static Regex SubmoduleStmtRegex = new Regex($@"\s*(?<Instantiation>{InstantiationPattern})\({ModulePattern}\)\s*;", RegexOptions.Compiled);

        #endregion

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
            ModuleRegex = new Regex($@"^\s*{Design.FileName}\({ModulePattern}\)\s*;$");
        }

        #region Design Helper Methods

        /// <summary>
        /// Converts the error dictionary to an error list to be displayed.
        /// </summary>
        /// <returns>List of errors to be displayed</returns>
        private List<string> GetErrorLog()
        {
            // If there are no errors
            if (ErrorLog.Count == 0)
            {
                return null;
            }

            // Create error log to return
            List<string> errorLog = new List<string>();
            // For every line
            for (int i = 1; i <= LineNumberCount; i++)
            {
                // If the line contains an error
                if (ErrorLog.ContainsKey(i))
                {
                    // Add the error to the error log
                    errorLog.Add($"{i}: {ErrorLog[i]}");
                }
            }
            return errorLog;
        }
        
        /// <summary>
        /// Exports the independent variables of the design.
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
            foreach (string input in CommaSeperatingRegex.Split(inputLists))
            {
                string[] vars = GetExpansion(AnyTypeRegex.Match(input)).ToArray();
                foreach (string var in vars)
                {
                    inputValues.Add(Design.Database.GetValue(var) == 1);
                }
            }

            // Get input variables
            List<string> inputNames = new List<string>();
            foreach (string input in CommaSeperatingRegex.Split(Regex.Match(moduleDeclaration, ModulePattern).Groups["Inputs"].Value))
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

        #endregion

        #region Parsing Methods

        #region Clock Methods

        /// <summary>
        /// Update all clock statements' next values
        /// </summary>
        private void UpdateClocks()
        {
            foreach (Statement stmt in Statements)
            {
                if (stmt.GetType() == typeof(ClockAssignmentStmt))
                {
                    ClockAssignmentStmt clockStmt = ((ClockAssignmentStmt)stmt);
                    clockStmt.Update();
                }
            }
        }
        
        /// <summary>
        /// Ticks statements with alt clocks that go from off to on.
        /// </summary>
        /// <returns>Whether an alternate clock was ticked</returns>
        private void TickAltClocks()
        {
            List<string> updateList = new List<string>();
            if (Design.Database.AltClocks.Count > 0)
            {
                foreach (KeyValuePair<string, bool> kv in Design.Database.AltClocks)
                {
                    if (!kv.Value && (Design.Database.GetValue(kv.Key) == 1))
                    {
                        foreach (Statement stmt in Statements)
                        {
                            if (stmt.GetType() == typeof(ClockAssignmentStmt))
                            {
                                ClockAssignmentStmt clockStmt = ((ClockAssignmentStmt)stmt);
                                if (clockStmt.Clock == kv.Key)
                                {
                                    clockStmt.Tick();
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

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
                output.AddRange(statement.Parse()); // Add output
            }

            return output;
        }

        /// <summary>
        /// Runs any present submodules. Returns whether there was an error.
        /// </summary>
        /// <returns>Whether there was an error</returns>
        private bool TryRunSubmodules()
        {
            var instantiations = Statements.Where(statement => statement.GetType() == typeof(InstantiationStmt)).ToArray();
            for (int i = 0; i < instantiations.Length; i++)
            {
                InstantiationStmt submodule = (InstantiationStmt)instantiations[i];
                if (!submodule.TryRunInstance())
                {
                    return false;
                }
            }

            bool reset;
            do
            {
                reset = false;
                for (int i = 0; i < instantiations.Length; i++)
                {
                    InstantiationStmt submodule = (InstantiationStmt)instantiations[i];
                    if (submodule.CheckRerun())
                    {
                        reset = true;
                    }
                }
            } while (reset == true);

            return true;
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
                ErrorListBox.Display(GetErrorLog());
                return null;
            }

            Design.Database.EvaluateExpressions();
            if (!TryRunSubmodules())
            {
                return null;
            }

            // Get output
            UpdateClocks();
            Design.Database.UpdateAltClocks();
            return GetParsedOutput();
        }

        /// <summary>
        /// Parses the current design text and clicks the provided variable.
        /// </summary>
        /// <param name="variableName">Variable clicked</param>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseClick(string variableName, string value = null)
        {
            string[] variables;
            if (variableName[0] != '{')
            {
                variables = new string[] { variableName };
                Design.Database.FlipValue(variableName);
            }
            else
            {
                variables = WhitespaceRegex.Split(variableName.Substring(1));
                Design.Database.SetValues(variables, value);
            }
            TryRunSubmodules();
            TickAltClocks();
            UpdateClocks();
            Design.Database.UpdateAltClocks();

            // Get output
            return GetParsedOutput();
        }

        /// <summary>
        /// Parsers the provided design text and ticks.
        /// </summary>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseTick()
        {
            // Tick clock statements
            foreach (Statement stmt in Statements)
            {
                if (stmt.GetType() == typeof(ClockAssignmentStmt))
                {
                    ClockAssignmentStmt clockStmt = ((ClockAssignmentStmt)stmt);
                    if (clockStmt.Clock == null)
                    {
                        clockStmt.Tick();
                    }
                }
            }
            TryRunSubmodules();
            TickAltClocks();
            UpdateClocks();
            Design.Database.UpdateAltClocks();

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
                ErrorListBox.Display(GetErrorLog());
                return null;
            }

            Design.Database.EvaluateExpressions();
            if (!TryRunSubmodules())
            {
                ErrorListBox.Display(GetErrorLog());
                return null;
            }

            UpdateClocks();
            Design.Database.UpdateAltClocks();
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
            if (Design.HeaderLine == null || !Regex.IsMatch(Design.HeaderLine, ModulePattern))
            {
                ErrorListBox.Display(new List<string>(new string[] { $"The module file '{Design.FileName}' did not contain a matching module statement." }));
                return null;
            }

            Match moduleMatch = Regex.Match(Design.HeaderLine, ModulePattern);

            // Set input values
            int inputValuesIndex = 0;
            foreach (string inputList in CommaSeperatingRegex.Split(moduleMatch.Groups["Inputs"].Value))
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
                ErrorListBox.Display(new List<string>(new string[] { $"Error parsing design '{Design.FileName}'. Please check/run that design file independently for errors." }));
                return null;
            }

            Design.Database.EvaluateExpressions();
            if (!TryRunSubmodules())
            {
                ErrorListBox.Display(GetErrorLog());
                return null;
            }
            UpdateClocks();
            Design.Database.UpdateAltClocks();

            // Get output values
            List<bool> outputValues = new List<bool>();
            foreach (string outputList in CommaSeperatingRegex.Split(moduleMatch.Groups["Outputs"].Value))
            {
                // Output each output var in the output list
                foreach (string output in WhitespaceRegex.Split(outputList))
                {
                    foreach (string outputVar in GetExpansion(AnyTypeRegex.Match(output)))
                    {
                        outputValues.Add(Design.Database.GetValue(outputVar) == 1);
                    }
                }
            }
            return outputValues;
        }

        #endregion

        #region Statement Creation

        /// <summary>
        /// Returns a string enumerable with the statement lines from the stream reader.
        /// </summary>
        /// <param name="streamReader">Stream reader that contains the lines</param>
        /// <returns>String enumerable containing the lines from the stream reader</returns>
        private List<string> ReadLines(StreamReader streamReader)
        {
            // Create list of lines to return
            var lines = new List<string>();
            // Create current statement
            var currentStatement = new StringBuilder();
            // Start current line number at 0
            CurrentLineNumber = 0;

            // Current line
            string line;
            // While the reader reads a line
            while ((line = streamReader.ReadLine()) != null)
            {
                line = line.TrimEnd();
                LineNumberCount++;
                CurrentLineNumber++;

                if (currentStatement.Length == 0)
                {
                    int firstCharIndex = -1;
                    char firstChar = '\0';
                    // For each character in the line
                    for (int i = 0; i < line.Length; i++)
                    {
                        // Get current character
                        char currentChar = line[i];
                        // If current character is not empty
                        if (currentChar != ' ')
                        {
                            // Set first non-empty character index to the index of the current character
                            firstCharIndex = i;
                            // Set first non-empty character to the current character
                            firstChar = currentChar;
                            // Break out of loop (We found our first non-empty character)
                            break;
                        }
                    }

                    if (firstChar == '"')
                    {
                        // Set ending character index to the last index of "
                        int endingQuoteIndex = line.LastIndexOf('"');
                        // If the line only contains one " or the line has non-empty characters after the "
                        if (firstCharIndex == endingQuoteIndex || line.Substring(endingQuoteIndex + 1).Any(c => c != ' '))
                        {
                            // Add invalid comment statement error to error log
                            ErrorLog.Add(CurrentLineNumber, "Invalid comment statement. Comment statements must begin and end with '\"'.");
                            // Return null for error
                            return null;
                        }

                        // Trim end of line and add to list of lines
                        lines.Add(line);
                        // Continue to next line
                        continue;
                    }
                    else if (firstChar == '\0')
                    {
                        lines.Add(line);
                    }
                }

                // Get index of ;
                int endingSemicolonIndex = line.IndexOf(';');
                if (endingSemicolonIndex != -1)
                {
                    // If the line has non-empty characters after the semicolon
                    if (line.Substring(endingSemicolonIndex + 1).Any(c => c != ' '))
                    {
                        // Add multiple statements on line error to error log
                        ErrorLog.Add(CurrentLineNumber, "Only one statement can appear on a line.");
                        // Return null for error
                        return null;
                    }

                    // If the current statement isn't empty
                    if (currentStatement.Length > 0)
                    {
                        // Add new line seperator to the current statement
                        currentStatement.Append('\n');
                        // Add current line to the current statement
                        currentStatement.Append(line);
                        lines.Add(currentStatement.ToString());
                        currentStatement.Clear();
                    }
                    else
                    {
                        lines.Add(line);
                    }
                }
                else
                {
                    // If the current statement isn't empty
                    if (currentStatement.Length > 0)
                    {
                        // Add new line seperator to the current statement
                        currentStatement.Append('\n');
                    }
                    // Add current line to the current statement
                    currentStatement.Append(line);
                }
            }

            // If the current statement is not empty
            if (currentStatement.Length > 0)
            {
                // Add unfinished statement error to error log
                ErrorLog.Add(CurrentLineNumber, $"'{currentStatement.Replace('\n', ' ')}' is missing an ending semicolon.");
                return null;
            }

            return lines;
        }

        /// <summary>
        /// Validates, expands and initializes the provided source code.
        /// </summary>
        /// <param name="sourceCode">Source code</param>
        /// <returns>List of expanded source if operations were successful</returns>
        private List<SourceCode> GetExpandedSourceCode(List<string> statementText)
        {
            // Create source code list to return
            List<SourceCode> sourceCode = new List<SourceCode>();
            // Create valid bool
            bool valid = true;
            // Start module declaration string
            Design.HeaderLine = null;
            // Start line number counter
            CurrentLineNumber = 0;
            // Declare statement type
            StatementType? type = StatementType.Empty;
            // Indicates whether a non header or library statement has been found
            bool foundDesignStatement = false;

            // For each statement in the statement text list
            foreach (string statement in statementText)
            {
                // Increment line number counter
                CurrentLineNumber++;
                // If source is not only whitespace
                if (!string.IsNullOrWhiteSpace(statement))
                {
                    // Get statement type
                    type = GetStatementType(statement);
                }
                // If source is only whitespace
                else
                {
                    // Set statement type to empty
                    type = StatementType.Empty;
                }

                // If statement type is null
                if (type == null)
                {
                    // If current execution is valid
                    if (valid)
                    {
                        // Set valid to false
                        valid = false;
                    }
                }
                // If statement type is library
                else if (type == StatementType.Library)
                {
                    if (Design.HeaderLine != null)
                    {
                        // Add invalid module statement error to error list
                        ErrorLog.Add(CurrentLineNumber, $"Library statements must be before header statements.");
                        // Set valid to false
                        valid = false;
                    }
                    else if (foundDesignStatement)
                    {
                        // Add invalid module statement error to error list
                        ErrorLog.Add(CurrentLineNumber, $"Library statements must be before any design statements.");
                        // Set valid to false
                        valid = false;
                    }
                    else
                    {
                        // If library isn't valid and the current execution is valid
                        if (!VerifyLibraryStatement(statement) && valid)
                        {
                            // Set valid to false
                            valid = false;
                        }
                    }
                }
                // If statement type is module
                else if (type == StatementType.Header)
                {
                    if (foundDesignStatement)
                    {
                        ErrorLog.Add(CurrentLineNumber, $"Header statements must be the first design statement following any library statements.");
                        // Set valid to false
                        valid = false;
                    }

                    // If design doesn't have a module declaration
                    if (Design.HeaderLine == null)
                    {
                        // Set the design's module declaration to the source
                        Design.HeaderLine = statement;
                    }
                    // If design has a module module declaration
                    else
                    {
                        ErrorLog.Add(CurrentLineNumber, $"Designs can only have one header statement.");
                        // Set valid to false
                        valid = false;
                    }
                }
                else
                {
                    if (type == StatementType.Instantiation)
                    {
                        // If submodule instantiation isn't valid and the current execution is valid
                        if (!VerifySubmoduleStatement(statement) && valid)
                        {
                            // Set valid to false
                            valid = false;
                        }
                    }

                    if (type != StatementType.Comment && type != StatementType.Empty)
                    {
                        foundDesignStatement = true;
                    }
                }

                // If current execution is valid
                if (valid)
                {
                    // Add statement and its type to the list of source code
                    sourceCode.Add(new SourceCode(statement, (StatementType)type));
                }
            }

            // If not valid
            if (!valid)
            {
                return null;
            }

            // Reset line number
            CurrentLineNumber = 1;
            // For each source code in the source code list
            for (int i = 0; i < sourceCode.Count; i++)
            {
                // Get source from source code list
                SourceCode source = sourceCode[i];
                // Get whether the source can be expanded
                bool canExpand = source.Type != StatementType.Empty && source.Type != StatementType.Comment && source.Type != StatementType.Library;
                
                // Declare expanded text
                string expandedText;
                // If source can expand
                if (canExpand)
                {
                    // Get expanded text of source
                    expandedText = ExpandSource(source.Text);
                }
                // If source can't expand
                else
                {
                    // Set expanded text equal to the source text
                    expandedText = source.Text;
                }
                // If expanded text is null
                if (expandedText == null)
                {
                    // If current execution is valid
                    if (valid)
                    {
                        // Set valid to false
                        valid = false;
                    }
                    CurrentLineNumber += source.Text.Count(c => c == '\n') + 1;
                    // Continue to next source
                    continue;
                }

                // If current execution is valid and not empty
                if (valid && expandedText.Length != 0 && source.Type != StatementType.Comment && source.Type != StatementType.Library)
                {
                    // Remove current source code
                    sourceCode.RemoveAt(i);

                    // Get expanded source text array
                    string[] expandedSourceText = expandedText.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    // For each expanded source text in reverse
                    for (int j = expandedSourceText.Length - 1; j >= 0; j--)
                    {
                        // Get line of the expanded source text
                        string line = expandedSourceText[j];
                        // If source is a header or instantiation
                        if (source.Type == StatementType.Header || source.Type == StatementType.Instantiation)
                        {
                            if (!InitSource(line.Substring(line.IndexOf('(')), source.Type))
                            {
                                // Set valid to false
                                valid = false;
                                // End expanded source iterations
                                break;
                            }
                        }
                        // If source is a variable display or assignment statement
                        else
                        {
                            if (!InitSource(line, source.Type))
                            {
                                // Set valid to false
                                valid = false;
                                // End expanded source iterations
                                break;
                            }
                        }

                        // Add line to expanded source code list
                        sourceCode.Insert(i, new SourceCode($"{line};", source.Type));
                    }
                    // Increment i by one less the number of added lines
                    i += expandedSourceText.Length - 1;
                }

                CurrentLineNumber += source.Text.Count(c => c == '\n') + 1;
            }

            // If execution is valid: return expanded source code list
            // Otherwise: return null
            return valid ? sourceCode : null;
        }

        /// <summary>
        /// Creates a list of statements from parsed source code.
        /// </summary>
        /// <returns>List of statements</returns>
        private List<Statement> ParseStatements()
        {
            // Create statement list to return
            List<Statement> statements = new List<Statement>();
            // Create statement text list
            List<string> statementText;

            // Get design text as bytes
            byte[] bytes = Encoding.UTF8.GetBytes(Design.Text);
            // Create memory stream of design bytes
            MemoryStream stream = new MemoryStream(bytes);
            // With a stream reader
            using (StreamReader reader = new StreamReader(stream))
            {
                // Read the statement text from the bytes in the stream
                statementText = ReadLines(reader);
            }
            // If statement text is null
            if (statementText == null)
            {
                return null;
            }

            // Get expanded source code from the statement text
            List<SourceCode> expandedSourceCode = GetExpandedSourceCode(statementText);
            // If expanded source code is null
            if (expandedSourceCode == null)
            {
                return null;
            }
            // For each source in the expanded source code
            foreach (SourceCode source in expandedSourceCode)
            {
                // If the source statement type is a library statement
                if (source.Type == StatementType.Library)
                {
                    // Skip library statement
                    continue;
                }
                // If the source statement type is an empty statement
                else if (source.Type == StatementType.Empty)
                {
                    // Add empty statement to statement list
                    statements.Add(new EmptyStmt());
                }
                // If the source statement type is a comment statement
                else if (source.Type == StatementType.Comment)
                {
                    // Get comment match
                    Match commentMatch = CommentStmtRegex.Match(source.Text);
                    // Get comment to display
                    string comment = $"{commentMatch.Groups["FrontSpacing"].Value}{commentMatch.Groups["Comment"].Value}";
                    // Add comment statement to statement list
                    statements.Add(new CommentStmt(comment));
                }
                // If the source statement type is a boolean statement
                else if (source.Type == StatementType.Assignment)
                {
                    // Add boolean statement to statement list
                    statements.Add(new BooleanAssignmentStmt(source.Text));
                }
                // If the source statement type is a clock statement
                else if (source.Type == StatementType.ClockAssignment)
                {
                    // Add clock statement to statement list
                    statements.Add(new ClockAssignmentStmt(source.Text));
                }
                // If the source statement type is a variable display statement
                else if (source.Type == StatementType.VariableDisplay)
                {
                    // Add format specifier statement to statement list
                    statements.Add(new VariableDisplayStmt(source.Text));
                }
                // If the source statement type is a header statement
                else if (source.Type == StatementType.Header)
                {
                    // Add module declaration statement to statement list
                    statements.Add(new HeaderStmt(source.Text));
                }
                // If the source statement type is a submodule statement
                else if (source.Type == StatementType.Instantiation)
                {
                    // Get module instantiation match
                    Match match = ModuleInstantiationRegex.Match(source.Text);
                    // Add submodule instantiation statement to statement list
                    statements.Add(new InstantiationStmt(source.Text, Subdesigns[match.Groups["Design"].Value]));
                }
            }

            // Return statement list
            return statements;
        }

        #endregion

        #region Statement Verifications

        /// <summary>
        /// Verifies a library statement
        /// </summary>
        /// <param name="line">Line to verify</param>
        /// <returns>Whether the line is valid or not</returns>
        private bool VerifyLibraryStatement(string line)
        {
            string library = LibraryStmtRegex.Match(line.Trim()).Groups["Name"].Value;
            library = library.Replace('/', '\\');
            try
            {
                string path;
                if (library[0] != '\\')
                {
                    path = Path.GetFullPath(Design.FileSource.DirectoryName + '\\' + library);
                }
                else
                {
                    path = Path.GetFullPath(library.Substring(1));
                }

                if (Directory.Exists(path))
                {
                    if (!Libraries.Contains(path))
                    {
                        Libraries.Add(path);
                    }
                    return true;
                }
                else
                {
                    ErrorLog.Add(CurrentLineNumber, $"Library named '{path}' doesn't exist. Please check your library name.");
                    return false;
                }
            }
            catch (Exception)
            {
                ErrorLog.Add(CurrentLineNumber, $"An error has occured while locating the library named'{library}'. Please check your library name.");
                return false;
            }
        }

        /// <summary>
        /// Verifies the instantiation with its matching declaration.
        /// </summary>
        /// <param name="instantiation">Instantiation to verify</param>
        /// <returns>Whether the instantiation is valid</returns>
        private bool VerifySubmoduleStatement(string instantiation)
        {
            Match instantiationMatch = ModuleInstantiationRegex.Match(instantiation);
            if (!instantiationMatch.Success)
            {
                ErrorLog.Add(CurrentLineNumber, $"Instantiation '{instantiation}' is not in valid format.");
                return false;
            }
            string[] instantiationInputVars = CommaSeperatingRegex.Split(instantiationMatch.Groups["Inputs"].Value);
            string[] instantiationOutputVars = CommaSeperatingRegex.Split(instantiationMatch.Groups["Outputs"].Value);
            List<List<string>> instantiationVars = new List<List<string>>();
            foreach (string var in instantiationInputVars)
            {
                instantiationVars.Add(GetExpansion(AnyTypeRegex.Match(var)));
            }
            foreach (string var in instantiationOutputVars)
            {
                instantiationVars.Add(GetExpansion(AnyTypeRegex.Match(var)));
            }

            string designName = instantiationMatch.Groups["Design"].Value;
            Match match = Regex.Match(Subdesigns[designName].HeaderLine, $@"^\s*{designName}\({ModulePattern}\);$");
            int i = 0;

            string[] declarationInputVars = CommaSeperatingRegex.Split(match.Groups["Inputs"].Value);
            if (instantiationInputVars.Length != declarationInputVars.Length)
            {
                ErrorLog.Add(CurrentLineNumber, $"Instantiation doesn't have the same number of input variables as the matching module declaration.");
                return false;
            }
            string[] declarationOutputVars = CommaSeperatingRegex.Split(match.Groups["Outputs"].Value);
            if (instantiationOutputVars.Length != declarationOutputVars.Length)
            {
                ErrorLog.Add(CurrentLineNumber, $"Instantiation doesn't have the same number of output variables as the matching module declaration.");
                return false;
            }

            foreach (string inputVar in declarationInputVars)
            {
                if (instantiationVars[i++].Count != GetExpansion(AnyTypeRegex.Match(inputVar)).Count)
                {
                    ErrorLog.Add(CurrentLineNumber, $"Instantiation doesn't have the same number of input variables as the matching module declaration.");
                    return false;
                }
            }

            foreach (string outputVar in declarationOutputVars)
            {
                if (instantiationVars[i++].Count != GetExpansion(AnyTypeRegex.Match(outputVar)).Count)
                {
                    ErrorLog.Add(CurrentLineNumber, $"Instantiation doesn't have the same number of output variables as the matching module declaration.");
                    return false;
                }
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Returns what line number the current index is on.
        /// </summary>
        /// <param name="source">Source text</param>
        /// <param name="index">Current index in the source text</param>
        /// <returns>Returns what line number the current index is on</returns>
        private int GetLineNumber(string source, int index)
        {
            int currentLineNumber = CurrentLineNumber;
            for (int i = 0; i < index; i++)
            {
                if (source[i] == '\n')
                {
                    currentLineNumber++;
                }
            }
            return currentLineNumber;
        }

        /// <summary>
        /// Initializes variables and dependencies in the provided source.
        /// </summary>
        /// <param name="source">Source to init</param>
        /// <param name="type">Statement type of source</param>
        /// <returns>Whether the source was initialized</returns>
        private bool InitSource(string source, StatementType type)
        {
            // Init dependents dictionary
            List<string> dependents = new List<string>();
            // Init dependencies list
            List<string> dependencies = new List<string>();
            // Init dependent seperator index
            int dependentSeperatorIndex = source.IndexOf('=');
            // Init module seperator index
            int moduleSeperatorIndex = source.IndexOf(':');

            // Get variables in the statement
            MatchCollection variableMatches = ScalarRegex.Matches(source);
            // Iterate through all variables in the statement
            foreach (Match variableMatch in variableMatches)
            {
                // Get variable
                string variable = variableMatch.Value;
                // Get value of variable
                bool value = variable.Contains("*");
                // If value is true
                if (value)
                {
                    // Remove * from variable
                    variable = variable.TrimStart('*');
                }
                // Get whether the variable is a dependent
                string dependent;
                if (variableMatch.Index < dependentSeperatorIndex
                    || (type == StatementType.Instantiation && variableMatch.Index > moduleSeperatorIndex))
                {
                    dependent = type == StatementType.ClockAssignment ? $"{variable}.d" : variable;
                }
                else
                {
                    dependent = string.Empty;
                }

                if (type == StatementType.Instantiation && dependent.Length != 0 && variableMatch.Value == "NC")
                {
                    continue;
                }

                // If statement type is boolean or submodule
                // Readd  || type == StatementType.Instantiation
                if (type == StatementType.Assignment || type == StatementType.ClockAssignment)
                {
                    if (dependent.Length == 0)
                    {
                        // If dependents contains the variable
                        if (dependents.Contains(variable))
                        {
                            // Circular dependency error
                            ErrorLog.Add(GetLineNumber(source, variableMatch.Index), $"Combinational circular dependency error: '{variable}' cannot depend on itself.");
                            return false;
                        }

                        // Add variable to dependencies list
                        if (!dependencies.Contains(variable))
                        {
                            dependencies.Add(variable);
                        }
                    }
                    // If variable is dependent
                    else
                    {
                        if (Design.Database.HasDependencyList(variable) || Design.Database.GetValue($"{variable}.d") != -1)
                        {
                            // Already dependent error
                            ErrorLog.Add(GetLineNumber(source, variableMatch.Index), $"{variable} was previously assigned a value.");
                            return false;
                        }

                        /*
                        if (dependencies.Contains(variable))
                        {
                            // Circular dependency error
                            ErrorLog.Add(GetLineNumber(source, variableMatch.Index), $"{variable} cannot depend on itself.");
                            return false;
                        }
                        */

                        // Add variable to dependents list
                        if (!dependents.Contains(dependent))
                        {
                            dependents.Add(dependent);
                        }
                    }
                }
                
                // If variable isn't in the database
                if (Design.Database.TryGetVariable<Variable>(variable) == null)
                {
                    // If variable isn't dependent
                    if (dependent.Length == 0)
                    {
                        if (type == StatementType.Header && variableMatch.Index < moduleSeperatorIndex)
                        {
                            // Add variable to the database
                            Design.Database.AddVariable(new IndependentVariable(variable, value), true);
                        }
                        else
                        {
                            // Add variable to the database
                            Design.Database.AddVariable(new IndependentVariable(variable, value));
                        }
                    }
                    // If variable is dependent
                    else
                    {
                        if (type != StatementType.ClockAssignment)
                        {
                            // Add variable to the database
                            Design.Database.AddVariable(new DependentVariable(variable, value));
                        }
                        else
                        {
                            // Add variable to the database
                            Design.Database.AddVariable(new IndependentVariable(variable, value));
                        }
                    }
                }
                // If variable is in the database
                else
                {
                    // If variable is dependent, in the database not as a dependent
                    if (type != StatementType.ClockAssignment && dependent.Length != 0 && Design.Database.TryGetVariable<DependentVariable>(variable) == null)
                    {
                        // Make variable in database a dependent
                        if (!Design.Database.MakeDependent(variable))
                        {
                            ErrorLog.Add(GetLineNumber(source, variableMatch.Index), $"'{variable}' must be an independent variable to be used as an input in a module header statement.");
                            return false;
                        }
                    }
                }

                if (dependent.Length != 0 && variable.Length != dependent.Length)
                {
                    Design.Database.AddVariable(new DependentVariable(dependent, value));
                }
            }

            // For each dependent in depedents list
            foreach (string dependent in dependents)
            {
                // If unable to add dependency list to database
                if (!Design.Database.TryAddDependencyList(dependent, dependencies))
                {
                    // Circular dependency error
                    ErrorLog.Add(GetLineNumber(source, source.IndexOf(dependent)), $"Combinational circular dependency error: '{dependent}' cannot depend on itself.");
                    return false;
                }
            }

            return true;
        }

        #region Expansion Methods

        /// <summary>
        /// Exapnds a single token into its components.
        /// </summary>
        /// <param name="token">Token to expand</param>
        /// <returns>List of expansion components</returns>
        protected List<string> ExpandToken(Match token)
        {
            if (token.Value.Contains("[") && string.IsNullOrEmpty(token.Groups["LeftBound"].Value))
            {
                List<string> components = Design.Database.GetVectorComponents(token.Groups["Name"].Value);
                if (components == null)
                {
                    ErrorLog.Add(CurrentLineNumber, $"'{token.Value}' is missing an explicit dimension.");
                    return null;
                }

                if (token.Value.Contains("~") && !components[0].Contains("~"))
                {
                    for (int i = 0; i < components.Count; i++)
                    {
                        components[i] = string.Concat("~", components[i]);
                    }
                }
                return components;
            }
            else
            {
                if (ExpansionMemo.ContainsKey(token.Value))
                {
                    return ExpansionMemo[token.Value].ToList();
                }
                else
                {
                    if (token.Value.Contains("["))
                    {
                        return ExpandVector(token);
                    }
                    else
                    {
                        return ExpandConstant(token);
                    }
                }
            }
        }

        /// <summary>
        /// Expands token into components.
        /// </summary>
        /// <param name="token">Token to expand</param>
        /// <returns>List of expansion components</returns>
        protected List<string> GetExpansion(Match token)
        {
            List<string> expansion = new List<string>();

            // Get token's variables
            string[] vars;
            if (token.Value.Contains("{"))
            {
                vars = WhitespaceRegex.Split(token.Value);
            }
            else
            {
                vars = new string[] { token.Value };
            }

            // Expand each variable
            foreach (string var in vars)
            {
                Match match = VariableRegex.Match(var);

                if (match.Value.Contains("[") || match.Value.Contains("'") || match.Value.All(char.IsDigit))
                {
                    List<string> tokenExpansion = ExpandToken(match);
                    if (tokenExpansion == null)
                    {
                        return null;
                    }

                    expansion.AddRange(tokenExpansion);
                }
                else
                {
                    expansion.Add(match.Value);
                }
            }

            return expansion;
        }

        /// <summary>
        /// Expands the provided source.
        /// </summary>
        /// <param name="source">Source to expand</param>
        /// <returns>Expanded source</returns>
        private string ExpandSource(string source)
        {
            // Get line count inside source
            int lineCount = source.Count(c => c == '\n');
            // Get whether the source needs to be expanded
            bool needsExpansion = ExpansionRegex.IsMatch(source) || source.Contains(':');

            if (needsExpansion && source.Contains("="))
            {
                if (source.Contains("+") || source.Contains("-"))
                {
                    source = ExpandHorizontally(source);
                }
                else
                {
                    var equalToOperations = EqualToRegex.Matches(source);
                    if (equalToOperations.Count > 0)
                    {
                        for (int i = equalToOperations.Count - 1; i >= 0; i--)
                        {
                            var equalToOperation = equalToOperations[i];
                            source = string.Concat(source.Substring(0, equalToOperation.Index), ExpandHorizontally(equalToOperation.Value), source.Substring(equalToOperation.Index + equalToOperation.Length));
                        }
                    }

                    if (ExpansionRegex.IsMatch(source))
                    {
                        // Vertical expansion needed
                        source = ExpandVertically(source);
                    }
                }
            }
            else if (needsExpansion)
            {
                // Horizontal expansion needed
                if (!source.Contains(':'))
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

            /*
            // If source needs to be expanded, is an expression statement and is not a mathematical expression
            if (needsExpansion && source.Contains("=") && !source.Contains("+") && !source.Contains("-") && !source.Contains("=="))
            {
                // Vertical expansion needed
                source = ExpandVertically(source);
            }
            // If source needs to be expanded
            else if (needsExpansion)
            {
                // Horizontal expansion needed
                if (!source.Contains(':'))
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
            */

            // Increment line number by the line count
            CurrentLineNumber += lineCount;
            return source;
        }

        /// <summary>
        /// Expands all concatenations and vectors in a line.
        /// </summary>
        /// <param name="line">Line to expand</param>
        /// <returns>Expanded line</returns>
        private string ExpandHorizontally(string line)
        {
            string expandedLine = line;
            int maxExpansionCount = 1;
            Match match;

            while ((match = VectorRegex2.Match(expandedLine)).Success)
            {
                List<string> expansion = GetExpansion(match);
                if (expansion == null)
                {
                    ErrorLog.Add(GetLineNumber(expandedLine, match.Index), $"'{match.Value}' is missing an explicit dimension somewhere.");
                    return null;
                }
                if (expansion.Count > maxExpansionCount)
                {
                    maxExpansionCount = expansion.Count;
                }
                
                // Replace matched vector with its components
                expandedLine = expandedLine.Substring(0, match.Index) + string.Join(" ", expansion) + expandedLine.Substring(match.Index + match.Length);
            }

            do
            {
                match = null;
                foreach (Match constant in ConstantRegex2.Matches(expandedLine))
                {
                    if (constant.Value != "1" && constant.Value != "0")
                    {
                        match = constant;

                        List<string> expansion = GetExpansion(match);
                        if (expansion.Count > maxExpansionCount)
                        {
                            maxExpansionCount = expansion.Count;
                        }

                        // Replace matched constants with its components
                        expandedLine = expandedLine.Substring(0, match.Index) + string.Join(" ", expansion) + expandedLine.Substring(match.Index + match.Length);

                        break;
                    }
                }
            } while (match != null);

            if (!line.Contains(':'))
            {
                while ((match = ConcatRegex.Match(expandedLine)).Success)
                {
                    List<string> expansion = GetExpansion(match);
                    if (expansion == null)
                    {
                        ErrorLog.Add(GetLineNumber(expandedLine, match.Index), $"'{match.Value}' is missing an explicit dimension somewhere.");
                        return null;
                    }
                    if (expansion.Count > maxExpansionCount)
                    {
                        maxExpansionCount = expansion.Count;
                    }

                    // Replace matched concat with its components
                    expandedLine = expandedLine.Substring(0, match.Index) + string.Join(" ", expansion) + expandedLine.Substring(match.Index + match.Length);
                }
            }

            // If line contains a = (Math expression)
            if (line.Contains('=') && maxExpansionCount > 1)
            {
                int assignmentIndex = expandedLine.IndexOf('=');
                Regex variableListRegex = new Regex($@"(?<=^|[\s({{|=^~+-])({VariableListPattern2}|[01])(?![^{{}}]*\}})"); // Variable lists not inside {}
                while ((match = variableListRegex.Match(expandedLine)).Success)
                {
                    // Get variable list
                    string variableList = match.Value;
                    // Count the number of elements
                    int elementCount = WhitespaceRegex.Split(variableList).Length;

                    // If element count is less than the max element count
                    if (elementCount < maxExpansionCount)
                    {
                        // If variable list isn't the dependent
                        if (match.Index > assignmentIndex)
                        {
                            // Add padding 0
                            for (int i = 0; i < maxExpansionCount - elementCount; i++)
                            {
                                variableList = variableList.Insert(0, "0 ");
                            }
                        }
                    }

                    // Add { } to variable list
                    expandedLine = expandedLine.Substring(0, match.Index) + string.Concat("{", variableList, "}") + expandedLine.Substring(match.Index + match.Length);
                }
            }

            return expandedLine;
        }

        /// <summary>
        /// Expands a line into lines.
        /// </summary>
        /// <param name="line">Line to expand</param>
        /// <returns>Expanded lines</returns>
        protected string ExpandVertically(string line)
        {
            string expanded = string.Empty;

            // Get dependent and expression
            int start = line.ToList().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            string dependent = line.Contains("<")
                ? line.Substring(start, line.IndexOf("<") - start).TrimEnd()
                : line.Substring(start, line.IndexOf("=") - start).TrimEnd();
            string expression = line.Substring(line.IndexOf("=") + 1).TrimStart();
            int expressionIndex = line.LastIndexOf(expression);

            // Expand dependent
            List<string> dependentExpansion = new List<string>();
            Match dependentMatch = AnyTypeRegex.Match(dependent);
            if (dependentMatch.Value.Contains("{"))
            {
                dependentExpansion = GetExpansion(dependentMatch);
                // If expansion fails
                if (dependentExpansion == null)
                {
                    ErrorLog.Add(GetLineNumber(line, dependentMatch.Index), $"'{dependent}' contains a [] notation that is missing an explicit dimension.");
                    return null;
                }
            }
            else if (dependentMatch.Value.Contains("["))
            {
                dependentExpansion = ExpandToken(dependentMatch);
                // If expansion fails
                if (dependentExpansion == null)
                {
                    ErrorLog.Add(GetLineNumber(line, dependentMatch.Index), $"'{dependentMatch.Groups["Name"].Value}[]' notation can't be used without an explicit dimension somewhere.");
                    return null;
                }
            }
            else
            {
                dependentExpansion.Add(dependent);
            }

            // Expand expression
            List<List<string>> expressionExpansions = new List<List<string>>();
            MatchCollection matches = ExpansionRegex.Matches(expression);
            foreach (Match match in matches)
            {
                if (!match.Value.Contains("=="))
                {
                    CurrentLineNumber = GetLineNumber(line, match.Index);
                    List<string> expansion;
                    bool canAdjust;
                    if (!match.Value.Contains("{"))
                    {
                        canAdjust = !match.Value.Contains("[") && string.IsNullOrEmpty(match.Groups["BitCount"].Value);
                        expansion = ExpandToken(match);
                    }
                    else
                    {
                        canAdjust = false;
                        expansion = GetExpansion(match);
                    }

                    // If expansion fails
                    if (expansion == null)
                    {
                        return null;
                    }

                    if (expansion.Count != dependentExpansion.Count)
                    {
                        if (canAdjust)
                        {
                            int adjustCount = dependentExpansion.Count - expansion.Count;
                            if (adjustCount > 0)
                            {
                                for (int i = 0; i < adjustCount; i++)
                                {
                                    expansion.Insert(0, "0");
                                }
                            }
                            else
                            {
                                for (int i = adjustCount; i < 0; i++)
                                {
                                    expansion.RemoveAt(0);
                                }
                            }
                        }
                        else
                        {
                            ErrorLog.Add(GetLineNumber(line, match.Index), $"Expansion size of '{match.Value}' doesn't match the size of '{dependent}'.");
                            return null;
                        }
                    }

                    expressionExpansions.Add(expansion);
                }
                else
                {
                    expressionExpansions.Add(new List<string>(new string[] { match.Value }));
                }
            }

            // Combine expansions
            List<List<string>> expansions = new List<List<string>>();
            expansions.Add(dependentExpansion);
            expansions.AddRange(expressionExpansions);


            if (matches.Count == 1 && (matches[0].Value[0] == '\'' || char.IsDigit(matches[0].Value[0])))
            {
                string newLine = line;

                string beforeMatch = newLine.Substring(0, matches[0].Index + expressionIndex);
                string afterMatch = newLine.Substring(expressionIndex + matches[0].Index + matches[0].Length);
                string matchReplacement = string.Join(" ", expressionExpansions[0]);
                newLine = string.Concat(beforeMatch, $"{{{matchReplacement}}}", afterMatch);

                // Replace dependent with expansion
                string beforeDependent = newLine.Substring(0, dependentMatch.Index);
                string afterDependent = newLine.Substring(dependentMatch.Index + dependentMatch.Length);
                string dependentReplacement = string.Join(" ", dependentExpansion);
                newLine = string.Concat(beforeDependent, $"{{{dependentReplacement}}}", afterDependent);

                expanded = newLine;
            }
            else
            {
                // Expand line into lines
                for (int i = 0; i < dependentExpansion.Count; i++)
                {
                    string newLine = line;

                    for (int j = matches.Count - 1; j >= 0; j--)
                    {
                        Match match = matches[j];
                        if (match.Value.Contains("=="))
                        {
                            continue;
                        }
                        string beforeMatch = newLine.Substring(0, match.Index + expressionIndex);
                        string afterMatch = newLine.Substring(match.Index + expressionIndex + match.Length);
                        newLine = string.Concat(beforeMatch, expansions[j + 1][i], afterMatch);
                    }

                    string beforeDependent = newLine.Substring(0, start);
                    string afterDependent = newLine.Substring(start + dependent.Length);
                    newLine = string.Concat(beforeDependent, expansions[0][i], afterDependent);

                    expanded += newLine;
                }
            }

            return expanded;
        }

        #endregion
    }
}
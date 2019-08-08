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
        /// Pattern for identifying instantiations.
        /// </summary>
        public static readonly string InstantiationPattern = $@"((?<Padding>\s*)?(?<Instant>{InstantPattern})\({ModulePattern}\))";

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
        public static Regex AnyTypeRegex = new Regex(AnyTypePattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying instantiations.
        /// </summary>
        private static Regex InstantiationRegex = new Regex(InstantiationPattern);

        /// <summary>
        /// Regex for identifying equal to operations.
        /// </summary>
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

        #endregion

        /// <summary>
        /// Constructs a parser to parse designs.
        /// </summary>
        public Parser() : base()
        {
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
                    errorLog.Add($"Line {i}: {ErrorLog[i]}");
                }
            }

            ErrorLog.Clear();
            return errorLog;
        }

        /// <summary>
        /// Creates a design header from the header text.
        /// </summary>
        /// <param name="header">Text of the header.</param>
        /// <returns>Design header.</returns>
        public DesignHeader CreateHeader(string header)
        {
            var designHeader = new DesignHeader();
            var headerMatch = Regex.Match(header, ModulePattern);
            int slotNumber = 0;

            foreach (string inputList in CommaSeperatingRegex.Split(headerMatch.Groups["Inputs"].Value))
            {
                foreach (string input in WhitespaceRegex.Split(inputList.TrimEnd()))
                {
                    if (input.Contains("[]"))
                    {
                        designHeader.Valid = false;
                        return designHeader;
                    }

                    foreach (string inputVar in GetExpansion(AnyTypeRegex.Match(input)))
                    {
                        designHeader.AddInput(slotNumber, inputVar);
                    }
                    slotNumber++;
                }
            }

            slotNumber = 0;
            foreach (string outputList in CommaSeperatingRegex.Split(headerMatch.Groups["Outputs"].Value))
            {
                // Output each output var in the output list
                foreach (string output in WhitespaceRegex.Split(outputList.TrimEnd()))
                {
                    if (output.Contains("[]"))
                    {
                        designHeader.Valid = false;
                        return designHeader;
                    }

                    foreach (string outputVar in GetExpansion(AnyTypeRegex.Match(output)))
                    {
                        designHeader.AddOutput(slotNumber, outputVar);
                    }
                    slotNumber++;
                }
            }

            return designHeader;
        }

        #endregion

        #region Parsing Methods
        
        /// <summary>
        /// Parses the provided design with the provided inputs (if any).
        /// </summary>
        /// <param name="design">Design to parse.</param>
        /// <param name="inputs">Inputs for the parsed design.</param>
        /// <returns>Output to be displayed.</returns>
        public List<IObjectCodeElement> Parse(Design design, List<Variable> inputs = null)
        {
            // Save design to parse
            Design = design;
            // Init Design
            Design.Database = new Database();
            ErrorLog = new Dictionary<int, string>();

            // If unable to parse statements
            if (!TryParseStatements())
            {
                // Display a error box with the errors
                ErrorListBox.Display(GetErrorLog());
                // Return null for error
                return null;
            }

            // If inputs were provided
            if (inputs != null)
            {
                // For each input provided
                foreach (var input in inputs)
                {
                    // Create independent variable test variable
                    IndependentVariable indVar;
                    // Try to get the input name in the database
                    Design.Database.IndVars.TryGetValue(input.Name, out indVar);

                    // If input exists in the database but as the wrong value
                    if (indVar != null && indVar.Value != input.Value)
                    {
                        // Set the value of the input to its inputted value
                        Design.Database.SetValue(input.Name, input.Value, false);
                    }
                }
            }

            // Evaluate all expressions
            Design.Database.EvaluateExpressions();
            // If unable to run all instantiations
            if (!Design.TryRunInstantiations(false))
            {
                // Return null for error
                return null;
            }
            // Update all clock statements' next values
            Design.ComputeClocks();

            // Return the parsed output of the statements
            return Design.GetOutput();
        }

        /// <summary>
        /// Parses the design as a module with the provided input values.
        /// </summary>
        /// <param name="inputValues">Values of input variables in the module declaration</param>
        /// <returns>Values of output variables in the module declaration</returns>
        public List<bool> ParseInstantiation(Design design, List<Variable> inputs)
        {
            Design = design;
            bool isUpdate = Design.Database.Statements.Count > 0;
            ErrorLog = new Dictionary<int, string>();

            // If unable to parse statements
            if (!isUpdate && !TryParseStatements())
            {
                // Display a error box with the errors
                ErrorListBox.Display(GetErrorLog());
                // Return null for error
                return null;
            }

            // For each input provided
            foreach (var input in inputs)
            {
                // Create independent variable test variable
                IndependentVariable indVar;
                // Try to get the input name in the database
                Design.Database.IndVars.TryGetValue(input.Name, out indVar);

                // If input exists in the database but as the wrong value
                if (indVar != null && indVar.Value != input.Value)
                {
                    // Set the value of the input to its inputted value
                    Design.Database.SetValue(input.Name, input.Value, isUpdate);
                }
            }

            if (!isUpdate)
            {
                // Evaluate all expressions
                Design.Database.EvaluateExpressions();
            }
            // If unable to run all instantiations
            if (!Design.TryRunInstantiations(false))
            {
                // Return null for error
                return null;
            }
            // Update all clock statements' next values
            Design.ComputeClocks();

            var outputValues = new List<bool>();
            foreach (var output in Design.Database.Header.GetOutputs())
            {
                outputValues.Add(Design.Database.GetValue(output) == 1);
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
            var sourceCode = new List<SourceCode>();
            // Create valid bool
            bool valid = true;
            // Clear design header
            Design.Database.Header = null;
            // Start line number counter
            CurrentLineNumber = 1;
            // Line number of the header
            int headerLineNumber = -1;
            // Indicates whether a non header or library statement has been found
            bool foundDesignStatement = false;

            // For each statement in the statement text list
            foreach (string statement in statementText)
            {
                // Get statement type
                StatementType? type = string.IsNullOrWhiteSpace(statement) ? StatementType.Empty : GetStatementType(statement);

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
                    if (Design.Database.Header != null)
                    {
                        // Add invalid module statement error to error list
                        ErrorLog.Add(CurrentLineNumber, $"Library statements must precede header statements.");
                        // Set valid to false
                        valid = false;
                    }
                    else if (foundDesignStatement)
                    {
                        // Add invalid module statement error to error list
                        ErrorLog.Add(CurrentLineNumber, $"Library statements must precede all design and display statements.");
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
                        ErrorLog.Add(CurrentLineNumber, $"Header statements must precede all design and display statements.");
                        // Set valid to false
                        valid = false;
                    }

                    // If design doesn't have a module declaration
                    if (headerLineNumber == -1)
                    {
                        // Set the design's module declaration to the source
                        Design.Database.Header = CreateHeader(statement);
                        headerLineNumber = CurrentLineNumber;
                    }
                    // If design has a module module declaration
                    else
                    {
                        ErrorLog.Add(CurrentLineNumber, $"Designs can only have one header statement.");
                        // Set valid to false
                        valid = false;
                    }
                }
                else if (type != StatementType.Comment && type != StatementType.Empty)
                {
                    foundDesignStatement = true;
                }

                // If current execution is valid
                if (valid)
                {
                    // Add statement and its type to the list of source code
                    sourceCode.Add(new SourceCode(statement, (StatementType)type));
                }

                // Increment line number counter
                CurrentLineNumber += statement.Count(c => c == '\n') + 1;
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
                    if (source.Type == StatementType.Instantiation)
                    {
                        // If instantiation isn't valid
                        if (!VerifyInstantiationStatement(source.Text))
                        {
                            // Set expanded text to null to continue
                            expandedText = null;
                        }
                        else
                        {
                            // Get expanded text of source
                            expandedText = ExpandSource(source);
                        }
                    }
                    else
                    {
                        // Get expanded text of source
                        expandedText = ExpandSource(source);
                    }
                }
                // If source can't expand
                else
                {
                    // Set expanded text equal to the source text
                    expandedText = source.Text;
                }

                int lineCount = source.Text.Count(c => c == '\n') + 1;

                // If expanded text is null
                if (expandedText == null)
                {
                    // Set valid to false
                    valid = false;
                    CurrentLineNumber += lineCount;
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
                        string line = source.Type == StatementType.Instantiation || source.Type == StatementType.Header
                            ? expandedSourceText[j].Substring(expandedSourceText[j].IndexOf('('))
                            : expandedSourceText[j];

                        if (!InitSource(line, source.Type))
                        {
                            // Set valid to false
                            valid = false;
                            // End expanded source iterations
                            break;
                        }

                        // Add line to expanded source code list
                        sourceCode.Insert(i, new SourceCode($"{expandedSourceText[j]};", source.Type));
                    }
                    // Increment i by one less the number of added lines
                    i += expandedSourceText.Length - 1;
                }

                CurrentLineNumber += lineCount;
            }

            if (Design.Database.Header != null && !Design.Database.Header.Valid)
            {
                ErrorLog.Add(headerLineNumber, $"All vectors in Headers must have explicit dimensions.");
                // Set valid to false
                valid = false;
            }
            else if (Design.Database.Header != null)
            {
                string inputCheck = Design.Database.Header.VerifyInputs();
                if (inputCheck != null)
                {
                    ErrorLog.Add(headerLineNumber, $"Header input {inputCheck} must be used in an assignment expression, as an alternate clock, or an input in an instantiation statement.");
                    // Set valid to false
                    valid = false;
                }
                else
                {
                    string outputCheck = Design.Database.Header.VerifyOutputs();
                    if (outputCheck != null)
                    {
                        ErrorLog.Add(headerLineNumber, $"Header output {outputCheck} must recieve a value from an assignment expression or an instantiation statement.");
                        // Set valid to false
                        valid = false;
                    }
                }
            }
            

            // If execution is valid: return expanded source code list
            // Otherwise: return null
            return valid ? sourceCode : null;
        }

        /// <summary>
        /// Attempts to create statements from the text of the design.
        /// </summary>
        /// <returns>List of statements</returns>
        private bool TryParseStatements()
        {
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
                // Return false for error
                return false;
            }

            // Get expanded source code from the statement text
            var expandedSourceCode = GetExpandedSourceCode(statementText);
            // If expanded source code is null
            if (expandedSourceCode == null)
            {
                return false;
            }
            // For each source in the expanded source code
            foreach (var source in expandedSourceCode)
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
                    Design.Database.Statements.Add(new EmptyStmt());
                }
                // If the source statement type is a comment statement
                else if (source.Type == StatementType.Comment)
                {
                    // Get comment match
                    Match commentMatch = CommentStmtRegex.Match(source.Text);
                    // Get comment to display
                    string comment = $"{commentMatch.Groups["FrontSpacing"].Value}{commentMatch.Groups["Comment"].Value}";
                    // Add comment statement to statement list
                    Design.Database.Statements.Add(new CommentStmt(comment));
                }
                // If the source statement type is a boolean statement
                else if (source.Type == StatementType.Assignment)
                {
                    // Add boolean statement to statement list
                    Design.Database.Statements.Add(new BooleanAssignmentStmt(source.Text));
                }
                // If the source statement type is a clock statement
                else if (source.Type == StatementType.ClockAssignment)
                {
                    // Create clock assignment statement
                    var clockStatement = new ClockAssignmentStmt(source.Text);
                    // Add clock statement to statement list
                    Design.Database.Statements.Add(clockStatement);
                    // Add clock statement to clock statements list
                    Design.Database.ClockStatements.Add(clockStatement);
                }
                // If the source statement type is a display statement
                else if (source.Type == StatementType.Display)
                {
                    // Add format specifier statement to statement list
                    Design.Database.Statements.Add(new DisplayStmt(source.Text));
                }
                // If the source statement type is a header statement
                else if (source.Type == StatementType.Header)
                {
                    // Add module declaration statement to statement list
                    Design.Database.Statements.Add(new HeaderStmt(source.Text));
                }
                // If the source statement type is a submodule statement
                else if (source.Type == StatementType.Instantiation)
                {
                    string instantName = Regex.Match(source.Text, InstantPattern).Groups["InstantName"].Value;
                    // Get instantiation of the statement
                    var instantiation = Design.Database.Instantiations[instantName];
                    // Create instantiation statement
                    var instantiationStatement = new InstantiationStmt(source.Text, instantiation);
                    // Add instantiation statement to statement list
                    Design.Database.Statements.Add(instantiationStatement);
                    // Add instantiation statement to instantiation statements list
                    Design.Database.InstantiationStatements.Add(instantiationStatement);
                }
            }

            // Return true for no errors
            return true;
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
                    if (!Design.Database.Libraries.Contains(path))
                    {
                        Design.Database.Libraries.Add(path);
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
        private bool VerifyInstantiationStatement(string instantiation)
        {
            Match instantiationMatch = InstantiationRegex.Match(instantiation);
            if (!instantiationMatch.Success)
            {
                ErrorLog.Add(CurrentLineNumber, $"Instantiation '{instantiation}' is not in valid format.");
                return false;
            }

            // Get design name
            string designName = instantiationMatch.Groups["DesignName"].Value;
            // Get design header
            var designHeader = Design.Database.Subdesigns[designName].Database.Header;
            if (!designHeader.Valid)
            {
                ErrorLog.Add(CurrentLineNumber, $"Error parsing design '{designName}'. Please check/run that design file independently for errors.");
                return false;
            }

            string[] instantiationInputs = CommaSeperatingRegex.Split(instantiationMatch.Groups["Inputs"].Value);
            string[] instantiationOutputs = CommaSeperatingRegex.Split(instantiationMatch.Groups["Outputs"].Value);
            var instantiationInputVars = new List<string[]>();
            var instantiationOutputVars = new List<string[]>();
            var instantiationInputVariables = new List<string>();
            var instantiationOutputVariables = new List<string>();

            if (instantiationInputs.Length != designHeader.InputSlots.Count)
            {
                ErrorLog.Add(CurrentLineNumber, $"Instantiation doesn't have the same number of input slots as the matching header.");
                return false;
            }
            for (int inputSlot = 0; inputSlot < instantiationInputs.Length; inputSlot++)
            {
                string input = instantiationInputs[inputSlot];
                // Get expansion of instantiation input
                var expansion = GetExpansion(AnyTypeRegex.Match(input))?.ToArray();
                if (expansion == null)
                {
                    return false;
                }
                else if (designHeader.InputSlots[inputSlot] != expansion.Length)
                {
                    ErrorLog.Add(CurrentLineNumber, $"Instantiation doesn't have the same number of input variables as the matching module declaration.");
                    return false;
                }
                else
                {
                    instantiationInputVars.Add(expansion);
                    instantiationInputVariables.AddRange(expansion);
                }
            }

            if (instantiationOutputs.Length != designHeader.OutputSlots.Count)
            {
                ErrorLog.Add(CurrentLineNumber, $"Instantiation doesn't have the same number of output slots as the matching header.");
                return false;
            }
            for (int outputSlot = 0; outputSlot < instantiationOutputs.Length; outputSlot++)
            {
                string output = instantiationOutputs[outputSlot];
                // Get expansion of instantiation output
                var expansion = GetExpansion(AnyTypeRegex.Match(output))?.ToArray();
                if (expansion == null)
                {
                    return false;
                }
                else if (designHeader.OutputSlots[outputSlot] != expansion.Length)
                {
                    ErrorLog.Add(CurrentLineNumber, $"Instantiation doesn't have the same number of output variables as the matching module declaration.");
                    return false;
                }
                else
                {
                    instantiationOutputVars.Add(expansion);
                    instantiationOutputVariables.AddRange(expansion);
                }
            }

            string instantName = instantiationMatch.Groups["InstantName"].Value;
            var designInstantiation = new DesignInstantiation(Design.Database.Subdesigns[designName].Clone(),
                instantiationInputVariables.ToArray(), instantiationOutputVariables.ToArray());
            Design.Database.Instantiations[instantName] = designInstantiation;

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
                if (variableMatch.Index < dependentSeperatorIndex || ((type == StatementType.Instantiation || type == StatementType.Header)
                    && variableMatch.Index > moduleSeperatorIndex))
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

                        // Add variable to dependents list
                        if (!dependents.Contains(dependent))
                        {
                            dependents.Add(dependent);
                        }
                    }
                }

                if (type != StatementType.Header)
                {
                    // If variable isn't in the database
                    if (Design.Database.TryGetVariable<Variable>(variable) == null)
                    {
                        // If variable isn't dependent
                        if (dependent.Length == 0)
                        {
                            // Add variable to the database
                            Design.Database.AddVariable(new IndependentVariable(variable, value));
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
                            if (Design.Database.Header != null && Design.Database.Header.HasInputVariable(variable))
                            {
                                ErrorLog.Add(GetLineNumber(source, variableMatch.Index), $"'{variable}' must be an independent variable to be used as an input in a module header statement.");
                                return false;
                            }

                            Design.Database.MakeDependent(variable);
                        }
                    }

                    if (dependent.Length != 0 && variable.Length != dependent.Length)
                    {
                        Design.Database.AddVariable(new DependentVariable(dependent, value));
                    }
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
        protected IEnumerable<string> ExpandToken(Match token)
        {
            if (token.Value.Contains("[") && string.IsNullOrEmpty(token.Groups["LeftBound"].Value))
            {
                var components = Design.Database.GetVectorComponents(token.Groups["Name"].Value)?.ToArray();
                if (components == null)
                {
                    PendingErrorMessage = $"'{token.Value}' is missing an explicit dimension.";
                    return null;
                }

                if (token.Value.Contains("~"))
                {
                    for (int i = 0; i < components.Length; i++)
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
                    return ExpansionMemo[token.Value];
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
        protected IEnumerable<string> GetExpansion(Match token)
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
                    var tokenExpansion = ExpandToken(match);
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
        /// <param name="sourceCode">Source to expand</param>
        /// <returns>Expanded source</returns>
        private string ExpandSource(SourceCode sourceCode)
        {
            // Get source text
            string source = sourceCode.Text;
            // Get whether the source needs to be expanded
            bool needsExpansion = ExpansionRegex.IsMatch(source) || source.Contains(':');

            if (needsExpansion && source.Contains("="))
            {
                if (source.Contains("+") || source.Contains("-"))
                {
                    source = ExpandHorizontally(source, sourceCode.Type);
                }
                else
                {
                    var equalToOperations = EqualToRegex.Matches(source);
                    if (equalToOperations.Count > 0)
                    {
                        for (int i = equalToOperations.Count - 1; i >= 0; i--)
                        {
                            var equalToOperation = equalToOperations[i];
                            source = string.Concat(source.Substring(0, equalToOperation.Index), ExpandHorizontally(equalToOperation.Value, sourceCode.Type), source.Substring(equalToOperation.Index + equalToOperation.Length));
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
                    source = ExpandHorizontally(source, sourceCode.Type);
                }
                else
                {
                    // Get text that shouldn't be expanded
                    string frontText = source.Substring(0, source.IndexOf("(") + 1);
                    // Get text that needs to be expanded
                    string restOfText = source.Substring(frontText.Length);

                    string backText = ExpandHorizontally(restOfText, sourceCode.Type);
                    if (backText == null)
                    {
                        return backText;
                    }
                    // Combine front text with the expanded form of the rest of the text
                    source = $"{frontText}{backText}";
                }
            }

            return source;
        }

        /// <summary>
        /// Expands all concatenations and vectors in a line.
        /// </summary>
        /// <param name="line">Line to expand</param>
        /// <returns>Expanded line</returns>
        private string ExpandHorizontally(string line, StatementType type)
        {
            string expandedLine = line;
            int maxExpansionCount = 1;
            Match match;

            while ((match = VectorRegex2.Match(expandedLine)).Success)
            {
                if (match.Value.Contains("[]") && type == StatementType.Header)
                {
                    return null;
                }

                var expansion = GetExpansion(match)?.ToArray();
                if (expansion == null)
                {
                    ErrorLog.Add(GetLineNumber(expandedLine, match.Index), PendingErrorMessage);
                    PendingErrorMessage = null;
                    return null;
                }
                if (expansion.Length > maxExpansionCount)
                {
                    maxExpansionCount = expansion.Length;
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

                        var expansion = GetExpansion(match)?.ToArray();
                        if (expansion.Length > maxExpansionCount)
                        {
                            maxExpansionCount = expansion.Length;
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
                    var expansion = GetExpansion(match)?.ToArray();
                    if (expansion == null)
                    {
                        ErrorLog.Add(GetLineNumber(expandedLine, match.Index), PendingErrorMessage);
                        PendingErrorMessage = null;
                        return null;
                    }
                    if (expansion.Length > maxExpansionCount)
                    {
                        maxExpansionCount = expansion.Length;
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
            List<string> dependentExpansion;
            Match dependentMatch = AnyTypeRegex.Match(dependent);
            if (dependentMatch.Value.Contains("{"))
            {
                dependentExpansion = GetExpansion(dependentMatch)?.ToList();
                // If expansion fails
                if (dependentExpansion == null)
                {
                    ErrorLog.Add(GetLineNumber(line, dependentMatch.Index), PendingErrorMessage);
                    PendingErrorMessage = null;
                    return null;
                }
            }
            else if (dependentMatch.Value.Contains("["))
            {
                dependentExpansion = ExpandToken(dependentMatch)?.ToList();
                // If expansion fails
                if (dependentExpansion == null)
                {
                    ErrorLog.Add(GetLineNumber(line, dependentMatch.Index), PendingErrorMessage);
                    return null;
                }
            }
            else
            {
                dependentExpansion = new List<string>(new string[] { dependent });
            }

            // Expand expression
            List<List<string>> expressionExpansions = new List<List<string>>();
            MatchCollection matches = ExpansionRegex.Matches(expression);
            foreach (Match match in matches)
            {
                if (!match.Value.Contains("=="))
                {
                    List<string> expansion;
                    bool canAdjust;
                    if (!match.Value.Contains("{"))
                    {
                        canAdjust = !match.Value.Contains("[") && string.IsNullOrEmpty(match.Groups["BitCount"].Value);
                        expansion = ExpandToken(match)?.ToList();
                    }
                    else
                    {
                        canAdjust = false;
                        expansion = GetExpansion(match)?.ToList();
                    }

                    // If expansion fails
                    if (expansion == null)
                    {
                        ErrorLog.Add(GetLineNumber(line, expressionIndex + match.Index), PendingErrorMessage);
                        PendingErrorMessage = null;
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

            if (matches.Count == 1 && expression.TrimEnd(';') == matches[0].Value &&
                (matches[0].Value[0] == '\'' || char.IsDigit(matches[0].Value[0])))
            {
                string newLine = line;

                string beforeMatch = newLine.Substring(0, matches[0].Index + expressionIndex);
                string afterMatch = newLine.Substring(expressionIndex + matches[0].Index + matches[0].Length);
                string matchReplacement = string.Join(" ", expressionExpansions[0]);
                if (expressionExpansions[0].Count == 1)
                {
                    newLine = string.Concat(beforeMatch, matchReplacement, afterMatch);
                }
                else
                {
                    newLine = string.Concat(beforeMatch, $"{{{matchReplacement}}}", afterMatch);
                }
                
                // Replace dependent with expansion
                string beforeDependent = newLine.Substring(0, dependentMatch.Index);
                string afterDependent = newLine.Substring(dependentMatch.Index + dependentMatch.Length);
                string dependentReplacement = string.Join(" ", dependentExpansion);
                if (dependentExpansion.Count == 1)
                {
                    newLine = string.Concat(beforeDependent, dependentReplacement, afterDependent);
                }
                else
                {
                    newLine = string.Concat(beforeDependent, $"{{{dependentReplacement}}}", afterDependent);
                }
                
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
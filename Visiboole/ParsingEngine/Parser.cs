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
using System.Windows.Forms;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.ParsingEngine.Statements;
using VisiBoole.Views;

namespace VisiBoole.ParsingEngine
{
    /// <summary>
    /// Statement types recognized by the parser.
    /// </summary>
    public enum StatementType
    {
        Library,
        Module,
        Submodule,
        Boolean,
        Clock,
        VariableList,
        FormatSpecifier,
        Comment,
        Empty
    }

    /// <summary>
    /// The main class of the parsing engine. This class is the brains of the parsing engine and 
    /// communicates with the calling classes.
    /// </summary>
	public class Parser
	{
        #region Parsing Patterns & Regular Expressions

        /// <summary>
        /// Pattern for identifying scalars (when creating tokens)
        /// </summary>
        public static readonly string ScalarPattern = @"^([~*]*(?<Name>[_a-zA-Z]\w{0,19}))$";

        /// <summary>
        /// Pattern for identifying scalars (no ~ or *)
        /// </summary>
        public static readonly string ScalarPattern2 = @"(?<Name>[_a-zA-Z]\w{0,19})";

        /// <summary>
        /// Pattern for identifying scalars (with optional ~)
        /// </summary>
        public static readonly string ScalarPattern3 = $@"(~?{ScalarPattern2})";

        /// <summary>
        /// Pattern for identifying scalars (with optional *)
        /// </summary>
        public static readonly string ScalarPattern4 = $@"(\*?{ScalarPattern2})";

        /// <summary>
        /// Pattern for identifying vector notation (appended to scalar patterns)
        /// </summary>
        private static readonly string VectorNotationPattern = @"((\[(?<LeftBound>\d+)\.(?<Step>[1-9]\d*)?\.(?<RightBound>\d+)\])|(\[\]))";

        /// <summary>
        /// Pattern for identifying vectors (when creating tokens)
        /// </summary>
        public static readonly string VectorPattern = $@"^([~*]*{ScalarPattern2}{VectorNotationPattern})$";

        /// <summary>
        /// Pattern for identifying vectors (no ~ or *)
        /// </summary>
        public static readonly string VectorPattern2 = $@"({ScalarPattern2}{VectorNotationPattern})";

        /// <summary>
        /// Pattern for identifying vectors (with optional ~)
        /// </summary>
        public static readonly string VectorPattern3 = $@"({ScalarPattern3}{VectorNotationPattern})";

        /// <summary>
        /// Pattern for identifying vectors (with optional *)
        /// </summary>
        public static readonly string VectorPattern4 = $@"({ScalarPattern4}{VectorNotationPattern})";

        /// <summary>
        /// Pattern for identifying scalars and vectors (no ~ or *)
        /// </summary>
        public static readonly string VariablePattern = $@"({VectorPattern2}|{ScalarPattern2})";

        /// <summary>
        /// Pattern for identifying constant notation
        /// </summary>
        private static readonly string ConstantNotationPattern = @"((?<BitCount>\d{1,2})?'(((?<Format>[hH])(?<Value>[a-fA-F\d]+))|((?<Format>[dD])(?<Value>\d+))|((?<Format>[bB])(?<Value>[0-1]+))))";

        /// <summary>
        /// Pattern for identifying constants (when creating tokens)
        /// </summary>
        public static readonly string ConstantPattern = $@"^(~*{ConstantNotationPattern})$";

        /// <summary>
        /// Pattern for identifying scalars, vectors and constants (no ~ or *)
        /// </summary>
        public static readonly string AnyVariablePattern = $@"({VectorPattern2}|{ConstantNotationPattern}|{ScalarPattern2})";

        /// <summary>
        /// Pattern for identifying concatenations (no constants)
        /// </summary>
        public static readonly string ConcatenationPattern = $@"(\{{(?<Vars>{VariablePattern}(\s+{VariablePattern})*)\}})";

        /// <summary>
        /// Pattern for identifying concatenations (with constants)
        /// </summary>
        public static readonly string ConcatenationPattern2 = $@"(\{{(?<Vars>{AnyVariablePattern}(\s+{AnyVariablePattern})*)\}})";

        /// <summary>
        /// Pattern for identifying format specifier notation
        /// </summary>
        private static readonly string FormatSpecifierNotationPattern = @"(%(?<Format>[ubhdUBHD]))";

        /// <summary>
        /// Pattern for identifying format specifiers (when creating tokens)
        /// </summary>
        public static readonly string FormatSpecifierPattern = $@"^{FormatSpecifierNotationPattern}$";

        /// <summary>
        /// Pattern for identifying entire format specifiers
        /// </summary>
        public static readonly string FormatSpecifierPattern2 = $@"({FormatSpecifierNotationPattern}{ConcatenationPattern})";

        /// <summary>
        /// Pattern for identifying extra spacing
        /// </summary>
        public static readonly string SpacingPattern = @"(^\s+|(?<=\s)\s+)";

        /// <summary>
        /// Pattern for identifying comment statements
        /// </summary>
        private static readonly string CommentPattern = @"^((?<Spacing>\s*)(?<DoInclude>[+-])?(?<Comment>"".*""\;))$";

        /// <summary>
        /// Pattern for identifying library statements
        /// </summary>
        private static readonly string LibraryPattern = @"^(#library\s(?<Name>.+);)$";

        /// <summary>
        /// Pattern for identifying module components
        /// </summary>
        private static readonly string ModuleComponentPattern = $@"({ConcatenationPattern}|{VariablePattern})";

        /// <summary>
        /// Pattern for identifying inputs or outputs in a module notation
        /// </summary>
        private static readonly string ModuleNotationPattern = $@"({ModuleComponentPattern}(,\s+{ModuleComponentPattern})*)";

        /// <summary>
        /// Pattern for identifying inputs and outputs in a module notation
        /// </summary>
        public static readonly string ModuleNotationPattern2 = $@"(?<Components>(?<Inputs>{ModuleNotationPattern})\s+:\s+(?<Outputs>{ModuleNotationPattern}))";

        /// <summary>
        /// Pattern for identifying submodule instantiations
        /// </summary>
        public static readonly string InstantiationNotationPattern = @"(?<Design>\w+)\.(?<Name>\w+)";

        /// <summary>
        /// Pattern for identifying submodule instantiations (when creating tokens)
        /// </summary>
        public static readonly string InstantiationPattern = $@"^{InstantiationNotationPattern}$";

        /// <summary>
        /// Pattern for identifying submodule instantiations (variable lists)
        /// </summary>
        public static readonly string InstantiationPattern2 = $@"({ModuleNotationPattern})";

        /// <summary>
        /// Pattern for identifying operators (when creating tokens)
        /// </summary>
        private static readonly string OperatorPattern = @"^(([=+^|-])|(<=)|(~+)|(==))$";

        /// <summary>
        /// Pattern for identifying seperators (when creating tokens)
        /// </summary>
        private static readonly string SeperatorPattern = @"[\s{}():,;]";

        /// <summary>
        /// Pattern for identifying invalid characters (when creating tokens)
        /// </summary>
        private static readonly string InvalidPattern = @"[^\s_a-zA-Z0-9~%^*()=+[\]{}|;'#<>,.-]";

        /// <summary>
        /// Regex for identifying scalars (when creating tokens)
        /// </summary>
        public static Regex ScalarRegex { get; } = new Regex(ScalarPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalars (when initializing variables)
        /// </summary>
        public static Regex ScalarRegex2 { get; } = new Regex(ScalarPattern4, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vectors (when creating tokens)
        /// </summary>
        public static Regex VectorRegex { get; } = new Regex(VectorPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vectors (no ~ or *)
        /// </summary>
        public static Regex VectorRegex2 { get; } = new Regex(VectorPattern2, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vectors (with optional *)
        /// </summary>
        public static Regex VectorRegex3 { get; } = new Regex(VectorPattern4, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying constants (when creating tokens)
        /// </summary>
        public static Regex ConstantRegex { get; } = new Regex(ConstantPattern);

        /// <summary>
        /// Regex for identifying scalars, vectors and constants
        /// </summary>
        private static Regex AnyVariableRegex = new Regex(AnyVariablePattern);

        /// <summary>
        /// Regex for identifying whether expansion is necessary
        /// </summary>
        private static Regex ExpansionRegex = new Regex($@"(({VectorPattern2}(?![^{{}}]*\}}))|({ConstantNotationPattern}(?![^{{}}]*\}}))|{ConcatenationPattern2})", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying format specifiers (when creating tokens)
        /// </summary>
        private static Regex FormatSpecifierRegex = new Regex(FormatSpecifierPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying entire format specifiers
        /// </summary>
        public static Regex FormatSpecifierRegex2 { get; } = new Regex(FormatSpecifierPattern2);

        /// <summary>
        /// Regex for identifying comment statements
        /// </summary>
        public static Regex CommentRegex { get; } = new Regex(CommentPattern);

        /// <summary>
        /// Regex for identifying library statements
        /// </summary>
        private static Regex LibraryRegex = new Regex(LibraryPattern);

        /// <summary>
        /// Regex for identifying submodule instantiations
        /// </summary>
        private static Regex InstantiationRegex = new Regex(InstantiationPattern);

        /// <summary>
        /// Regex for identifying module declarations
        /// </summary>
        private Regex ModuleRegex;

        /// <summary>
        /// Regex for identifying operators (used when creating tokens)
        /// </summary>
        private static Regex OperatorRegex = new Regex(OperatorPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying seperators (used when creating tokens)
        /// </summary>
        private static Regex SeperatorRegex = new Regex(SeperatorPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying invalid characters (used when creating tokens)
        /// </summary>
        private static Regex InvalidRegex = new Regex(InvalidPattern, RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// List of operators.
        /// </summary>
        public static readonly IList<string> OperatorsList = new ReadOnlyCollection<string>(new List<string>{"^", "|", "+", "-", "==", " ", "~"});
        public static readonly IList<string> ExclusiveOperatorsList = new ReadOnlyCollection<string>(new List<string>{"^", "+", "-", "=="});

        /// <summary>
        /// List of libraries included for this instance.
        /// </summary>
        private List<string> Libraries;

        /// <summary>
        /// Dictionary of submodules.
        /// </summary>
        private Dictionary<string, Design> Submodules;

        /// <summary>
        /// Dictionary of submodule instantiations.
        /// </summary>
        private Dictionary<string, Instantiation> Instantiations;

        /// <summary>
        /// The design being parsed.
        /// </summary>
        private Design Design;

        /// <summary>
        /// Line number of the design being parsed.
        /// </summary>
        private int LineNumber;

        /// <summary>
        /// Memo for vector expansions.
        /// </summary>
        private static Dictionary<string, List<string>> ExpansionMemo = new Dictionary<string, List<string>>();

        /// <summary>
        /// Indicates whether the parser is ticking.
        /// </summary>
        private bool Tick;

        /// <summary>
        /// Indicates whether the parser is initializing.
        /// </summary>
        private bool Init;

        /// <summary>
        /// Constructs a parser for the provided design.
        /// </summary>
        public Parser()
        {
            Libraries = new List<string>();
            Submodules = new Dictionary<string, Design>();
            Instantiations = new Dictionary<string, Instantiation>();
        }

        #region Parsing Methods

        /// <summary>
        /// Gets the parsed output from the provided statement list
        /// </summary>
        /// <param name="stmtList">List of statements</param>
        /// <returns>Parsed output</returns>
        private List<IObjectCodeElement> GetParsedOutput(List<Statement> stmtList)
        {
            // Parse statements for output
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

        /// <summary>
        /// Parsers the provided design text into output.
        /// </summary>
        /// <param name="design">Design to parse</param>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> Parse(Design design)
        {
            // Init parser
            Design = design;
            Tick = false;
            Init = true;
            Globals.Logger.Start();

            // Get statements for parsing
            Design.Database = new Database();
            List<Statement> stmtList = ParseStatements();
            if (stmtList == null)
            {
                Globals.Logger.Display();
                return null;
            }

            // Parse statements for output
            return GetParsedOutput(stmtList);
        }

        /// <summary>
        /// Parses the provided design text and clicks the provided variable.
        /// </summary>
        /// <param name="design">Design to parse</param>
        /// <param name="variableName">Variable clicked</param>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseClick(Design design, string variableName)
        {
            // Init parser
            Design = design;
            Tick = false;
            Init = false;
            Globals.Logger.Start();

            // Get statements for parsing
            Design.Database.VariableClicked(variableName);
            List<Statement> stmtList = ParseStatements();
            if (stmtList == null)
            {
                Globals.Logger.Display();
                return null;
            }

            // Parse statements for output
            return GetParsedOutput(stmtList);
        }

        /// <summary>
        /// Parsers the provided design text and ticks.
        /// </summary>
        /// <param name="design">Design to parse</param>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseTick(Design design)
        {
            // Init parser
            Design = design;
            Tick = true;
            Init = false;
            Globals.Logger.Start();

            // Get statements for parsing
            List<Statement> stmtList = ParseStatements();
            if (stmtList == null)
            {
                Globals.Logger.Display();
                return null;
            }

            // Set delay values
            foreach (Statement stmt in stmtList)
            {
                if (stmt.GetType() == typeof(DffClockStmt))
                {
                    ((DffClockStmt)stmt).Tick();
                }
            }
            // Reevlaute booleans
            foreach (Statement stmt in stmtList)
            {
                if (stmt.GetType() == typeof(BooleanAssignmentStmt))
                {
                    ((BooleanAssignmentStmt)stmt).Evaluate();
                }
            }

            // Parse statements for output
            return GetParsedOutput(stmtList);
        }

        /// <summary>
        /// Parsers the provided design text with input variables.
        /// </summary>
        /// <param name="design">Design to parse</param>
        /// <param name="inputs">Input variables</param>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseWithInput(Design design, List<Variable> inputs)
        {
            // Init parser
            Design = design;
            Tick = false;
            Init = true;
            Globals.Logger.Start();

            // Get statements for parsing
            Design.Database = new Database();
            foreach (Variable var in inputs)
            {
                if (var.GetType() == typeof(IndependentVariable))
                {
                    Design.Database.AddVariable((IndependentVariable)var);
                }
                else
                {
                    Design.Database.AddVariable(new IndependentVariable(var.Name, var.Value));
                }
            }
            List<Statement> stmtList = ParseStatements();
            if (stmtList == null)
            {
                Globals.Logger.Display();
                return null;
            }

            // Parse statements for output
            return GetParsedOutput(stmtList);
        }

        /// <summary>
        /// Returns a list of statements from parsed source code.
        /// </summary>
        /// <returns>List of statements</returns>
        private List<Statement> ParseStatements()
        {
            bool valid = true;
            List<Statement> statements = new List<Statement>();
            ModuleRegex = new Regex($@"^\s*{Design.FileName}\({ModuleNotationPattern2}\);$");
            string moduleDeclaration = "";

            byte[] bytes = Encoding.UTF8.GetBytes(Design.Text);
            MemoryStream stream = new MemoryStream(bytes);
            using (StreamReader reader = new StreamReader(stream))
            {
                LineNumber = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    LineNumber++;
                    line = line.TrimEnd(); // Trim end of line
                    StatementType? type = GetStatementType(line);
                    if (type != StatementType.Comment)
                    {
                        line = line.Replace("**", ""); // Remove double negatives
                        line = line.Replace("~~", ""); // Remove double negatives
                    }

                    if (type == null)
                    {
                        valid = false;
                    }
                    else if (type == StatementType.Library)
                    {
                        if (!VerifyLibraryStatement(line))
                        {
                            valid = false;
                        }
                        continue;
                    }
                    else if (type == StatementType.Module)
                    {
                        if (!String.IsNullOrEmpty(moduleDeclaration))
                        {
                            Globals.Logger.Add($"Line {LineNumber}: A module declaration already exists.");
                            valid = false;
                        }
                        else
                        {
                            if (!ModuleRegex.IsMatch(line))
                            {
                                valid = false;
                            }
                            else
                            {
                                moduleDeclaration = line;
                            }
                        }

                        continue;
                    }

                    if (valid)
                    {
                        // Create statements
                        if (type == StatementType.Empty)
                        {
                            statements.Add(new EmptyStmt(Design.Database, line));
                        }
                        else if (type == StatementType.Comment)
                        {
                            Match match = CommentRegex.Match(line);

                            if (match.Groups["DoInclude"].Value != "-" && (Properties.Settings.Default.SimulationComments || match.Groups["DoInclude"].Value == "+"))
                            {
                                line = String.Concat(match.Groups["Spacing"].Value, match.Groups["Comment"].Value); // Remove + or -
                                statements.Add(new CommentStmt(Design.Database, line));
                            }
                        }
                        else if (type == StatementType.Submodule)
                        {
                            string instantiationName = Regex.Match(line, InstantiationNotationPattern).Groups["Name"].Value;

                            line = ExpandHorizontally(line);
                            if (line == null)
                            {
                                return null;
                            }

                            statements.Add(new SubmoduleInstantiationStmt(Design.Database, line, Instantiations[instantiationName]));
                        }
                        else
                        {
                            bool needsExpansion = ExpansionRegex.IsMatch(line);
                            if (needsExpansion && line.Contains("="))
                            {
                                // Vertical expansion needed
                                line = ExpandVertically(line);
                                if (line == null)
                                {
                                    return null;
                                }
                            }
                            else if (needsExpansion)
                            {
                                // Horizontal expansion needed
                                if (type == StatementType.FormatSpecifier)
                                {
                                    MatchCollection matches = FormatSpecifierRegex2.Matches(line);
                                    foreach (Match match in matches)
                                    {
                                        string expanded = ExpandHorizontally(match.Groups["Vars"].Value);
                                        if (expanded == null)
                                        {
                                            return null;
                                        }

                                        string formatSpecifier = match.Value.Replace(match.Groups["Vars"].Value, expanded);
                                        line = String.Concat(line.Substring(0, match.Index), formatSpecifier, line.Substring(match.Index + match.Length));
                                    }
                                }
                                else
                                {
                                    line = ExpandHorizontally(line);
                                }
                            }

                            // Add statement for each expansion of line
                            string[] lines = line.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string source in lines)
                            {
                                if (Init && !InitVariables(type, source))
                                {
                                    // Unable to initialize variables
                                    valid = false;
                                }

                                if (type == StatementType.Boolean)
                                {
                                    statements.Add(new BooleanAssignmentStmt(Design.Database, source));
                                }
                                else if (type == StatementType.Clock)
                                {
                                    statements.Add(new DffClockStmt(Design.Database, source, Tick));
                                }
                                else if (type == StatementType.FormatSpecifier)
                                {
                                    statements.Add(new FormatSpecifierStmt(Design.Database, source));
                                }
                                else if (type == StatementType.VariableList)
                                {
                                    statements.Add(new VariableListStmt(Design.Database, source));
                                }
                            }
                        }
                    }
                }
            }

            // If module declaration verify
            if (!String.IsNullOrEmpty(moduleDeclaration) && !VerifyModuleDeclarationStatement(moduleDeclaration))
            {
                valid = false;
            }

            if (!valid)
            {
                return null;
            }
            else
            {
                return statements;
            }
        }

        #endregion

        

        /// <summary>
        /// Returns the type of statement for a given line.
        /// </summary>
        /// <param name="line">Line to interpret</param>
        /// <returns>Type of statement</returns>
        private StatementType? GetStatementType(string line)
        {
            StatementType? type = StatementType.Empty;
            List<string> tokens = new List<string>();
            Stack<char> groupings = new Stack<char>();
            StringBuilder lexeme = new StringBuilder();
            string currentLexeme;
            string newChar;

            line = line.TrimEnd();
            if (String.IsNullOrWhiteSpace(line))
            {
                return type;
            }

            if (line[line.Length - 1] != ';')
            {
                Globals.Logger.Add($"Line {LineNumber}: Missing ';'.");
                return null;
            }

            foreach (char c in line)
            {
                newChar = c.ToString();
                currentLexeme = lexeme.ToString();

                if (type == StatementType.Comment || type == StatementType.Library)
                {
                    lexeme.Append(c);
                }
                else if (currentLexeme == "#library")
                {
                    type = StatementType.Library;
                }
                else if (c == '"')
                {
                    // Make sure current lexeme is empty, + or -
                    if (!(currentLexeme == "+" || currentLexeme == "-" || currentLexeme == ""))
                    {
                        Globals.Logger.Add($"Line {LineNumber}: Invalid '\"'.");
                        return null;
                    }

                    // Make sure no other tokens exist
                    if (tokens.Any(token => token != " "))
                    {
                        Globals.Logger.Add($"Line {LineNumber}: Invalid '\"'.");
                        return null;
                    }

                    type = StatementType.Comment;
                    lexeme.Append(c);
                }
                else if (SeperatorRegex.IsMatch(newChar))
                {
                    // Ending characters
                    if (currentLexeme.Length > 0)
                    {
                        if (IsToken(currentLexeme))
                        {
                            // Check for invalid tokens with current statement type
                            if (currentLexeme == "=")
                            {
                                if (type != StatementType.Empty)
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: '{lexeme}' can only be used after the dependent in a boolean statement.");
                                    return null;
                                }
                                else
                                {
                                    type = StatementType.Boolean;
                                }
                            }
                            else if (currentLexeme == "<=")
                            {
                                if (type != StatementType.Empty)
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: '{lexeme}' can only be used after the dependent in a clock statement.");
                                    return null;
                                }
                                else
                                {
                                    type = StatementType.Clock;
                                }
                            }
                            else if (Regex.IsMatch(currentLexeme, @"^([+|^-])|(==)$"))
                            {
                                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: '{lexeme}' operator can only be used in a boolean or clock statement.");
                                    return null;
                                }
                            }
                            else if (currentLexeme.Contains("%"))
                            {
                                if (type != StatementType.Empty && type != StatementType.FormatSpecifier)
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: '{lexeme}' can only be used in a format specifier statement.");
                                    return null;
                                }
                                else if (type == StatementType.Empty)
                                {
                                    type = StatementType.FormatSpecifier;
                                }
                            }
                            else if (currentLexeme.Contains("~"))
                            {
                                if (currentLexeme == "~" && c != '(' && c != '{')
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: '~' must be attached to a scalar, vector, constant, parenthesis or concatenation.");
                                    return null;
                                }

                                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: '~' can only be used in on the right side of a boolean or clock statement.");
                                    return null;
                                }

                                if (groupings.Count > 0 && groupings.Peek() == '{')
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: '~' can't be used inside a concatenation field.");
                                    return null;
                                }
                            }
                            else if (currentLexeme.Contains("*"))
                            {
                                if (type != StatementType.Empty)
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: '*' can only be used in a variable list statement.");
                                    return null;
                                }
                            }
                            else if (currentLexeme.Contains("'"))
                            {
                                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: Constants can only be used on the right side of a boolean or clock statement.");
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            if (type != StatementType.Empty || c != '(' || !InstantiationRegex.IsMatch(currentLexeme))
                            {
                                // If token is not valid and is not an instantiation
                                Globals.Logger.Add($"Line {LineNumber}: Invalid '{currentLexeme}'.");
                                return null;
                            }
                        }

                        tokens.Add(currentLexeme);
                        lexeme.Clear();
                    }

                    // Validate ending token
                    if (c == '{' || c == '}')
                    {
                        if (type == StatementType.Empty)
                        {
                            Globals.Logger.Add($"Line {LineNumber}: Concatenations can't be used in a variable list statement.");
                            return null;
                        }

                        if (c == '{' && groupings.Count > 0 && groupings.Peek() == '{')
                        {
                            Globals.Logger.Add($"Line {LineNumber}: Concatenations can't be used inside of other concatenations.");
                            return null;
                        }
                    }
                    else if (c == '(' || c == ')')
                    {
                        if (groupings.Count > 0 && groupings.Peek() == '{')
                        {
                            Globals.Logger.Add($"Line {LineNumber}: '{c}' can't be used in a concatenation.");
                            return null;
                        }

                        if (currentLexeme == Design.FileName)
                        {
                            type = StatementType.Module;
                        }
                        else if (InstantiationRegex.IsMatch(currentLexeme))
                        {
                            Match instantiation = InstantiationRegex.Match(currentLexeme);
                            string designName = instantiation.Groups["Design"].Value;
                            string instantiationName = instantiation.Groups["Name"].Value;

                            if (designName == Design.FileName)
                            {
                                Globals.Logger.Add($"Line {LineNumber}: You cannot instantiate from the current design.");
                                return null;
                            }

                            if (Instantiations.ContainsKey(instantiationName))
                            {
                                Globals.Logger.Add($"Line {LineNumber}: Instantiation name '{instantiationName}' is already being used.");
                                return null;
                            }

                            // Find design (First look in current design directory, then libraries)
                            try
                            {
                                if (!Submodules.ContainsKey(designName))
                                {
                                    bool foundDeclaration = false;
                                    string[] files = Directory.GetFiles(Design.FileSource.DirectoryName, String.Concat(designName, ".vbi"));
                                    if (files.Length > 0)
                                    {
                                        // Check for module Declaration
                                        foundDeclaration = DesignHasModuleDeclaration(files[0], line);
                                    }

                                    if (!foundDeclaration)
                                    {
                                        for (int i = 0; i < Libraries.Count; i++)
                                        {
                                            files = Directory.GetFiles(Libraries[i], String.Concat(designName, ".vbi"));
                                            if (files.Length > 0)
                                            {
                                                // Check for module Declaration
                                                foundDeclaration = DesignHasModuleDeclaration(files[0], line);
                                                if (foundDeclaration)
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                        if (!foundDeclaration)
                                        {
                                            // Not found
                                            Globals.Logger.Add($"Line {LineNumber}: Unable to find '{designName}'.");
                                            return null;
                                        }
                                    }

                                    // At this point, the design was found
                                    Submodules.Add(designName, new Design(files[0], delegate { }));
                                    Instantiations.Add(instantiationName, new Instantiation(instantiation.Value, files[0]));
                                }
                                else
                                {
                                    Instantiations.Add(instantiationName, new Instantiation(instantiation.Value, Submodules[designName].FileSource.FullName));
                                }
                            }
                            catch (Exception)
                            {
                                Globals.Logger.Add($"Line {LineNumber}: Error locating '{Design}'.");
                                return null;
                            }

                            type = StatementType.Submodule;
                        }

                        if (type == StatementType.FormatSpecifier || type == StatementType.Empty)
                        {
                            Globals.Logger.Add($"Line {LineNumber}: '{c}' can't be used in a format specifier or variable list statement.");
                            return null;
                        }
                    }
                    else if (c == ';')
                    {
                        if (tokens.Count == 0 || tokens.Contains(";"))
                        {
                            Globals.Logger.Add($"Line {LineNumber}: ';' can only be used to end a statement.");
                            return null;
                        }

                        if (tokens.Last() == " ")
                        {
                            Globals.Logger.Add($"Line {LineNumber}: Spaces cannot occur before ';'.");
                            return null;
                        }
                    }
                    else if (c == ',')
                    {
                        // Check for misplaced comma
                        if (groupings.Count == 0)
                        {
                            Globals.Logger.Add($"Line {LineNumber}: ',' can only be used inside the () in a submodule or module statement.");
                            return null;
                        }
                        else
                        {
                            char top = groupings.Peek();

                            if (!((type == StatementType.Submodule || type == StatementType.Module) && top == '('))
                            {
                                Globals.Logger.Add($"Line {LineNumber}: ',' can only be used inside the () in a submodule or module statement.");
                                return null;
                            }
                        }
                    }
                    else if (c == ':')
                    {
                        if (type != StatementType.Module || groupings.Count == 0 || groupings.Peek() != '(')
                        {
                            Globals.Logger.Add($"Line {LineNumber}: ':' can only be used to seperate input and output variables in a module declaration statement.");
                            return null;
                        }

                        if (tokens.Contains(":"))
                        {
                            Globals.Logger.Add($"Line {LineNumber}: ':' can only be used once in a module declaration statement.");
                            return null;
                        }
                    }

                    // Add groupings
                    if (c == '{' || c == '(')
                    {
                        // Add grouping char to stack
                        groupings.Push(c);
                    }
                    else if (c == '}' || c == ')')
                    {
                        // Check for correct closing
                        if (groupings.Count > 0)
                        {
                            char top = groupings.Peek();
                            if ((c == ')' && top == '(') || (c == '}' && top == '{'))
                            {
                                groupings.Pop();
                            }
                            else
                            {
                                Globals.Logger.Add($"Line {LineNumber}: '{top}' must be matched first.");
                                return null;
                            }
                        }
                        else
                        {
                            Globals.Logger.Add($"Line {LineNumber}: Unmatched '{c}'.");
                            return null;
                        }
                    }

                    tokens.Add(c.ToString());
                }
                else if (InvalidRegex.IsMatch(newChar))
                {
                    // Invalid char
                    Globals.Logger.Add($"Line {LineNumber}: Invalid character '{c}'.");
                    return null;
                }
                else
                {
                    // Appending characters
                    // Check for constant inside {}
                    if (c == '\'')
                    {
                        // Check for constant bit count inside {}
                        if (groupings.Count > 0 && groupings.Peek() == '{' && (String.IsNullOrEmpty(currentLexeme) || !currentLexeme.All(ch => Char.IsDigit(ch))))
                        {
                            Globals.Logger.Add($"Line {LineNumber}: Constants in concatenation fields must specify bit count.");
                            return null;
                        }
                    }

                    lexeme.Append(c);
                }
            }

            // Check for unclosed groupings
            if (groupings.Count > 0)
            {
                foreach (char grouping in groupings)
                {
                    Globals.Logger.Add($"Line {LineNumber}: '{grouping}' is not matched.");
                }
                return null;
            }

            // At this point, if type is Empty, type should be set to VariableList
            if (type == StatementType.Empty)
            {
                type = StatementType.VariableList;
            }

            // Verify line from its statement type
            if (!VerifyStatement(line, type))
            {
                return null;
            }

            return type;
        }

        #region Token Verifications

        /// <summary>
        /// Returns whether a lexeme is a token.
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <returns>Whether the lexeme is a token</returns>
        private bool IsToken(string lexeme)
        {
            if (IsScalar(lexeme))
            {
                return true;
            }
            else if (IsVector(lexeme))
            {
                return true;
            }
            else if (OperatorRegex.IsMatch(lexeme))
            {
                return true;
            }
            else if (IsConstant(lexeme))
            {
                return true;
            }
            else if (FormatSpecifierRegex.IsMatch(lexeme))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether a lexeme is a scalar. (If so, initializes it)
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <returns>Whether the lexeme is a scalar</returns>
        private bool IsScalar(string lexeme)
        {
            Match match = ScalarRegex.Match(lexeme);
            if (match.Success)
            {
                // Check scalar name has at least one letter
                if (!lexeme.Any(c => char.IsLetter(c)))
                {
                    Globals.Logger.Add($"Line {LineNumber}: Invalid scalar name.");
                    return false;
                }

                if (Design.Database.HasVectorNamespace(match.Groups["Name"].Value))
                {
                    Globals.Logger.Add($"Line {LineNumber}: {match.Groups["Name"].Value} already exists as a vector and can't be used as a scalar.");
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether a lexeme is a vector. (If so, initializes it)
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <returns>Whether the lexeme is a vector</returns>
        private bool IsVector(string lexeme)
        {
            Match match = VectorRegex.Match(lexeme);
            if (match.Success)
            {
                // Check for invalid vector namespace name
                string vectorNamespace = match.Groups["Name"].Value;
                if (Char.IsDigit(vectorNamespace[vectorNamespace.Length - 1]))
                {
                    Globals.Logger.Add($"Line {LineNumber}: Vector namespaces cannot end in a number.");
                    return false;
                }

                // Check vector bounds and step
                int leftBound = String.IsNullOrEmpty(match.Groups["LeftBound"].Value) ? -1 : Convert.ToInt32(match.Groups["LeftBound"].Value);
                int step = String.IsNullOrEmpty(match.Groups["Step"].Value) ? -1 : Convert.ToInt32(match.Groups["Step"].Value);
                int rightBound = String.IsNullOrEmpty(match.Groups["RightBound"].Value) ? -1 : Convert.ToInt32(match.Groups["RightBound"].Value);
                if (leftBound > 31 || rightBound > 31 || step > 31)
                {
                    Globals.Logger.Add($"Line {LineNumber}: Vector bounds and step must be between 0 and 31.");
                    return false;
                }

                if (Init)
                {
                    // Check if namespace is used by a scalar
                    if (Design.Database.TryGetVariable<Variable>(vectorNamespace) != null)
                    {
                        Globals.Logger.Add($"Line {LineNumber}: A scalar with the name '{vectorNamespace}' already exists.");
                        return false;
                    }

                    // Check for existing vector namespace
                    if (lexeme.Contains('~'))
                    {
                        lexeme = lexeme.TrimStart('~');
                    }
                    else if (lexeme.Contains('*'))
                    {
                        lexeme = lexeme.TrimStart('*');
                    }
                    List<string> expandedList = new List<string>();
                    if (leftBound != -1)
                    {
                        if (ExpansionMemo.ContainsKey(lexeme))
                        {
                            expandedList = ExpansionMemo[lexeme];
                        }
                        else
                        {
                            expandedList = ExpandVector(VectorRegex.Match(lexeme));
                        }
                    }

                    if (Design.Database.HasVectorNamespace(vectorNamespace))
                    {
                        // These ifs cannot be combined
                        if (leftBound != -1)
                        {
                            List<string> vectorComponents = Design.Database.GetVectorComponents(vectorNamespace);
                            int intersectCount = vectorComponents.Intersect(expandedList).Count();

                            if (!(intersectCount > 0 && intersectCount == expandedList.Count()))
                            {
                                // Display error if the referenced components aren't a subset of the components in the database
                                Globals.Logger.Add($"Line {LineNumber}: Vector namespace {vectorNamespace} is already defined. Only the following components: {String.Join(" ", vectorComponents)} can be referenced.");
                                return false;
                            }
                        }
                        // Else isn't needed. Vector namespace[] notation will never reference outside components
                    }
                    else
                    {
                        if (leftBound == -1)
                        {
                            // Vector can't be declared as namespace[]
                            Globals.Logger.Add($"Line {LineNumber}: '{lexeme}' notation cannot be used before the vector is initialized.");
                            return false;
                        }
                        Design.Database.AddVectorNamespace(vectorNamespace, expandedList);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether a lexeme is a constant.
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <returns>Whether the lexeme is a constant</returns>
        private bool IsConstant(string lexeme)
        {
            Match match = ConstantRegex.Match(lexeme);
            if (match.Success)
            {
                // Check bit count
                if (!String.IsNullOrEmpty(match.Groups["BitCount"].Value) && Convert.ToInt32(match.Groups["BitCount"].Value) > 32)
                {
                    Globals.Logger.Add($"Line {LineNumber}: Constant can have at most 32 bits.");
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Statement Verifications

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private bool VerifyStatement(string line, StatementType? type)
        {
            if (type == StatementType.Boolean || type == StatementType.Clock)
            {
                // Verify expressions
                int start = line.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
                string dependent = line.Contains("<")
                    ? line.Substring(start, line.IndexOf('<') - start).Trim()
                    : line.Substring(start, line.IndexOf('=') - start).Trim();
                string expression = line.Substring(line.IndexOf("=") + 1).Trim();
                expression = expression.TrimEnd(';');
                expression = String.Concat("(", expression, ")");

                if (expression.Contains(dependent) && expression.Contains("=="))
                {
                    Globals.Logger.Add($"Line {LineNumber}: Circular Dependency Found.");
                    return false;
                }

                if (!VerifyExpressionStatement(expression))
                {
                    return false;
                }
            }
            else if (type == StatementType.Comment)
            {
                if (!CommentRegex.IsMatch(line))
                {
                    Globals.Logger.Add($"Line {LineNumber}: Invalid comment statement.");
                    return false;
                }
            }
            else if (type == StatementType.FormatSpecifier)
            {
                return VerifyFormatSpeciferStatement(line);
            }
            /*
            else if (type == StatementType.Submodule)
            {
                return VerifySubmoduleInstantiationStatement(line);
            }
            */

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
            List<StringBuilder> expressions = new List<StringBuilder>();
            List<List<string>> expressionOperators = new List<List<string>>();
            List<string> expressionExclusiveOperators = new List<string>();

            // And operator: (?<=\w|\))\s+(?=[\w(~'])
            MatchCollection matches = Regex.Matches(expression, @"([_a-zA-Z]\w{0,19})|([~^()|+-])|(==)|(?<=\w|\))\s+(?=[\w(~'])");
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
                        Globals.Logger.Add($"Line {LineNumber}: Empty ().");
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
                        Globals.Logger.Add($"Line {LineNumber}: An operator is missing its operands.");
                        return false;
                    }

                    // Check exclusive operator for errors
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
                            else
                            {
                                pastOperators = pastOperators.Where(o => o != "+" && o != "-").ToList();
                            }

                            if (pastOperators.Count > 0)
                            {
                                if (token == "+" || token == "-")
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: '{token}' can only appear with '+' or '-' in its parentheses level.");
                                }
                                else
                                {
                                    Globals.Logger.Add($"Line {LineNumber}: '{token}' must be the only operator in its parentheses level.");
                                }
                                return false;
                            }

                            expressionExclusiveOperators[expressionExclusiveOperators.Count - 1] = token;
                        }
                    }
                    else if (exclusiveOperator == "+" || exclusiveOperator == "-")
                    {
                        if (!(token == "+" || token == "-"))
                        {
                            Globals.Logger.Add($"Line {LineNumber}: '{exclusiveOperator}' can only appear with '+' or '-' in its parentheses level.");
                            return false;
                        }
                    }
                    else
                    {
                        if (token != exclusiveOperator)
                        {
                            Globals.Logger.Add($"Line {LineNumber}: '{exclusiveOperator}' must be the only operator in its parentheses level.");
                            return false;
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
        /// Verifies a format specifier statement
        /// </summary>
        /// <param name="line">Line to verify</param>
        /// <returns>Whether the line is valid or not</returns>
        private bool VerifyFormatSpeciferStatement(string line)
        {
            if (Regex.IsMatch(line, $@"(?<!%)(?![^{{}}]*\}})({VariablePattern})"))
            {
                // If line contains a variable outside {}
                Globals.Logger.Add($"Line {LineNumber}: Scalars and vectors in a format specifier statement must be inside a format specifier.");
                return false;
            }

            // Check for formats inside formats or formats without variables
            MatchCollection formats = FormatSpecifierRegex2.Matches(line);
            if (formats.Count != line.Count(c => c == '%'))
            {
                Globals.Logger.Add($"Line {LineNumber}: Invalid format specifier.");
                return false;
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
            string library = LibraryRegex.Match(line).Groups["Name"].Value;
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
                    Globals.Logger.Add($"Line {LineNumber}: Library '{path}' doesn't exist or is invalid.");
                    return false;
                }
            }
            catch (Exception)
            {
                Globals.Logger.Add($"Line {LineNumber}: Invalid library name '{library}'.");
                return false;
            }
        }

        /// <summary>
        /// Returns whether the design contains a module Declaration.
        /// </summary>
        /// <param name="path">Path of the design</param>
        /// <param name="instantiation">Submodule instantiation</param>
        /// <returns>Whether the design contains a module Declaration</returns>
        private bool DesignHasModuleDeclaration(string path, string instantiation)
        {
            FileInfo fileInfo = new FileInfo(path);
            string name = fileInfo.Name.Split('.')[0];
            Regex declarationRegex = new Regex($@"^\s*{name}\({ModuleNotationPattern2}\);$");
            using (StreamReader reader = fileInfo.OpenText())
            {
                string nextLine = string.Empty;
                while ((nextLine = reader.ReadLine()) != null)
                {
                    Match match = declarationRegex.Match(nextLine);
                    if (match.Success)
                    {
                        // Validate it before return true
                        if (VerifySubmoduleInstantiationStatement(instantiation, match))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Verifies a submodule instantiation statement
        /// </summary>
        /// <param name="instantiation">Instantiation to verifiy</param>
        /// <param name="declaration">Declaration</param>
        /// <returns>Whether the instantiation is valid or not</returns>
        private bool VerifySubmoduleInstantiationStatement(string instantiation, Match declaration)
        {
            List<string> components = Regex.Split(declaration.Groups["Inputs"].Value, @",\s+").ToList();
            components.AddRange(Regex.Split(declaration.Groups["Outputs"].Value, @",\s+"));
            MatchCollection matches = Regex.Matches(instantiation.Substring(instantiation.IndexOf("(")), ModuleComponentPattern);

            if (matches.Count != components.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    List<string> instantiationExpansion = GetExpansions(ExpansionRegex.Match(matches[i].Value)); // Needs scalar attached to regex
                    List<string> declarationExpansion = GetExpansions(ExpansionRegex.Match(components[i]));

                    if (instantiationExpansion.Count != declarationExpansion.Count)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Verifies a module declaration statement
        /// </summary>
        /// <param name="declaration">Declaration to verifiy</param>
        /// <returns>Whether the declaration is valid or not</returns>
        private bool VerifyModuleDeclarationStatement(string declaration)
        {
            // Get input and output variables
            Match module = ModuleRegex.Match(declaration);
            string[] inputVars = Regex.Split(module.Groups["Inputs"].Value, @",\s+");
            string[] outputVars = Regex.Split(module.Groups["Outputs"].Value, @",\s+");

            // Check input variables
            foreach (string input in inputVars)
            {
                List<string> vars;
                if (!input.Contains("{") && !input.Contains("["))
                {
                    vars = new List<string>();
                    vars.Add(input);
                }
                else
                {
                    vars = GetExpansions(ExpansionRegex.Match(input));
                }
                foreach (string var in vars)
                {
                    if (Design.Database.TryGetVariable<IndependentVariable>(var) == null)
                    {
                        Globals.Logger.Add($"Variable '{var}' cannot be used as an input in a module declaration statement.");
                        return false;
                    }
                }
            }

            // Check output variables
            foreach (string output in outputVars)
            {
                List<string> vars;
                if (!output.Contains("{") && !output.Contains("["))
                {
                    vars = new List<string>();
                    vars.Add(output);
                }
                else
                {
                    vars = GetExpansions(ExpansionRegex.Match(output));
                }
                foreach (string var in vars)
                {
                    if (var != "NC" && Design.Database.TryGetVariable<DependentVariable>(var) == null)
                    {
                        Globals.Logger.Add($"Variable '{var}' cannot be used as an output in a module declaration statement.");
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Initializes variables in a line.
        /// </summary>
        /// <param name="type">Type of statement</param>
        /// <param name="line">Line</param>
        /// <returns>Whether the operation was successful</returns>
        private bool InitVariables(StatementType? type, string line)
        {
            MatchCollection matches = ScalarRegex2.Matches(line);
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
                        if (Design.Database.TryGetVariable<Variable>(var) == null)
                        {
                            Design.Database.AddVariable<DependentVariable>(new DependentVariable(var, false));

                            if (line.Contains("<"))
                            {
                                // Create delay variable
                                var += ".d";
                                Design.Database.AddVariable<DependentVariable>(new DependentVariable(var, false));
                            }
                        }
                        else
                        {
                            if (Design.Database.TryGetVariable<IndependentVariable>(var) as IndependentVariable != null)
                            {
                                Design.Database.MakeDependent(var);
                            }

                            if (line.Contains("<"))
                            {
                                var += ".d";
                                if (Design.Database.TryGetVariable<Variable>(var) == null)
                                {
                                    // Delay doesn't exist
                                    Design.Database.AddVariable<DependentVariable>(new DependentVariable(var, false));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Design.Database.TryGetVariable<Variable>(var) == null)
                        {
                            IndependentVariable indVar = new IndependentVariable(var, false);
                            Design.Database.AddVariable<IndependentVariable>(indVar);
                        }
                    }
                }
                else
                {
                    bool val = var.Contains("*");
                    var = var.TrimStart('*'); // Remove * if present

                    if (Design.Database.TryGetVariable<Variable>(var) == null)
                    {
                        // Create Variable
                        IndependentVariable indVar = new IndependentVariable(var, val);
                        Design.Database.AddVariable<IndependentVariable>(indVar);
                    }
                    else if (type == StatementType.VariableList)
                    {
                        Globals.Logger.Add($"Line {LineNumber}: {var} has already been declared.");
                        return false; // You cannot declare a variable twice
                    }
                }
            }

            return true;
        }

        #region Expansion Methods

        /// <summary>
        /// Expands a vector into its components.
        /// </summary>
        /// <param name="vector">Vector to expand</param>
        /// <returns>List of vector components</returns>
        private List<string> ExpandVector(Match vector)
        {
            List<string> expansion = new List<string>();

            // Get vector name, bounds and step
            string name = vector.Value.Contains("*")
                ? String.Concat(vector.Value[0], vector.Groups["Name"].Value)
                : vector.Groups["Name"].Value;
            int leftBound = Convert.ToInt32(vector.Groups["LeftBound"].Value);
            int rightBound = Convert.ToInt32(vector.Groups["RightBound"].Value);
            if (leftBound < rightBound)
            {
                // Flips bounds so MSB is the leftBound
                leftBound = leftBound + rightBound;
                rightBound = leftBound - rightBound;
                leftBound = leftBound - rightBound;
            }
            int step = String.IsNullOrEmpty(vector.Groups["Step"].Value) ? -1 : -Convert.ToInt32(vector.Groups["Step"].Value);

            // Expand vector
            for (int i = leftBound; i >= rightBound; i += step)
            {
                expansion.Add(String.Concat(name, i));
            }

            // Save expansion
            if (!ExpansionMemo.ContainsKey(vector.Value))
            {
                ExpansionMemo.Add(vector.Value, expansion);
            }

            return expansion;
        }

        /// <summary>
        /// Expands a constant into its bit components.
        /// </summary>
        /// <param name="constant">Constant to expand</param>
        /// <returns>List of constant bits</returns>
        private List<string> ExpandConstant(Match constant)
        {
            List<string> expansion = new List<string>();
            string outputBinary;
            char[] charBits; // Converted binary bits as chars

            // Get binary bits from format type
            if (constant.Groups["Format"].Value == "h" || constant.Groups["Format"].Value == "H")
            {
                outputBinary = Convert.ToString(Convert.ToInt32(constant.Groups["Value"].Value, 16), 2);
                charBits = outputBinary.ToCharArray();
            }
            else if (constant.Groups["Format"].Value == "d" || constant.Groups["Format"].Value == "D")
            {
                outputBinary = Convert.ToString(Convert.ToInt32(constant.Groups["Value"].Value, 10), 2);
                charBits = outputBinary.ToCharArray();
            }
            else
            {
                charBits = constant.Groups["Value"].Value.ToCharArray();
            }

            int[] bits = Array.ConvertAll(charBits, bit => (int)Char.GetNumericValue(bit));
            int specifiedBitCount = String.IsNullOrEmpty(constant.Groups["BitCount"].Value)
                ? -1
                : Convert.ToInt32(constant.Groups["BitCount"].Value);

            if (specifiedBitCount != -1 && specifiedBitCount < bits.Length)
            {
                // Error
                Globals.Logger.Add($"Line {LineNumber}: {constant.Value} doesn't specify enough bits.");
                return null;
            }
            else if (specifiedBitCount > bits.Length)
            {
                // Add padding
                for (int i = 0; i < specifiedBitCount - bits.Length; i++)
                {
                    expansion.Add("'b0");
                }
            }

            foreach (int bit in bits)
            {
                expansion.Add(String.Concat("'b", bit));
            }

            // Save expansion
            if (!ExpansionMemo.ContainsKey(constant.Value))
            {
                ExpansionMemo.Add(constant.Value, expansion);
            }

            return expansion;
        }

        /// <summary>
        /// Exapnds a single token into its components.
        /// </summary>
        /// <param name="token">Token to expand</param>
        /// <returns>List of expansion components</returns>
        private List<string> ExpandToken(Match token)
        {
            if (token.Value.Contains("[") && String.IsNullOrEmpty(token.Groups["LeftBound"].Value))
            {
                List<string> components = Design.Database.GetVectorComponents(token.Groups["Name"].Value);
                if (token.Value.Contains("~") && !components[0].Contains("~"))
                {
                    for (int i = 0; i < components.Count; i++)
                    {
                        components[i] = String.Concat("~", components[i]);
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
        /// Expands tokens into components.
        /// </summary>
        /// <param name="tokens">Tokens to expand</param>
        /// <returns>List of expansion components</returns>
        private List<string> GetExpansions(Match tokens)
        {
            List<string> expansion = new List<string>();

            // Get token's variables
            string[] vars;
            if (tokens.Value.Contains("{"))
            {
                vars = Regex.Split(tokens.Value, @"\s+");
            }
            else
            {
                vars = new string[] {tokens.Value};
            }

            // Expand each variable
            foreach (string var in vars)
            {
                Match token = AnyVariableRegex.Match(var);

                if (token.Value.Contains("[") || token.Value.Contains("'"))
                {
                    expansion.AddRange(ExpandToken(token));
                }
                else
                {
                    expansion.Add(token.Value);
                }
            }

            return expansion;
        }

        /// <summary>
        /// Expands all concatenations and vectors in a line.
        /// </summary>
        /// <param name="line">Line to expand</param>
        /// <returns>Expanded line</returns>
        private string ExpandHorizontally(string line)
        {
            string expandedLine = line;

            // Expand all expansions
            while (ExpansionRegex.IsMatch(expandedLine))
            {
                Match match = ExpansionRegex.Match(expandedLine);

                // Get expansion
                List<string> expansion;
                if (!match.Value.Contains("{"))
                {
                    expansion = ExpandToken(match);
                }
                else
                {
                    expansion = GetExpansions(match);
                }

                // Check and combine expansion
                if (expansion == null)
                {
                    return null;
                }
                string expanded = String.Join(" ", expansion);

                // Replace with expansion
                expandedLine = expandedLine.Substring(0, match.Index) + expanded + expandedLine.Substring(match.Index + match.Length);
            }

            return expandedLine;
        }

        /// <summary>
        /// Expands a line into lines.
        /// </summary>
        /// <param name="line">Line to expand</param>
        /// <returns>Expanded lines</returns>
        private string ExpandVertically(string line)
        {
            string expanded = String.Empty;

            // Get dependent and expression
            int start = line.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            string dependent = line.Contains("<")
                ? line.Substring(start, line.IndexOf("<") - start).TrimEnd()
                : line.Substring(start, line.IndexOf("=") - start).TrimEnd();
            string expression = line.Substring(line.IndexOf("=") + 1).TrimStart();

            // Expand dependent
            List<string> dependentExpansion = new List<string>();
            Match dependentMatch = VectorRegex2.Match(dependent);
            if (dependentMatch.Success)
            {
                dependentExpansion.AddRange(ExpandToken(dependentMatch));
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
                if (!match.Value.Contains("{"))
                {
                    expressionExpansions.Add(ExpandToken(match));
                }
                else
                {
                    expressionExpansions.Add(GetExpansions(match));
                }
            }

            // Verify expansions
            foreach (List<string> expressionExpansion in expressionExpansions)
            {
                if (dependentExpansion.Count != expressionExpansion.Count)
                {
                    Globals.Logger.Add($"Line {LineNumber}: Vector and/or concatenation element counts must be consistent across the entire expression.");
                    return null;
                }
            }

            // Combine expansions
            List<List<string>> expansions = new List<List<string>>();
            expansions.Add(dependentExpansion);
            expansions.AddRange(expressionExpansions);

            // Expand line into lines
            for (int i = 0; i < dependentExpansion.Count; i++)
            {
                string newLine = line;
                newLine = newLine.Replace(dependent, expansions[0][i]); // Replace dependent
                int j = 1;
                foreach (Match match in matches)
                {
                    newLine = newLine.Replace(match.Value, expansions[j++][i]); // Replace expression parts
                }
                expanded += String.Concat(newLine, "\n");
            }

            return expanded;
        }

        #endregion
    }
}
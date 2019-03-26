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
        /// <summary>
        /// Regex patterns for parsing.
        /// </summary>
        public static readonly string NamePattern = @"([~*]*(?<Name>[_a-zA-Z]\w{0,19}))";
        public static readonly string VectorPattern = $@"({NamePattern}((\[(?<LeftBound>\d+)\.(?<Step>[1-9]\d*)?\.(?<RightBound>\d+)\])|(\[\])))";
        public static readonly string VariablePattern = $@"({NamePattern}|{VectorPattern})";
        public static readonly string ConstantPattern = @"((?<BitCount>\d{1,2})?\'(((?<Format>[hH])(?<Value>[a-fA-F\d]+))|((?<Format>[dD])(?<Value>\d+))|((?<Format>[bB])(?<Value>[0-1]+))))";
        public static readonly string AnyVariablePattern = $@"({VectorPattern}|{NamePattern}|{ConstantPattern})";
        public static readonly string FormatSpecifierPattern = $@"(%(?<Format>[ubhdUBHD])\{{(?<Vars>{VariablePattern}(\s*{VariablePattern})*)\}})";
        public static readonly string SpacingPattern = @"(^\s+|(?<=\s)\s+)";
        public static readonly string CommentPattern = @"^((?<Spacing>\s*)(?<Color><#?[a-zA-Z0-9]+>)?(?<DoInclude>[+-])?(?<Comment>"".*""\;))$";
        private static readonly string LibraryPattern = @"^(#library\s(?<Name>.+);)$";
        private static readonly string OperatorPattern = @"^(([=+^|-])|(<=)|(~+)|(==))$";
        private static readonly string SeperatorPattern = @"[\s{}(),;]";
        private static readonly string InvalidPattern = @"[^\s_a-zA-Z0-9~%^*()=+[\]{}|;'#<>,.-]";

        /// <summary>
        /// Compiled regexs for parsing.
        /// </summary>
        public static Regex NameRegex = new Regex(String.Concat("^", NamePattern, "$"), RegexOptions.Compiled);
        public static Regex VectorRegex = new Regex(VectorPattern, RegexOptions.Compiled);
        public static Regex ConstantRegex = new Regex(String.Concat("^", ConstantPattern, "$"), RegexOptions.Compiled);
        private static Regex ExpansionRegex = new Regex(String.Concat(VectorPattern, "|", ConstantPattern), RegexOptions.Compiled);
        public static Regex FormatSpecifierRegex = new Regex(FormatSpecifierPattern, RegexOptions.Compiled);
        public static Regex CommentRegex = new Regex(CommentPattern, RegexOptions.Compiled);
        private static Regex LibraryRegex = new Regex(LibraryPattern, RegexOptions.Compiled);
        private static Regex OperatorRegex = new Regex(OperatorPattern, RegexOptions.Compiled);
        private static Regex SeperatorRegex = new Regex(SeperatorPattern, RegexOptions.Compiled);
        private static Regex InvalidRegex = new Regex(InvalidPattern, RegexOptions.Compiled);

        /// <summary>
        /// List of operators.
        /// </summary>
        public static readonly IList<string> OperatorsList = new ReadOnlyCollection<string>(new List<string>{"^", "|", "+", "-", "==", " ", "~"});
        public static readonly IList<string> ExclusiveOperatorsList = new ReadOnlyCollection<string>(new List<string>{"^", "+", "-", "=="});

        private List<string> Libraries;

        /// <summary>
        /// The design being parsed.
        /// </summary>
        private Design Design;

        /// <summary>
        /// Line number of the design being parsed. (Used for errors)
        /// </summary>
        private int PreLineNumber;

        /// <summary>
        /// Line number of the parsed output. (Used for statements)
        /// </summary>
        private int PostLineNumber;

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

        public Parser()
        {
            Libraries = new List<string>();
        }

        /// <summary>
        /// The entry method of the parsing engine. This method acts as "main" for the parsing engine.
        /// </summary>
        /// <param name="sd">The subdesign containing the text to parse</param>
        /// <param name="variableName">The clicked variable if it exists, else the empty string</param>
        /// <returns>Returns a list of parsed elements containing the text and value of each unit in the given expression</returns>
		public List<IObjectCodeElement> Parse(Design sd, string variableName, bool tick)
		{
            Design = sd;
            Tick = tick;
            Globals.Logger.Start();

            //initial run
            if(string.IsNullOrEmpty(variableName) && tick.Equals(false))
            {
                Init = true;
                Design.Database = new Database();
                List<Statement> stmtList = ParseStatements();
                if(stmtList == null)
                {
                    Globals.Logger.Display();
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
                Init = false;
                Design.Database.VariableClicked(variableName);
                List<Statement> stmtList = ParseStatements();
                if (stmtList == null)
                {
                    Globals.Logger.Display();
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
                Init = false;
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
        /// Returns a list of statements from parsed source code.
        /// </summary>
        /// <returns>List of statements</returns>
        private List<Statement> ParseStatements()
        {
            bool valid = true;
            List<Statement> statements = new List<Statement>();

            byte[] bytes = Encoding.UTF8.GetBytes(Design.Text);
            MemoryStream stream = new MemoryStream(bytes);
            using (StreamReader reader = new StreamReader(stream))
            {
                PreLineNumber = 0;
                PostLineNumber = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    PreLineNumber++;
                    line = line.TrimEnd(); // Trim end of line
                    StatementType? type = GetStatementType(line);
                    line = line.Replace("**", ""); // Remove double negatives
                    line = line.Replace("~~", ""); // Remove double negatives

                    if (type == null)
                    {
                        valid = false;
                    }
                    else if (type == StatementType.Library)
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
                                continue;
                            }
                            else
                            {
                                Globals.Logger.Add($"Line {PreLineNumber}: Library '{path}' doesn't exist or is invalid.");
                                valid = false;
                            }
                        }
                        catch (Exception)
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: Invalid library name '{library}'.");
                            valid = false;
                        }              
                    }
                    else if (type == StatementType.Submodule)
                    {

                    }
                    else if (type == StatementType.Module)
                    {

                    }
                    else if (type == StatementType.Boolean || type == StatementType.Clock)
                    {
                        // Verify expressions
                        string dependent = line.Contains("<")
                            ? line.Substring(0, line.IndexOf('<')).Trim()
                            : line.Substring(0, line.IndexOf('=')).Trim();
                        string expression = line.Substring(line.IndexOf("=") + 1).Trim();
                        expression = expression.TrimEnd(';');
                        expression = String.Concat("(", expression, ")");

                        if (!ValidateExpressionStatement(expression))
                        {
                            valid = false;
                        }

                        if (expression.Contains(dependent) && expression.Contains("=="))
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: Circular Dependency Found.");
                            valid = false;
                        }
                    }
                    else if (type == StatementType.FormatSpecifier)
                    {
                        // Verify formats
                        if (!ValidateFormatSpeciferStatement(line))
                        {
                            valid = false;
                        }
                    }

                    if (valid)
                    {
                        // Create statements
                        if (type == StatementType.Empty)
                        {
                            statements.Add(new EmptyStmt(PostLineNumber++, line));
                        }
                        else if (type == StatementType.Comment)
                        {
                            Match match = CommentRegex.Match(line);

                            if (match.Groups["DoInclude"].Value != "-" && (Properties.Settings.Default.SimulationComments || match.Groups["DoInclude"].Value == "+"))
                            {
                                line = String.Concat(match.Groups["Spacing"].Value, match.Groups["Color"].Value, match.Groups["Comment"].Value); // Remove + or -
                                statements.Add(new CommentStmt(PostLineNumber++, line));
                            }
                        }
                        else if (type == StatementType.Module)
                        {
                            statements.Add(new ModuleDeclarationStmt(PostLineNumber++, line));
                        }
                        else if (type == StatementType.Submodule)
                        {
                            statements.Add(new SubmoduleInstantiationStmt(PostLineNumber++, line));
                        }
                        else
                        {
                            bool needsExpansion = ExpansionRegex.IsMatch(line);
                            if (needsExpansion && line.Contains("="))
                            {
                                // Vertical expansion needed
                                string expansion = ExpandVertically(line);
                                if (expansion == null)
                                {
                                    return null;
                                }

                                line = expansion;
                            }
                            else if (needsExpansion)
                            {
                                // Horizontal expansion needed
                                string expandedLine = line;
                                while (VectorRegex.IsMatch(expandedLine))
                                {
                                    Match match = Regex.Matches(expandedLine, VectorPattern)[0]; // Get match

                                    string expanded;
                                    List<string> expansion = GetExpansion(match);
                                    if (expansion == null)
                                    {
                                        return null;
                                    }

                                    expanded = String.Join(" ", expansion);
                                    expandedLine = expandedLine.Substring(0, match.Index) + expanded + expandedLine.Substring(match.Index + match.Length);
                                }

                                line = expandedLine;
                            }

                            // Add statement for each expansion of line
                            foreach (string source in line.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (Init && !InitVariables(type, source))
                                {
                                    // Unable to initialize variables
                                    valid = false;
                                }

                                if (type == StatementType.Boolean)
                                {
                                    statements.Add(new BooleanAssignmentStmt(PostLineNumber++, source));
                                }
                                else if (type == StatementType.Clock)
                                {
                                    statements.Add(new DffClockStmt(PostLineNumber++, source, Tick, Init));
                                }
                                else if (type == StatementType.FormatSpecifier)
                                {
                                    statements.Add(new FormatSpecifierStmt(PostLineNumber++, source));
                                }
                                else if (type == StatementType.VariableList)
                                {
                                    statements.Add(new VariableListStmt(PostLineNumber++, source));
                                }
                            }
                        }
                    }
                }
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

        /// <summary>
        /// Validates an expression statement
        /// </summary>
        /// <param name="expression">Expression to validate</param>
        /// <returns>Whether the expression is valid or not</returns>
        private bool ValidateExpressionStatement(string expression)
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
                        Globals.Logger.Add($"Line {PreLineNumber}: Empty ().");
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
                        Globals.Logger.Add($"Line {PreLineNumber}: An operator is missing its operands.");
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
                                    Globals.Logger.Add($"Line {PreLineNumber}: '{token}' can only appear with '+' or '-' in its parentheses level.");
                                }
                                else
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '{token}' must be the only operator in its parentheses level.");
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
                            Globals.Logger.Add($"Line {PreLineNumber}: '{exclusiveOperator}' can only appear with '+' or '-' in its parentheses level.");
                            return false;
                        }
                    }
                    else
                    {
                        if (token != exclusiveOperator)
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: '{exclusiveOperator}' must be the only operator in its parentheses level.");
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
        /// Validates a format specifier statement
        /// </summary>
        /// <param name="line">Line to validate</param>
        /// <returns>Whether the line is valid or not</returns>
        private bool ValidateFormatSpeciferStatement(string line)
        {
            if (Regex.IsMatch(line, $@"(?<!%)(?![^{{}}]*\}})({NamePattern})"))
            {
                // If line contains a variable outside {}
                Globals.Logger.Add($"Line {PreLineNumber}: Variables in a format specifier statement must be inside a format specifier.");
                return false;
            }

            // Check for formats inside formats or formats without variables
            MatchCollection formats = FormatSpecifierRegex.Matches(line);
            if (formats.Count != line.Count(c => c == '%'))
            {
                Globals.Logger.Add($"Line {PreLineNumber}: Invalid format specifier.");
                return false;
            }

            return true;
        }

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
                Globals.Logger.Add($"Line {PreLineNumber}: Missing ';'.");
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
                    if (!Regex.IsMatch(currentLexeme, @"^(<#?[a-zA-Z0-9]+>[+-]?)$"))
                    {
                        // If not possible color/color code: do normal comment checks
                        if (!(currentLexeme == "+" || currentLexeme == "-" || currentLexeme == ""))
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: Invalid '\"'.");
                            return null;
                        }

                        if (tokens.Any(token => token != " "))
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: Invalid '\"'.");
                            return null;
                        }
                    }

                    type = StatementType.Comment;
                    lexeme.Append(c);
                }
                else if (SeperatorRegex.IsMatch(newChar))
                {
                    // Ending characters
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
                                Globals.Logger.Add($"Line {PreLineNumber}: '{top}' must be matched first.");
                                return null;
                            }
                        }
                        else
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: Unmatched '{c}'.");
                            return null;
                        }
                    }
                    else if (c == ',')
                    {
                        // Check for misplaced comma
                        if (groupings.Count == 0)
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: ',' can only be used inside the () in a submodule or module statement.");
                            return null;
                        }
                        else
                        {
                            char top = groupings.Peek();

                            if (!((type == StatementType.Submodule || type == StatementType.Module) && top == '('))
                            {
                                Globals.Logger.Add($"Line {PreLineNumber}: ',' can only be used inside the () in a submodule or module statement.");
                                return null;
                            }
                        }
                    }

                    if (currentLexeme.Length > 0)
                    {
                        if (IsToken(currentLexeme))
                        {
                            // Check for invalid tokens with current statement type
                            if (currentLexeme == "=")
                            {
                                if (type != StatementType.Empty)
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '{lexeme}' can only be used after the dependent in a boolean statement.");
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
                                    Globals.Logger.Add($"Line {PreLineNumber}: '{lexeme}' can only be used after the dependent in a clock statement.");
                                    return null;
                                }
                                else
                                {
                                    type = StatementType.Clock;
                                }
                            }
                            else if (currentLexeme.Contains("%"))
                            {
                                if (type != StatementType.Empty && type != StatementType.FormatSpecifier)
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '{lexeme}' can only be used in a format specifier statement.");
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
                                    Globals.Logger.Add($"Line {PreLineNumber}: '~' can only be used in front of a variable, vector, ( or {{ on the right side of a boolean or clock statement.");
                                    return null;
                                }

                                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '~' can only be used in front of a variable, vector or parenthesis on the right side of a boolean or clock statement.");
                                    return null;
                                }
                            }
                            else if (currentLexeme.Contains("*"))
                            {
                                if (type != StatementType.Empty)
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '*' can only be used in a variable list statement.");
                                    return null;
                                }
                            }
                            else if (Regex.IsMatch(currentLexeme, @"^([+|^-])|(==)$"))
                            {
                                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '{lexeme}' operator can only be used in a boolean or clock statement.");
                                    return null;
                                }
                            }
                            else if (currentLexeme.Contains("'"))
                            {
                                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: Constants can only be used on the right side of a boolean or clock statement.");
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }

                        tokens.Add(currentLexeme);
                        lexeme.Clear();
                    }

                    if (c == '{' || c == '}')
                    {
                        if (type != StatementType.FormatSpecifier && type != StatementType.Boolean)
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: '{c}' must be part of a boolean or format specifier statement.");
                            return null;
                        }
                    }
                    else if (c == '(' || c == ')')
                    {
                        if (currentLexeme == Design.FileSourceName.Split('.')[0])
                        {
                            type = StatementType.Module;
                        }

                        if (!(type == StatementType.Submodule || type == StatementType.Boolean || type == StatementType.Module))
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: '{c}' must be part of a module, submodule, boolean or clock statement.");
                            return null;
                        }
                    }
                    else if (c == ';')
                    {
                        if (tokens.Count == 0 || tokens.Contains(";"))
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: ';' can only be used to end a statement.");
                            return null;
                        }

                        if (tokens.Last() == " ")
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: Spaces cannot occur before ';'.");
                            return null;
                        }
                    }

                    tokens.Add(c.ToString());
                }
                else if (InvalidRegex.IsMatch(newChar))
                {
                    // Invalid char
                    Globals.Logger.Add($"Line {PreLineNumber}: Invalid character '{c}'.");
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
                            Globals.Logger.Add($"Line {PreLineNumber}: Constants in concatenation fields must specify bit count.");
                            return null;
                        }
                    }

                    lexeme.Append(c);
                }
            }

            // Check for valid comment
            if (type == StatementType.Comment)
            {
                if (!CommentRegex.IsMatch(lexeme.ToString()))
                {
                    Globals.Logger.Add($"Line {PreLineNumber}: Invalid comment statement.");
                    return null;
                }

                return type;
            }

            if (type == StatementType.Library)
            {
                if (LibraryRegex.IsMatch(line))
                {
                    return type;
                }
                else
                {
                    Globals.Logger.Add($"Line {PreLineNumber}: Invalid Library Statement.");
                    return null;
                }
            }

            // At this point, if type is Empty & a non whitespace token exists => type should be set to VariableList
            if (type == StatementType.Empty)
            {
                type = StatementType.VariableList;
            }

            // Check for unclosed groupings
            if (groupings.Count > 0)
            {
                foreach (char grouping in groupings)
                {
                    Globals.Logger.Add($"Line {PreLineNumber}: '{grouping}' is not matched.");
                }
                return null;
            }

            return type;
        }

        /// <summary>
        /// Returns whether a lexeme is a token.
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <returns>Whether the lexeme is a token</returns>
        private bool IsToken(string lexeme)
        {
            if (IsVariable(lexeme))
            {
                // Lexeme is variable
                return true;
            }
            else if (IsVector(lexeme))
            {
                // Lexeme is vector
                return true;
            }
            else if (OperatorRegex.IsMatch(lexeme))
            {
                // Lexeme is operator
                return true;
            }
            else if (Regex.IsMatch(lexeme, @"^%[bBdDuUhH]$"))
            {
                // Lexeme is format specifier
                return true;
            }
            else if (IsConstant(lexeme))
            {
                // Lexeme is constant
                return true;
            }
            else
            {
                // Not sure what lexeme is
                Globals.Logger.Add($"Line {PreLineNumber}: Invalid '{lexeme}'.");
                return false;
            }
        }

        /// <summary>
        /// Returns whether a lexeme is a variable. (If so, initializes it)
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <returns>Whether the lexeme is a variable</returns>
        private bool IsVariable(string lexeme)
        {
            Match match = NameRegex.Match(lexeme);
            if (match.Success)
            {
                // Check variable name has at least one letter
                if (!lexeme.Any(c => char.IsLetter(c)))
                {
                    Globals.Logger.Add($"Line {PreLineNumber}: Invalid variable name.");
                    return false;
                }

                if (Design.Database.HasVectorNamespace(lexeme))
                {
                    Globals.Logger.Add($"Line {PreLineNumber}: Variable {lexeme} already exists as a vector.");
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
            Match match = Regex.Match(lexeme, $@"^{VectorPattern}$");
            if (match.Success)
            {
                // Check for invalid vector namespace name
                string vectorNamespace = match.Groups["Name"].Value;
                if (Char.IsDigit(vectorNamespace[vectorNamespace.Length - 1]))
                {
                    Globals.Logger.Add($"Line {PreLineNumber}: Vector namespaces cannot end in a number.");
                    return false;
                }

                // Check vector bounds and step
                int leftBound = String.IsNullOrEmpty(match.Groups["LeftBound"].Value) ? -1 : Convert.ToInt32(match.Groups["LeftBound"].Value);
                int step = String.IsNullOrEmpty(match.Groups["Step"].Value) ? -1 : Convert.ToInt32(match.Groups["Step"].Value);
                int rightBound = String.IsNullOrEmpty(match.Groups["RightBound"].Value) ? -1 : Convert.ToInt32(match.Groups["RightBound"].Value);
                if (leftBound > 31 || rightBound > 31 || step > 31)
                {
                    Globals.Logger.Add($"Line {PreLineNumber}: Vector bounds and step must be between 0 and 31.");
                    return false;
                }

                if (Init)
                {
                    // Check if namespace is used by a variable
                    if (Design.Database.TryGetVariable<Variable>(vectorNamespace) != null)
                    {
                        Globals.Logger.Add($"Line {PreLineNumber}: {vectorNamespace} cannot be used. A variable with that name already exists.");
                        return false;
                    }

                    // Check for existing vector namespace
                    string vector = (lexeme.Contains('~') || lexeme.Contains('~'))
                        ? lexeme.Substring(lexeme.IndexOf(lexeme.First(c => Regex.IsMatch(c.ToString(), @"[_a-zA-Z]"))))
                        : lexeme; // Remove all ~ or all *
                    List<string> expandedList = new List<string>();
                    if (leftBound != -1)
                    {
                        if (ExpansionMemo.ContainsKey(vector))
                        {
                            expandedList = ExpansionMemo[vector];
                        }
                        else
                        {
                            expandedList = ExpandHorizontally(Regex.Match(vector, $@"^{VectorPattern}$"));
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
                                Globals.Logger.Add($"Line {PreLineNumber}: Vector namespace {vectorNamespace} is already defined. Only the following components: {String.Join(" ", vectorComponents)} can be referenced.");
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
                            Globals.Logger.Add($"Line {PreLineNumber}: '{vector}' notation cannot be used before the vector is initialized.");
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
                    Globals.Logger.Add($"Line {PreLineNumber}: Constant can have at most 32 bits.");
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
        /// Initializes variables in a line.
        /// </summary>
        /// <param name="type">Type of statement</param>
        /// <param name="line">Line</param>
        /// <returns>Whether the operation was successful</returns>
        private bool InitVariables(StatementType? type, string line)
        {
            MatchCollection matches = Regex.Matches(line, @"\*?(?<Name>[_a-zA-Z]\w{0,19})");
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
                    var = val ? var.Substring(1) : var; // Remove * if present

                    if (Design.Database.TryGetVariable<Variable>(var) == null)
                    {
                        // Create Variable
                        IndependentVariable indVar = new IndependentVariable(var, val);
                        Design.Database.AddVariable<IndependentVariable>(indVar);
                    }
                    else if (type == StatementType.VariableList)
                    {
                        Globals.Logger.Add($"Line {PreLineNumber}: {var} has already been declared.");
                        return false; // You cannot declare a variable twice
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the expansion of a provided match
        /// </summary>
        /// <param name="match">Vector expansion or constant expansion match</param>
        /// <returns>Expansion of the provided match</returns>
        private List<string> GetExpansion(Match match)
        {
            if (match.Value.Contains("[") && String.IsNullOrEmpty(match.Groups["LeftBound"].Value))
            {
                List<string> components = Design.Database.GetVectorComponents(match.Groups["Name"].Value);
                if (match.Value.Contains("~") && !components[0].Contains("~"))
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
                if (ExpansionMemo.ContainsKey(match.Value))
                {
                    return ExpansionMemo[match.Value];
                }
                else
                {
                    return ExpandHorizontally(match);
                }
            }
        }

        /// <summary>
        /// Expands vectors or constants into their variable list or bits.
        /// </summary>
        /// <param name="match">The expansion match</param>
        /// <returns>The components</returns>
        private List<string> ExpandHorizontally(Match match)
        {
            List<string> expansion = new List<string>();

            if (!match.Value.Contains("'"))
            {
                // Expand vector
                // Get vector name, bounds and step
                string name = (match.Value.Contains("*") || match.Value.Contains("~"))
                    ? String.Concat(match.Value[0], match.Groups["Name"].Value)
                    : match.Groups["Name"].Value;
                int leftBound = Convert.ToInt32(match.Groups["LeftBound"].Value);
                int rightBound = Convert.ToInt32(match.Groups["RightBound"].Value);
                if (leftBound < rightBound)
                {
                    // Flips bounds so MSB is the leftBound
                    leftBound = leftBound + rightBound;
                    rightBound = leftBound - rightBound;
                    leftBound = leftBound - rightBound;
                }
                int step = String.IsNullOrEmpty(match.Groups["Step"].Value) ? -1 : -Convert.ToInt32(match.Groups["Step"].Value);

                // Expand vector
                for (int i = leftBound; i >= rightBound; i += step)
                {
                    expansion.Add(String.Concat(name, i));
                }
            }
            else
            {
                // Expand constant
                char[] charBits; // Converted binary bits as chars
                int[] bits; // Converted binary bits
                string outputBinary;

                if (match.Groups["Format"].Value == "h" || match.Groups["Format"].Value == "H")
                {
                    outputBinary = Convert.ToString(Convert.ToInt32(match.Groups["Value"].Value, 16), 2);
                    charBits = outputBinary.ToCharArray();
                }
                else if (match.Groups["Format"].Value == "d" || match.Groups["Format"].Value == "D")
                {
                    outputBinary = Convert.ToString(Convert.ToInt32(match.Groups["Value"].Value, 10), 2);
                    charBits = outputBinary.ToCharArray();
                }
                else
                {
                    charBits = match.Groups["Value"].Value.ToCharArray();
                }

                bits = Array.ConvertAll(charBits, bit => (int)Char.GetNumericValue(bit));

                int specifiedBitCount = String.IsNullOrEmpty(match.Groups["BitCount"].Value)
                    ? -1
                    : Convert.ToInt32(match.Groups["BitCount"].Value);

                if (specifiedBitCount != -1 && specifiedBitCount < bits.Length)
                {
                    // Error
                    Globals.Logger.Add($"Line {PreLineNumber}: {match.Value} doesn't specify enough bits.");
                    return null;
                }
                else if (specifiedBitCount > bits.Length)
                {
                    // Adding padding
                    for (int i = 0; i < specifiedBitCount - bits.Length; i++)
                    {
                        expansion.Add("'b0");
                    }
                }

                foreach (int bit in bits)
                {
                    expansion.Add(String.Concat("'b", bit));
                }
            }

            // Save expansion
            if (!ExpansionMemo.ContainsKey(match.Value))
            {
                ExpansionMemo.Add(match.Value, expansion);
            }

            return expansion;
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
                @"(" + VectorPattern + @"(?![^{}]*\}))"             // Any Vector Not Inside {}
                + @"|"                                              // Or
                + @"(" + ConstantPattern + @"(?![^{}]*\}))"         // Any Constant Not Inside {}
                + @"|"                                              // Or
                + @"(\{"                                            // {
                    + AnyVariablePattern                            // Any Variable Type
                    + @"(\s+" + AnyVariablePattern + @")*"          // Any Other Variables Seperated By Whitespace
                + @"\})"                                            // }
            );

            // Get dependent and expression
            string dependent = !line.Contains("<")
                ? line.Substring(0, line.IndexOf("=")).TrimEnd()
                : line.Substring(0, line.IndexOf("<")).TrimEnd();
            string expression = line.Substring(line.IndexOf("=") + 1).TrimStart();

            // Expand dependent
            List<string> dependentExpansion = new List<string>();
            Match dependentMatch = VectorRegex.Match(dependent);
            if (dependentMatch.Success)
            {
                dependentExpansion.AddRange(GetExpansion(dependentMatch));
            }
            else
            {
                dependentExpansion.Add(dependent);
            }

            // Expand expression
            List<List<string>> expressionExpansions = new List<List<string>>();
            MatchCollection matches = regex.Matches(expression);
            foreach (Match match in matches)
            {
                string[] vars;

                // Get vars
                if (!match.Value.Contains("{"))
                {
                    vars = new string[] { match.Value };
                }
                else
                {
                    // Get concat and split into vars
                    string concat = Regex.Replace(match.Value, @"[{}]", string.Empty);
                    vars = Regex.Split(concat, @"\s+"); // Split variables by whitespace
                }

                // Create current expansion
                List<string> currentExpansion = new List<string>();
                foreach (string var in vars)
                {
                    Match currentMatch = Regex.Match(var, AnyVariablePattern);

                    if (var.Contains("[") || var.Contains("'"))
                    {
                        // Vectors and constants
                        List<string> expansion = GetExpansion(currentMatch);
                        if (expansion == null)
                        {
                            return null;
                        }
                        currentExpansion.AddRange(expansion);
                    }
                    else
                    {
                        // Variables
                        currentExpansion.Add(var);
                    }
                }

                expressionExpansions.Add(currentExpansion);
            }

            // Verify expansions
            foreach (List<string> expressionExpansion in expressionExpansions)
            {
                if (dependentExpansion.Count != expressionExpansion.Count)
                {
                    Globals.Logger.Add($"Line {PreLineNumber}: Vector and/or concatenation element counts must be consistent across the entire expression.");
                    return null;
                }
            }

            // Combine expansions
            List<List<string>> expansions = new List<List<string>>();
            expansions.Add(dependentExpansion);
            expansions.AddRange(expressionExpansions);

            // Expand lines 
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
    }
}
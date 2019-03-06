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
        public static readonly string NamePattern = @"([~*]?(?<Name>[_a-zA-Z]\w{0,19}))";
        public static readonly string VectorPattern = $@"({NamePattern}((\[(?<LeftBound>\d+)\.(?<Step>[1-9]\d*)?\.(?<RightBound>\d+)\])|(\[\])))";
        public static readonly string VariablePattern = $@"({NamePattern}|{VectorPattern})";
        public static readonly string ConstantPattern = @"((?<BitCount>\d{1,2})?\'(((?<Format>[hH])(?<Value>[a-fA-F\d]+))|((?<Format>[dD])(?<Value>\d+))|((?<Format>[bB])(?<Value>[0-1]+))))";
        public static readonly string AnyVariablePattern = $@"({NamePattern}|{VectorPattern}|{ConstantPattern})";
        public static readonly string FormatSpecifierPattern = $@"(%(?<Format>[ubhdUBHD])\{{(?<Vars>{VariablePattern}(\s*{VariablePattern})*)\}})";
        public static readonly string SpacingPattern = @"(^\s+|(?<=\s)\s+)";
        public static readonly string CommentPattern = @"^((?<Spacing>\s*)(?<DoInclude>[+-])?(?<Comment>"".*""\;))$";

        /// <summary>
        /// The design being parsed.
        /// </summary>
        private SubDesign Design;

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
        private Dictionary<string, List<string>> ExpansionMemo;

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
            ExpansionMemo = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// The entry method of the parsing engine. This method acts as "main" for the parsing engine.
        /// </summary>
        /// <param name="sd">The subdesign containing the text to parse</param>
        /// <param name="variableName">The clicked variable if it exists, else the empty string</param>
        /// <returns>Returns a list of parsed elements containing the text and value of each unit in the given expression</returns>
		public List<IObjectCodeElement> Parse(SubDesign sd, string variableName, bool tick)
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

                    if (type == null)
                    {
                        valid = false;
                    }
                    else if (type == StatementType.Boolean || type == StatementType.Clock)
                    {
                        // Verify expressions
                    }
                    else if (type == StatementType.FormatSpecifier)
                    {
                        // Verify formats
                        if (Regex.IsMatch(line, $@"(?<!%)(?![^{{}}]*\}})({NamePattern})"))
                        {
                            // If line contains a variable outside {}
                            Globals.Logger.Add($"Line {PreLineNumber}: Variables in a format specifier statement must be inside a format specifier.");
                            valid = false;
                        }

                        // Check for formats inside formats or formats without variables
                        MatchCollection formats = Regex.Matches(line, FormatSpecifierPattern);
                        if (formats.Count != line.Count(c => c == '%'))
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: Invalid format specifier.");
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
                            Match match = Regex.Match(line, CommentPattern);

                            if (!match.Groups["DoInclude"].Value.Equals("-") && (Properties.Settings.Default.SimulationComments || match.Groups["DoInclude"].Value.Equals("+")))
                            {
                                line = String.Concat(match.Groups["Spacing"].Value, match.Groups["Comment"].Value); // Remove + or -
                                statements.Add(new CommentStmt(PostLineNumber++, line));
                            }
                        }
                        else if (type == StatementType.Submodule)
                        {
                            statements.Add(new SubmoduleInstantiationStmt(PostLineNumber++, line));
                        }
                        else
                        {
                            bool needsExpansion = Regex.IsMatch(line, VectorPattern);
                            if (needsExpansion && line.Contains("="))
                            {
                                // Vertical expansion needed
                                string expansion = ExpandVertically(line);
                                if (expansion == null)
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: Vector and/or concatenation element counts must be consistent across the entire expression.");
                                    return null;
                                }

                                line = expansion;
                            }
                            else if (needsExpansion)
                            {
                                // Horizontal expansion needed
                                string expandedLine = line;
                                while (Regex.IsMatch(expandedLine, VectorPattern))
                                {
                                    Match match = Regex.Matches(expandedLine, VectorPattern)[0]; // Get match

                                    string expanded;
                                    if (String.IsNullOrEmpty(match.Groups["LeftBound"].Value))
                                    {
                                        // Indicates vectorNameSpace[] syntax
                                        expanded = String.Join(" ", Design.Database.GetVectorComponents(match.Groups["Name"].Value));
                                    }
                                    else if (ExpansionMemo.ContainsKey(match.Value))
                                    {
                                        expanded = String.Join(" ", ExpansionMemo[match.Value]);
                                    }
                                    else
                                    {
                                        expanded = String.Join(" ", ExpandHorizontally(match));
                                    }

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
        /// Returns the type of statement for a given line.
        /// </summary>
        /// <param name="line">Line to interpret</param>
        /// <returns>Type of statement</returns>
        private StatementType? GetStatementType(string line)
        {
            StatementType? type = StatementType.Empty;
            List<string> tokens = new List<string>();
            Stack<char> groupings = new Stack<char>();
            string lexeme = "";

            foreach (char c in line)
            {
                string newChar = c.ToString();

                if (type == StatementType.Comment)
                {
                    lexeme = String.Concat(lexeme, newChar);
                }
                else if (newChar.Equals("\""))
                {
                    if (!(lexeme.Equals("+") || lexeme.Equals("-") || lexeme.Equals("")))
                    {
                        Globals.Logger.Add($"Line {PreLineNumber}: Invalid '\"'.");
                        return null;
                    }

                    if (tokens.Any(token => !token.Equals(" ")))
                    {
                        Globals.Logger.Add($"Line {PreLineNumber}: Invalid '\"'.");
                        return null;
                    }

                    type = StatementType.Comment;
                    lexeme = String.Concat(lexeme, newChar);
                }
                else if (Regex.IsMatch(newChar, @"[^\s_a-zA-Z0-9~@%^*()=+[\]{}|;'<,.-]"))
                {
                    // Invalid char
                    Globals.Logger.Add($"Line {PreLineNumber}: Invalid character '{newChar}'.");
                    return null;
                }
                else if (Regex.IsMatch(newChar, @"[\s{}(),;]"))
                {
                    // Ending characters
                    if (newChar.Equals("{") || newChar.Equals("("))
                    {
                        // Add grouping char to stack
                        groupings.Push(newChar[0]);
                    }
                    else if (newChar.Equals("}") || newChar.Equals(")"))
                    {
                        // Check for correct closing
                        if (groupings.Count > 0)
                        {
                            char top = groupings.Peek();
                            if ((newChar.Equals(")") && top == '(') || (newChar.Equals("}") && top == '{'))
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
                            Globals.Logger.Add($"Line {PreLineNumber}: Unmatched '{newChar}'.");
                            return null;
                        }
                    }
                    else if (newChar.Equals(","))
                    {
                        // Check for misplaced comma
                        if (groupings.Count == 0)
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: ',' must be in {{}} or () in a submodule statement.");
                            return null;
                        }
                        else
                        {
                            char top = groupings.Peek();

                            if (type == StatementType.Submodule && top != '(')
                            {
                                Globals.Logger.Add($"Line {PreLineNumber}: ',' must be in () in a submodule statement.");
                                return null;
                            }
                            else if (type != StatementType.Submodule && top != '{')
                            {
                                Globals.Logger.Add($"Line {PreLineNumber}: ',' must be in {{}}.");
                                return null;
                            }
                        }
                    }

                    if (lexeme.Length > 0)
                    {
                        if (IsToken(lexeme))
                        {
                            // Check for invalid tokens with current statement type
                            if (lexeme.Equals("="))
                            {
                                if (type != StatementType.Empty)
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '{lexeme}' can only be used once in a boolean statement.");
                                    return null;
                                }
                                else
                                {
                                    type = StatementType.Boolean;
                                }
                            }
                            else if (lexeme.Equals("<="))
                            {
                                if (type != StatementType.Empty)
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '{lexeme}' can only be used once in a clock statement.");
                                    return null;
                                }
                                else
                                {
                                    type = StatementType.Clock;
                                }
                            }
                            else if (lexeme.Contains("%"))
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
                            else if (lexeme.Contains("@"))
                            {
                                if (type != StatementType.Empty)
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '{lexeme}' can only be used in a submodule statement.");
                                    return null;
                                }
                                else
                                {
                                    type = StatementType.Submodule;
                                }
                            }
                            else if (lexeme.Contains("~"))
                            {
                                if (lexeme.Equals("~") && !newChar.Equals("("))
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '~' can only be used in front of a variable, vector or parenthesis on the right side of a boolean statement.");
                                    return null;
                                }

                                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '~' can only be used in front of a variable, vector or parenthesis on the right side of a boolean statement.");
                                    return null;
                                }
                            }
                            else if (lexeme.Contains("*"))
                            {
                                if (type != StatementType.Empty)
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '*' can only be used in a variable list statement.");
                                    return null;
                                }
                            }
                            else if (Regex.IsMatch(lexeme, @"^([+|^-])|(==)$"))
                            {
                                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: '{lexeme}' can only be used in a boolean or clock statement.");
                                    return null;
                                }
                            }
                            else if (lexeme.Contains("'"))
                            {
                                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                                {
                                    Globals.Logger.Add($"Line {PreLineNumber}: Constants can only be used on the right side of a boolean or statement.");
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }

                        tokens.Add(lexeme);
                        lexeme = "";
                    }

                    if (newChar.Equals("{") || newChar.Equals("}"))
                    {
                        if (type != StatementType.FormatSpecifier && type != StatementType.Boolean)
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: '{newChar}' must be part of a boolean or format specifier statement.");
                            return null;
                        }
                    }
                    else if (newChar.Equals("(") || newChar.Equals(")"))
                    {
                        if (type != StatementType.Submodule && type != StatementType.Boolean)
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: '{newChar}' must be part of a submodule or boolean statement.");
                            return null;
                        }
                    }
                    else if (newChar.Equals(";"))
                    {
                        if (tokens.Count == 0)
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: ';' cannot start a statement.");
                            return null;
                        }

                        if (tokens.Contains(";"))
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: Invalid ';'.");
                            return null;
                        }

                        if (tokens.Last().Equals(" "))
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: Spaces cannot be before ';'.");
                            return null;
                        }
                    }

                    tokens.Add(newChar);
                }
                else
                {
                    // Appending characters

                    // Check for constant inside {}
                    if (newChar.Equals("'"))
                    {
                        // Check for constant bit count inside {}
                        if (groupings.Count > 0 && groupings.Peek() == '{' && (String.IsNullOrEmpty(lexeme) || !lexeme.All(ch => Char.IsDigit(ch))))
                        {
                            Globals.Logger.Add($"Line {PreLineNumber}: Constants in concat fields must specify bit count.");
                            return null;
                        }
                    }

                    lexeme = String.Concat(lexeme, newChar);
                }
            }

            // Check for valid comment
            if (type == StatementType.Comment)
            {
                if (!Regex.IsMatch(lexeme, CommentPattern))
                {
                    Globals.Logger.Add($"Line {PreLineNumber}: Invalid comment statement.");
                    return null;
                }

                return type;
            }

            // Check for unparsed lexeme
            if (lexeme.Length > 0)
            {
                Globals.Logger.Add($"Line {PreLineNumber}: Invalid '{lexeme}'.");
                return null;
            }

            // Check if all tokens are only whitespace
            if (!tokens.Any(token => !token.Equals(" ")))
            {
                // If only whitespace tokens return
                return type;
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

            // Check for ending ;
            if (!tokens.Contains(";"))
            {
                Globals.Logger.Add($"Line {PreLineNumber}: Line must end with a ';'.");
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
            if (Regex.IsMatch(lexeme, @"^[=+^|-]$"))
            {
                // Lexeme is operator
                return true;
            }
            else if (lexeme.Equals("=="))
            {
                // Lexeme is == operator
                return true;
            }
            else if (lexeme.Equals("<="))
            {
                // Lexeme is <= operator
                return true;
            }
            else if (lexeme.Equals("~"))
            {
                // Lexeme is ~ operator
                return true;
            }
            else if (Regex.IsMatch(lexeme, @"^%[bBdDuUhH]$"))
            {
                // Lexeme is format specifier
                return true;
            }
            else if (IsVector(lexeme))
            {
                // Lexeme is vector
                return true;
            }
            else if (IsVariable(lexeme))
            {
                // Lexeme is variable
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
            Match match = Regex.Match(lexeme, $@"^{NamePattern}$");
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

                // (If necessary) reformat vector so leftBound is most significant bit
                /*
                if (leftBound < rightBound)
                {
                    lexeme = String.Concat(lexeme.Substring(0, lexeme.IndexOf("[") + 1), match.Groups["RightBound"].Value, ".", match.Groups["Step"].Value, ".", match.Groups["LeftBound"].Value, "]");
                }
                */

                if (Init)
                {
                    // Check if namespace is used by a variable
                    if (Design.Database.TryGetVariable<Variable>(vectorNamespace) != null)
                    {
                        Globals.Logger.Add($"Line {PreLineNumber}: {vectorNamespace} cannot be used. A variable with that name already exists.");
                        return false;
                    }

                    // Check for existing vector namespace
                    string vector = (lexeme[0] == '~' || lexeme[0] == '*') ? lexeme.Substring(1) : lexeme; // Remove ~ or *
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
            Match match = Regex.Match(lexeme, $@"^{ConstantPattern}$");
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
        /// Expands vector to its variable list
        /// </summary>
        /// <param name="match">The Vector Match</param>
        /// <returns>The expanded string</returns>
        private List<string> ExpandHorizontally(Match match)
        {
            List<string> expansion = new List<string>();

            // Get vector name, bounds and step
            string name = (match.Value.Contains("*") || match.Value.Contains("~")) ? String.Concat(match.Value[0], match.Groups["Name"].Value) : match.Groups["Name"].Value;
            int leftBound = Convert.ToInt32(match.Groups["LeftBound"].Value);
            int rightBound = Convert.ToInt32(match.Groups["RightBound"].Value);
            int step = (leftBound < rightBound)
                    ? (String.IsNullOrEmpty(match.Groups["Step"].Value) ? 1 : Convert.ToInt32(match.Groups["Step"].Value))
                    : (String.IsNullOrEmpty(match.Groups["Step"].Value) ? -1 : -Convert.ToInt32(match.Groups["Step"].Value));

            // Expand vector
            int i = leftBound;
            while ((step > 0 && i <= rightBound) || (step < 0 && i >= rightBound))
            {
                expansion.Add(String.Concat(name, i));
                i += step;
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
                @"("                                            // Begin Group
                    + VectorPattern                             // Any Vector Type
                    + @"(?![^{}]*\})"                           // Not Inside {}
                + @")"                                          // End Group
                + @"|"                                          // Or
                + @"("                                          // Begin Group
                    + @"\{"                                     // {
                    + AnyVariablePattern                        // Any Variable Type
                    + @"("                                      // Begin Optional Group
                        + @"\,\s*"                              // Comma & Any Number of Whitespace
                        + AnyVariablePattern                    // Any Variable Type
                    + @")*"                                     // End Optional Group
                    + @"\}"                                     // }
                + @")"                                          // End Group
            );
            MatchCollection matches = regex.Matches(line);

            // Expand all variables
            List<List<string>> variables = new List<List<string>>();
            foreach (Match match in matches)
            {
                if (!match.Value.Contains("{"))
                {
                    if (String.IsNullOrEmpty(match.Groups["LeftBound"].Value))
                    {
                        // Indicates vectorNameSpace[] syntax
                        List<string> components = Design.Database.GetVectorComponents(match.Groups["Name"].Value);
                        if (match.Value.Contains("~") && !components[0].Contains("~"))
                        {
                            for (int i = 0; i < components.Count; i++)
                            {
                                components[i] = String.Concat("~", components[i]);
                            }
                        }
                        variables.Add(components);
                    }
                    else if (ExpansionMemo.ContainsKey(match.Value))
                    {
                        variables.Add(ExpansionMemo[match.Value]);
                    }
                    else
                    {
                        variables.Add(ExpandHorizontally(match));
                    }
                }
                else
                {
                    // Get concat and split into vars
                    string concat = Regex.Replace(match.Value, @"[{\s*}]", string.Empty);
                    string[] vars = concat.Split(','); // Split variables by commas

                    List<string> concatVars = new List<string>();
                    foreach (string var in vars)
                    {
                        Match vector = Regex.Match(var, VectorPattern);
                        if (vector.Success) // Come back to here
                        {
                            List<string> components;
                            if (String.IsNullOrEmpty(vector.Groups["LeftBound"].Value))
                            {
                                // Indicates vectorNameSpace[] syntax
                                components = Design.Database.GetVectorComponents(vector.Groups["Name"].Value);
                            }
                            else if (ExpansionMemo.ContainsKey(var))
                            {
                                components = ExpansionMemo[var];
                            }
                            else
                            {
                                components = ExpandHorizontally(vector);
                            }

                            foreach (string component in components)
                            {
                                concatVars.Add(component);
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
    }
}
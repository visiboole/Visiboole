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
    /// The main class of the parsing engine. This class is the brains of the parsing engine and 
    /// communicates with the calling classes.
    /// </summary>
	public class Parser : Lexer
	{
        #region Parsing Patterns & Regular Expressions

        /// <summary>
        /// Pattern for identifying scalars. (Optional ~)
        /// </summary>
        public static readonly string ScalarPattern1 = $"(~?{ScalarNotationPattern})";

        /// <summary>
        /// Pattern for identifying scalars. (Optional *)
        /// </summary>
        public static readonly string ScalarPattern2 = $@"(\*?{ScalarNotationPattern})";

        /// <summary>
        /// Pattern for identifying vectors. (Optional ~)
        /// </summary>
        public static readonly string VectorPattern1 = $"(~?{VectorNotationPattern})";

        /// <summary>
        /// Pattern for identifying vectors. (Optional *)
        /// </summary>
        public static readonly string VectorPattern2 = $@"(\*?{VectorNotationPattern})";

        /// <summary>
        /// Pattern for identifying scalars and vectors. (No ~ or *)
        /// </summary>
        public static readonly string VariablePattern1 = $@"({VectorNotationPattern}|{ScalarNotationPattern})";

        /// <summary>
        /// Pattern for identifying scalars and vectors. (Optional *)
        /// </summary>
        public static readonly string VariablePattern2 = $@"({VectorPattern2}|{ScalarPattern2})";

        /// <summary>
        /// Pattern for identifying scalars, vectors and constants. (No ~ or *)
        /// </summary>
        public static readonly string AnyTypePattern = $@"({VectorNotationPattern}|{ConstantNotationPattern}|{ScalarNotationPattern})";

        /// <summary>
        /// Pattern for identifying concatenations of any type.
        /// </summary>
        public static readonly string ConcatenationPattern = $@"(\{{(?<Vars>{AnyTypePattern}(\s+{AnyTypePattern})*)\}})";

        /// <summary>
        /// Pattern for identifying concatenations of any type or any type.
        /// </summary>
        public static readonly string ConcatOrAnyTypePattern = $@"({ConcatenationPattern}|{AnyTypePattern})";

        /// <summary>
        /// Pattern for identifying entire format specifiers.
        /// </summary>
        public static readonly string FormatSpecifierPattern = $@"({FormatSpecifierNotationPattern}{ConcatenationPattern})";

        /// <summary>
        /// Pattern for identifying extra spacing.
        /// </summary>
        public static readonly string SpacingPattern = @"(^\s+|(?<=\s)\s+)";

        /// <summary>
        /// Pattern for identifying comment statements.
        /// </summary>
        private static readonly string CommentPattern = @"^((?<Spacing>\s*)(?<DoInclude>[+-])?(?<Comment>"".*""\;))$";

        /// <summary>
        /// Pattern for identifying library statements.
        /// </summary>
        private static readonly string LibraryPattern = @"^(#library\s(?<Name>.+);)$";

        /// <summary>
        /// Pattern for identifying components (inputs or outputs) in a module notation.
        /// </summary>
        private static readonly string ModuleComponentNotationPattern = $@"({ConcatOrAnyTypePattern}(,\s+{ConcatOrAnyTypePattern})*)";

        /// <summary>
        /// Pattern for identifying modules.
        /// </summary>
        private static readonly string ModuleNotationPattern = $@"(?<Components>(?<Inputs>{ModuleComponentNotationPattern})\s+:\s+(?<Outputs>{ModuleComponentNotationPattern}))";

        /// <summary>
        /// Pattern for identifying whitespace.
        /// </summary>
        public static Regex WhitespaceRegex { get; } = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalar notation.
        /// </summary>
        public static Regex ScalarRegex1 { get; } = new Regex(ScalarNotationPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalars. (Optional *)
        /// </summary>
        public static Regex ScalarRegex2 { get; } = new Regex(ScalarPattern2, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vectors not in concats. (No ~ or *)
        /// </summary>
        public static Regex VectorRegex1 { get; } = new Regex($@"({VectorNotationPattern}(?![^{{}}]*\}}))", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vectors. (Optional *)
        /// </summary>
        public static Regex VectorRegex2 { get; } = new Regex(VectorPattern2, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalars, vectors and constants. (No ~ or *)
        /// </summary>
        private static Regex AnyTypeRegex = new Regex(AnyTypePattern);

        /// <summary>
        /// Regex for identifying whether expansion is necessary. (Concat, Vectors or Constants)
        /// </summary>
        private static Regex ExpansionRegex1 = new Regex($@"((?<!{FormatSpecifierNotationPattern}){ConcatenationPattern})|{VectorPattern2}|{ConstantNotationPattern}", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying whether expansion is necessary. (Concat, Vectors)
        /// </summary>
        private static Regex ExpansionRegex2 = new Regex($@"((?<!{FormatSpecifierNotationPattern}){ConcatenationPattern})|{VectorPattern2}", RegexOptions.Compiled);

        /// <summary>
        /// Regex used in expansion to preserve scalars. 
        /// </summary>
        private static Regex ExpansionRegex3 = new Regex($@"(({AnyTypePattern}(?![^{{}}]*\}}))|{ConcatenationPattern})", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying entire format specifiers.
        /// </summary>
        public static Regex FormatSpecifierRegex { get; } = new Regex(FormatSpecifierPattern);

        /// <summary>
        /// Regex for identifying comment statements.
        /// </summary>
        public static Regex CommentRegex { get; } = new Regex(CommentPattern);

        /// <summary>
        /// Regex for identifying library statements.
        /// </summary>
        private static Regex LibraryRegex = new Regex(LibraryPattern);

        /// <summary>
        /// Regex for identifying module declarations.
        /// </summary>
        private Regex ModuleRegex;

        #endregion

        /// <summary>
        /// List of operators.
        /// </summary>
        public static readonly IList<string> OperatorsList = new ReadOnlyCollection<string>(new List<string>{"^", "|", "+", "-", "==", " ", "~"});
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
            ModuleRegex = new Regex($@"^\s*{Design.FileName}\({ModuleNotationPattern}\);$");
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
        /// Updates expression statements in the statement list.
        /// </summary>
        private void UpdateExpressionStatements()
        {
            // Replace expression statements with the correctly updated versions
            for (int i = 0; i < Statements.Count; i++)
            {
                Statement statement = Statements[i];

                if (statement.GetType() == typeof(BooleanAssignmentStmt) || statement.GetType() == typeof(DffClockStmt))
                {
                    ExpressionStatement expressionStatement = (ExpressionStatement)statement;
                    Statements[i] = Design.Database.GetExpression(expressionStatement.LineNumber);
                }
            }
        }

        /// <summary>
        /// Parsers the current design text into output.
        /// </summary>
        public List<IObjectCodeElement> Parse()
        {
            // Init parser
            Globals.Logger.Start();

            // Get statements for parsing
            Design.Database = new Database();
            Statements = ParseStatements();
            if (Statements == null)
            {
                Globals.Logger.Display();
                return null;
            }
            UpdateExpressionStatements();

            // Get output
            return GetParsedOutput();
        }

        /// <summary>
        /// Parses the current design text and clicks the provided variable.
        /// </summary>
        /// <param name="variableName">Variable clicked</param>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseClick(string variableName)
        {
            // Init parser
            Globals.Logger.Start();

            // Flip value of variable clicked and reevlaute expressions
            Design.Database.FlipValue(variableName);
            Design.Database.ReevaluateExpressions();
            UpdateExpressionStatements();

            // Get output
            return GetParsedOutput();
        }

        /// <summary>
        /// Parsers the provided design text and ticks.
        /// </summary>
        /// <returns>Parsed output</returns>
        public List<IObjectCodeElement> ParseTick()
        {
            // Init parser
            Globals.Logger.Start();

            // Tick clock statements and reevaluate expressions
            foreach (Statement stmt in Statements)
            {
                if (stmt.GetType() == typeof(DffClockStmt))
                {
                    ((DffClockStmt)stmt).Tick();
                }
            }
            Design.Database.ReevaluateExpressions();
            UpdateExpressionStatements();

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
            // Init parser
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

            Statements = ParseStatements();
            if (Statements == null)
            {
                Globals.Logger.Display();
                return null;
            }
            UpdateExpressionStatements();

            // Get output
            return GetParsedOutput();
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
                Design.ModuleDeclaration = null;
                LineNumber = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    LineNumber++;
                    StatementType? type = null;

                    line = line.TrimEnd(); // Trim end of line
                    if (String.IsNullOrWhiteSpace(line))
                    {
                        type = StatementType.Empty;
                    }
                    else
                    {
                        if (line[line.Length - 1] != ';')
                        {
                            Globals.Logger.Add($"Line {LineNumber}: Missing ';'.");
                            valid = false;
                        }
                    }

                    if (valid && type == null)
                    {
                        type = GetStatementType(line);
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
                        if (!String.IsNullOrEmpty(Design.ModuleDeclaration))
                        {
                            Globals.Logger.Add($"Line {LineNumber}: A module declaration already exists.");
                            valid = false;
                        }
                        else
                        {
                            if (!ModuleRegex.IsMatch(line))
                            {
                                Globals.Logger.Add($"Line {LineNumber}: Invalid module declaration notation.");
                                valid = false;
                            }
                            else
                            {
                                Design.ModuleDeclaration = line;
                            }
                        }

                        continue;
                    }
                    else
                    {
                        // Verify line from its statement type
                        if (!VerifyStatement(line, type))
                        {
                            valid = false;
                        }
                    }

                    if (valid)
                    {
                        if (type != StatementType.Comment)
                        {
                            line = line.Replace("**", ""); // Remove double negatives
                            line = line.Replace("~~", ""); // Remove double negatives
                        }

                        // Create statements
                        if (type == StatementType.Empty)
                        {
                            statements.Add(new EmptyStmt(line));
                        }
                        else if (type == StatementType.Comment)
                        {
                            Match match = CommentRegex.Match(line);

                            if (match.Groups["DoInclude"].Value != "-" && (Properties.Settings.Default.SimulationComments || match.Groups["DoInclude"].Value == "+"))
                            {
                                line = String.Concat(match.Groups["Spacing"].Value, match.Groups["Comment"].Value); // Remove + or -
                                statements.Add(new CommentStmt(line));
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

                            //statements.Add(new SubmoduleInstantiationStmt(line, Instantiations[instantiationName]));
                        }
                        else
                        {
                            bool needsExpansion = ExpansionRegex1.IsMatch(line);
                            if (needsExpansion && line.Contains("=") && !line.Contains("+") && !line.Contains("-"))
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
                                line = ExpandHorizontally(line);
                            }

                            // Add statement for each expansion of line
                            string[] lines = line.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string source in lines)
                            {
                                if (!InitVariables(type, source))
                                {
                                    // Unable to initialize variables
                                    valid = false;
                                }

                                if (type == StatementType.Boolean)
                                {
                                    statements.Add(new BooleanAssignmentStmt(source, statements.Count));
                                }
                                else if (type == StatementType.Clock)
                                {
                                    statements.Add(new DffClockStmt(source, statements.Count));
                                }
                                else if (type == StatementType.FormatSpecifier)
                                {
                                    statements.Add(new FormatSpecifierStmt(source));
                                }
                                else if (type == StatementType.VariableList)
                                {
                                    statements.Add(new VariableListStmt(source));
                                }
                            }
                        }
                    }
                }
            }

            // If module declaration verify
            if (!String.IsNullOrEmpty(Design.ModuleDeclaration) && !VerifyModuleDeclarationStatement())
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

                    // Check operator and mathematical expression status for error
                    if (mathematicalExpression && !(token == "+" || token == "-"))
                    {
                        Globals.Logger.Add($"Line {LineNumber}: '+' and '-' can't appear with boolean operators.");
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
                                    Globals.Logger.Add($"Line {LineNumber}: '{token}' must be the only operator in its parentheses level.");
                                    return false;
                                }

                                expressionExclusiveOperators[expressionExclusiveOperators.Count - 1] = token;
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
            if (Regex.IsMatch(line, $@"(?<!%)(?![^{{}}]*\}})({VariablePattern2})"))
            {
                // If line contains a variable outside {}
                Globals.Logger.Add($"Line {LineNumber}: Scalars and vectors in a format specifier statement must be inside a format specifier.");
                return false;
            }

            // Check for formats inside formats or formats without variables
            MatchCollection formats = FormatSpecifierRegex.Matches(line);
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
        /// Verifies a module declaration statement
        /// </summary>
        /// <param name="declaration">Declaration to verifiy</param>
        /// <returns>Whether the declaration is valid or not</returns>
        private bool VerifyModuleDeclarationStatement()
        {
            // Get input and output variables
            Match module = ModuleRegex.Match(Design.ModuleDeclaration);
            string[] inputVars = Regex.Split(module.Groups["Inputs"].Value, @",\s+");

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
                    vars = GetExpansion(ExpansionRegex2.Match(input));
                }

                foreach (string var in vars)
                {
                    if (Design.Database.TryGetVariable<IndependentVariable>(var) == null)
                    {
                        Globals.Logger.AddTop($"'{var}' must be an independent variable to be used as an input in a module declaration statement.");
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
                    if (match.Index < line.IndexOf("="))
                    {
                        if (Design.Database.TryGetVariable<Variable>(var) == null)
                        {
                            Design.Database.AddVariable<DependentVariable>(new DependentVariable(var, false));
                        }
                        else
                        {
                            if (Design.Database.TryGetVariable<IndependentVariable>(var) as IndependentVariable != null)
                            {
                                Design.Database.MakeDependent(var);
                            }
                        }

                        if (line.Contains("<"))
                        {
                            // Create delay variable
                            var += ".d";

                            if (Design.Database.TryGetVariable<Variable>(var) == null)
                            {
                                Design.Database.AddVariable<DependentVariable>(new DependentVariable(var, false));
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
        /// Exapnds a single token into its components.
        /// </summary>
        /// <param name="token">Token to expand</param>
        /// <returns>List of expansion components</returns>
        private List<string> ExpandToken(Match token)
        {
            if (token.Value.Contains("[") && String.IsNullOrEmpty(token.Groups["LeftBound"].Value))
            {
                List<string> components = Design.Database.GetComponents(token.Groups["Name"].Value);
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
        /// Expands token into components.
        /// </summary>
        /// <param name="token">Token to expand</param>
        /// <returns>List of expansion components</returns>
        private List<string> GetExpansion(Match token)
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
                vars = new string[] {token.Value};
            }

            // Expand each variable
            foreach (string var in vars)
            {
                Match match = AnyTypeRegex.Match(var);

                if (match.Value.Contains("[") || match.Value.Contains("'"))
                {
                    expansion.AddRange(ExpandToken(match));
                }
                else
                {
                    expansion.Add(match.Value);
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

            if (line.Contains("+") || line.Contains("-"))
            {
                // Expand to concats
                while (VectorRegex1.IsMatch(expandedLine))
                {
                    Match match = VectorRegex1.Match(expandedLine);

                    // Get expansion
                    List<string> expansion = GetExpansion(match);

                    // Check and combine expansion
                    if (expansion == null)
                    {
                        return null;
                    }
                    string expanded = String.Join(" ", expansion);
                    expanded = String.Concat("{", expanded, "}");

                    expandedLine = expandedLine.Substring(0, match.Index) + expanded + expandedLine.Substring(match.Index + match.Length);
                }
            }
            else
            {
                // Expand all expansions
                while (ExpansionRegex2.IsMatch(expandedLine))
                {
                    Match match = ExpansionRegex2.Match(expandedLine);

                    // Get expansion
                    List<string> expansion;
                    if (!match.Value.Contains("{"))
                    {
                        expansion = ExpandToken(match);
                    }
                    else
                    {
                        expansion = GetExpansion(match);
                    }

                    // Check and combine expansion
                    if (expansion == null)
                    {
                        return null;
                    }
                    string expanded = String.Join(" ", expansion);

                    expandedLine = expandedLine.Substring(0, match.Index) + expanded + expandedLine.Substring(match.Index + match.Length);
                }
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
            Match dependentMatch = ExpansionRegex3.Match(dependent);
            if (dependentMatch.Value.Contains("{"))
            {
                dependentExpansion.AddRange(GetExpansion(dependentMatch));
            }
            else if (dependentMatch.Value.Contains("["))
            {
                dependentExpansion.AddRange(ExpandToken(dependentMatch));
            }
            else
            {
                dependentExpansion.Add(dependent);
            }

            // Expand expression
            List<List<string>> expressionExpansions = new List<List<string>>();
            MatchCollection matches = ExpansionRegex1.Matches(expression);
            foreach (Match match in matches)
            {
                if (!match.Value.Contains("{"))
                {
                    expressionExpansions.Add(ExpandToken(match));
                }
                else
                {
                    expressionExpansions.Add(GetExpansion(match));
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
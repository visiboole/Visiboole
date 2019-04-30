﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.Views;

namespace VisiBoole.ParsingEngine
{
    public class Lexer
    {
        #region Lexer Types

        /// <summary>
        /// Statement type proposed by the lexer.
        /// </summary>
        protected enum StatementType
        {
            Boolean,
            Clock,
            Comment,
            Empty,
            FormatSpecifier,
            Library,
            Module,
            Submodule,
            VariableList
        }

        #endregion

        #region Lexer Patterns and Regular Expressions

        /// <summary>
        /// Pattern for identifying names.
        /// </summary>
        private static readonly string NamePattern = @"(?<Name>[_a-zA-Z]\w*(?<!\d))";

        /// <summary>
        /// Pattern for identifying scalars. (No ~ or *)
        /// </summary>
        public static readonly string ScalarPattern = $@"({NamePattern}(?<Bit>\d+)?)";

        /// <summary>
        /// Pattern for identfying indexes.
        /// </summary>
        private static readonly string IndexPattern = @"(\[(?<LeftBound>\d+)\.(?<Step>\d+)?\.(?<RightBound>\d+)\]|\[\])";

        /// <summary>
        /// Pattern for identifying vectors. (No ~ or *)
        /// </summary>
        protected static readonly string VectorPattern = $@"({NamePattern}{IndexPattern})";

        /// <summary>
        /// Pattern for identifying binary constants.
        /// </summary>
        private static readonly string BinaryConstantPattern = @"((?<BitCount>\d+)?'(?<Format>[bB])(?<Value>[0-1]+))";

        /// <summary>
        /// Pattern for identifying hex constants.
        /// </summary>
        private static readonly string HexConstantPattern = @"((?<BitCount>\d+)?'(?<Format>[hH])(?<Value>[a-fA-F0-9]+))";

        /// <summary>
        /// Pattern for identifying decimal constants.
        /// </summary>
        private static readonly string DecimalConstantPattern = @"(((?<BitCount>\d+)?'(?<Format>[dD])?)?(?<Value>[0-9]+))";

        /// <summary>
        /// Pattern for identifying constants. (No ~)
        /// </summary>
        public static readonly string ConstantPattern = $@"({BinaryConstantPattern}|{HexConstantPattern}|{DecimalConstantPattern})";

        /// <summary>
        /// Pattern for identifying scalars, vectors and constants. (No ~ or *)
        /// </summary>
        protected static readonly string VariablePattern = $@"({VectorPattern}|{ConstantPattern}|{ScalarPattern})";

        /// <summary>
        /// Pattern for identifying variable lists.
        /// </summary>
        protected static readonly string VariableListPattern = $@"(?<Vars>{VariablePattern}(\s+{VariablePattern})*)";

        /// <summary>
        /// Pattern for identifying concatenations.
        /// </summary>
        public static readonly string ConcatPattern = $@"(\{{{VariableListPattern}\}})";

        /// <summary>
        /// Pattern for identifying concatenations of any type or any type.
        /// </summary>
        public static readonly string AnyTypePattern = $@"({ConcatPattern}|{VariablePattern})";

        /// <summary>
        /// Pattern for identifying formatters.
        /// </summary>
        protected static readonly string FormatterPattern = @"(%(?<Format>[ubhdUBHD]))";

        /// <summary>
        /// Pattern for identifying submodule instantiations.
        /// </summary>
        public static readonly string InstantiationPattern = @"(?<Design>\w+)\.(?<Name>\w+)";

        /// <summary>
        /// Pattern for identifying components (inputs or outputs) in a module notation.
        /// </summary>
        private static readonly string ModuleComponentPattern = $@"({AnyTypePattern}(,\s+{AnyTypePattern})*)";

        /// <summary>
        /// Pattern for identifying modules.
        /// </summary>
        public static readonly string ModulePattern = $@"(?<Components>(?<Inputs>{ModuleComponentPattern})\s+:\s+(?<Outputs>{ModuleComponentPattern}))";

        /// <summary>
        /// Pattern for identifying operators.
        /// </summary>
        private static readonly string OperatorPattern = $@"^(([=+^|-])|(<=(@{ScalarPattern})?)|(~+)|(==))$";

        /// <summary>
        /// Pattern for identifying seperators.
        /// </summary>
        private static readonly string SeperatorPattern = @"[\s{}():,;]";

        /// <summary>
        /// Pattern for identifying invalid characters.
        /// </summary>
        private static readonly string InvalidPattern = @"[^\s_a-zA-Z0-9~@%^*()=+[\]{}<|:;',.-]";

        /// <summary>
        /// Regex for identifying bits.
        /// </summary>
        private static Regex BitRegex { get; } = new Regex(@"\d+$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalars. (Optional ~ or *)
        /// </summary>
        private static Regex ScalarRegex { get; } = new Regex($"^[~*]*{ScalarPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vectors. (Optional ~ or *)
        /// </summary>
        private static Regex VectorRegex { get; } = new Regex($"^[~*]*{VectorPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying constants. (Optional ~)
        /// </summary>
        public static Regex ConstantRegex { get; } = new Regex($"^~*{ConstantPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying formatters.
        /// </summary>
        private static Regex FormatterRegex = new Regex($"^{FormatterPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying submodule instantiations.
        /// </summary>
        private static Regex InstantiationRegex = new Regex($"^{InstantiationPattern}$");

        /// <summary>
        /// Regex for identifying operators.
        /// </summary>
        private static Regex OperatorRegex = new Regex(OperatorPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying seperators.
        /// </summary>
        private static Regex SeperatorRegex = new Regex(SeperatorPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying invalid characters.
        /// </summary>
        private static Regex InvalidRegex = new Regex(InvalidPattern, RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// Line number of the design being parsed.
        /// </summary>
        protected int LineNumber;

        /// <summary>
        /// List of errors.
        /// </summary>
        protected List<string> ErrorLog;

        /// <summary>
        /// The design being parsed.
        /// </summary>
        protected Design Design;

        /// <summary>
        /// List of libraries included for this instance.
        /// </summary>
        protected List<string> Libraries;

        /// <summary>
        /// Dictionary of submodules for this instance.
        /// </summary>
        public Dictionary<string, string> Subdesigns;

        /// <summary>
        /// Dictionary of instantiations for this instance.
        /// </summary>
        public Dictionary<string, string> Instantiations;

        /// <summary>
        /// Memo for vector expansions.
        /// </summary>
        protected static Dictionary<string, IEnumerable<string>> ExpansionMemo = new Dictionary<string, IEnumerable<string>>();

        /// <summary>
        /// Constructs a lexer to verify the design.
        /// </summary>
        /// <param name="design">Design to parse</param>
        protected Lexer(Design design)
        {
            ErrorLog = new List<string>();
            Design = design;
            Libraries = new List<string>();
            Subdesigns = new Dictionary<string, string>();
            Instantiations = new Dictionary<string, string>();
        }

        // To Do: Format Specifiers not inside each other.
        //        All variables in this kind of statement must be in a formatter.

        protected StatementType? GetStatementType(string line)
        {
            // Create statement type
            StatementType? statementType = null;
            // Create tokens list
            List<string> tokens = new List<string>();
            // Create string builder for current lexeme
            StringBuilder lexeme = new StringBuilder();
            // Create groupings stack
            Stack<char> groupings = new Stack<char>();

            // Iterate through all characters in the provided line
            foreach (char c in line)
            {
                // Get character as a string
                string newChar = c.ToString();
                // Get current lexme
                string currentLexeme = lexeme.ToString();

                // If the character is an invalid character
                if (InvalidRegex.IsMatch(newChar))
                {
                    // Add invalid character error to error log
                    ErrorLog.Add($"{LineNumber}: Invalid character '{c}'.");
                    // Return null
                    return null;
                }
                // If the character is a seperator character
                else if (SeperatorRegex.IsMatch(newChar))
                {
                    // If there is a token before the seperator
                    if (currentLexeme.Length > 0)
                    {
                        // If current lexeme isn't a token or isn't valid
                        if (!IsToken(currentLexeme, c) || !ValidateCurrentToken(line, c, currentLexeme, ref statementType, groupings))
                        {
                            // Return null
                            return null;
                        }

                        // Add current lexeme to tokens list
                        tokens.Add(currentLexeme);
                        // Clear current lexeme
                        lexeme = lexeme.Clear();
                    }

                    // Validate ending token
                    if (!ValidateSeperatorToken(line, c, currentLexeme, ref statementType, groupings, tokens))
                    {
                        return null;
                    }

                    // If character is an opening grouping
                    if (c == '{' || c == '(')
                    {
                        // Add grouping char to stack
                        groupings.Push(c);
                    }
                    // If character is a closing grouping
                    else if (c == '}' || c == ')')
                    {
                        // If groupings stack isn't empty
                        if (groupings.Count > 0)
                        {
                            // Get the top grouping
                            char top = groupings.Peek();
                            // If the top grouping matches the closing grouping
                            if ((c == ')' && top == '(') || (c == '}' && top == '{'))
                            {
                                // Pop top grouping
                                groupings.Pop();
                            }
                            // If the top grouping doesn't match the closing grouping
                            else
                            {
                                // Add grouping error to error log
                                ErrorLog.Add($"{LineNumber}: '{top}' must be matched first.");
                                // Return null
                                return null;
                            }
                        }
                        // If groupings stack is empty
                        else
                        {
                            // Add grouping error to error log
                            ErrorLog.Add($"{LineNumber}: Unmatched '{c}'.");
                            // Return null
                            return null;
                        }
                    }

                    if (c == '\n')
                    {
                        LineNumber++;
                    }
                    // Add seperator to tokens list
                    tokens.Add(newChar);
                }
                // If the character is not a seperator character
                else
                {
                    // Check for constant inside {}
                    if (c == '\'')
                    {
                        // Check for constant bit count inside {}
                        if (groupings.Count > 0 && groupings.Peek() == '{' && (String.IsNullOrEmpty(currentLexeme) || !currentLexeme.All(ch => Char.IsDigit(ch))))
                        {
                            ErrorLog.Add($"{LineNumber}: Constants in concatenation fields must specify bit count.");
                            return null;
                        }
                    }

                    // Append new character to the current lexeme
                    lexeme.Append(c);
                }
            }

            // If there are unclosed groupings
            if (groupings.Count > 0)
            {
                // Add groupings error to error log
                ErrorLog.Add($"{LineNumber}: '{groupings.Peek()}' is not matched.");
                // Return null
                return null;
            }

            // If there are no errors and the statement type is still null
            if (statementType == null)
            {
                // Set statement type to variable list
                statementType = StatementType.VariableList;
            }

            // Return statement type
            return statementType;
        }

        #region Token Verifications

        /// <summary>
        /// Returns whether the provided lexeme is a token.
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <param name="seperatorChar">Character seperator</param>
        /// <returns>Whether the provided lexeme is a token</returns>
        private bool IsToken(string lexeme, char seperatorChar)
        {
            if (lexeme == Design.FileName && seperatorChar == '(')
            {
                return true;
            }
            else if (IsScalar(lexeme))
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
            else if (FormatterRegex.IsMatch(lexeme))
            {
                return true;
            }
            else if (InstantiationRegex.IsMatch(lexeme) && seperatorChar == '(')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether a lexeme is a scalar.
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <returns>Whether the lexeme is a scalar</returns>
        private bool IsScalar(string lexeme)
        {
            if (ScalarRegex.IsMatch(lexeme))
            {
                // Get scalar name and bit
                Match bitMatch = BitRegex.Match(lexeme);
                int bit = bitMatch.Success ? Convert.ToInt32(bitMatch.Value) : -1;
                string name = bitMatch.Success ? lexeme.Substring(0, bitMatch.Index) : lexeme;

                // Check for invalid bit
                if (bit > 31)
                {
                    ErrorLog.Add($"{LineNumber}: Bit count of '{name}' must be between 0 and 31.");
                    return false;
                }

                // Check scalar name has at least one letter
                if (!name.Any(c => char.IsLetter(c)))
                {
                    ErrorLog.Add($"{LineNumber}: Scalar name '{name}' must contain at least one letter.");
                    return false;
                }

                name = name.TrimStart('*').TrimStart('~');
                lexeme = lexeme.TrimStart('*').TrimStart('~');

                // Check for taken namespace
                if (Design.Database.GetComponents(name) != null && bit == -1)
                {
                    ErrorLog.Add($"{LineNumber}: Namespace '{name}' is already being used by a vector.");
                    return false;
                }

                // Add namespace component
                if (bit != -1)
                {
                    Design.Database.AddNamespaceComponent(name, lexeme);
                }
                else
                {
                    if (!Design.Database.HasNamespace(name))
                    {
                        Design.Database.AddNamespaceComponent(name, null);
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
                    ErrorLog.Add($"{LineNumber}: Vector name '{vectorNamespace}' cannot end in a number.");
                    return false;
                }

                // Check vector bounds and step
                int leftBound = String.IsNullOrEmpty(match.Groups["LeftBound"].Value) ? -1 : Convert.ToInt32(match.Groups["LeftBound"].Value);
                int step = String.IsNullOrEmpty(match.Groups["Step"].Value) ? -1 : Convert.ToInt32(match.Groups["Step"].Value);
                int rightBound = String.IsNullOrEmpty(match.Groups["RightBound"].Value) ? -1 : Convert.ToInt32(match.Groups["RightBound"].Value);
                if (leftBound > 31 || rightBound > 31)
                {
                    ErrorLog.Add($"{LineNumber}: Vector bounds of '{lexeme}' must be between 0 and 31.");
                    return false;
                }
                else if (step > 31)
                {
                    ErrorLog.Add($"{LineNumber}: Vector step of '{lexeme}' must be between 0 and 31.");
                    return false;
                }

                // Check for invalid [] notation
                if (!Design.Database.HasNamespace(vectorNamespace) && leftBound == -1)
                {
                    ErrorLog.Add($"{LineNumber}: '{vectorNamespace}[]' cannot be used without an explicit dimension somewhere.");
                    return false;
                }

                // Check for taken namespace
                if (Design.Database.GetComponents(vectorNamespace) == null && Design.Database.HasNamespace(vectorNamespace))
                {
                    ErrorLog.Add($"{LineNumber}: Namespace '{vectorNamespace}' is already being used by a scalar.");
                    return false;
                }

                if (leftBound != -1)
                {
                    if (leftBound < rightBound)
                    {
                        // Flips bounds so MSB is the leftBound
                        leftBound = leftBound + rightBound;
                        rightBound = leftBound - rightBound;
                        leftBound = leftBound - rightBound;
                    }

                    for (int i = leftBound; i >= rightBound; i--)
                    {
                        Design.Database.AddNamespaceComponent(vectorNamespace, String.Concat(vectorNamespace, i));
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
                    ErrorLog.Add($"{LineNumber}: Constant can have at most 32 bits.");
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

        #region Token Validation

        /// <summary>
        /// Validates the provided token with the current character, statement type and the grouping stack.
        /// </summary>
        /// <param name="line">Current line</param>
        /// <param name="currentChar">Character being appended to the current lexeme</param>
        /// <param name="currentLexeme">Current lexeme</param>
        /// <param name="type">Statement type of the line</param>
        /// <param name="groupings">Groupings stack</param>
        /// <returns>Whether the token is validate in its current context</returns>
        private bool ValidateCurrentToken(string line, char currentChar, string currentLexeme, ref StatementType? type, Stack<char> groupings)
        {
            // Check for invalid tokens with current statement type
            if (currentLexeme == "=")
            {
                if (type != null)
                {
                    ErrorLog.Add($"{LineNumber}: '{currentLexeme}' can only be used after the dependent in a boolean statement.");
                    return false;
                }
                else
                {
                    type = StatementType.Boolean;
                }

                if (line.Substring(0, line.IndexOf(currentLexeme)).Contains("*"))
                {
                    ErrorLog.Add($"{LineNumber}: '*' can only be used in a variable list statement.");
                    return false;
                }

                if (line.Substring(0, line.IndexOf(currentLexeme)).Contains("'"))
                {
                    ErrorLog.Add($"{LineNumber}: Constants can't be used on the left side of a boolean statement.");
                    return false;
                }
            }
            else if (currentLexeme.Contains("<="))
            {
                if (type != null)
                {
                    ErrorLog.Add($"{LineNumber}: '{currentLexeme}' can only be used after the dependent in a clock statement.");
                    return false;
                }
                else
                {
                    type = StatementType.Clock;
                }

                if (line.Substring(0, line.IndexOf(currentLexeme)).Contains("*"))
                {
                    ErrorLog.Add($"{LineNumber}: '*' can only be used in a variable list statement.");
                    return false;
                }

                if (line.Substring(0, line.IndexOf(currentLexeme)).Contains("'"))
                {
                    ErrorLog.Add($"{LineNumber}: Constants can't be used on the left side of a clock statement.");
                    return false;
                }
            }
            else if (Regex.IsMatch(currentLexeme, @"^([+|^-])|(==)$"))
            {
                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                {
                    ErrorLog.Add($"{LineNumber}: '{currentLexeme}' operator can only be used in a boolean or clock statement.");
                    return false;
                }
            }
            else if (currentLexeme.Contains("%"))
            {
                if (type != null && type != StatementType.FormatSpecifier)
                {
                    ErrorLog.Add($"{LineNumber}: '{currentLexeme}' can only be used in a format specifier statement.");
                    return false;
                }
                else if (type == null)
                {
                    type = StatementType.FormatSpecifier;
                }
            }
            else if (currentLexeme.Contains("~"))
            {
                if (currentLexeme == "~" && currentChar != '(' && currentChar != '{')
                {
                    ErrorLog.Add($"{LineNumber}: '~' must be attached to a scalar, vector, constant, parenthesis or concatenation.");
                    return false;
                }

                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                {
                    ErrorLog.Add($"{LineNumber}: '~' can only be used in on the right side of a boolean or clock statement.");
                    return false;
                }

                if (groupings.Count > 0 && groupings.Peek() == '{')
                {
                    ErrorLog.Add($"{LineNumber}: '~' can't be used inside a concatenation field.");
                    return false;
                }
            }
            else if (currentLexeme.Contains("*"))
            {
                if (type != null)
                {
                    ErrorLog.Add($"{LineNumber}: '*' can only be used in a variable list statement.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates the seperator token with the current lexeme, statement type, groupings stack and the previous tokens.
        /// </summary>
        /// <param name="line">Current line</param>
        /// <param name="seperatorChar">Character that is ending the current lexeme</param>
        /// <param name="currentLexeme">Current lexeme</param>
        /// <param name="type">Statement type of the line</param>
        /// <param name="groupings">Groupings stack</param>
        /// <param name="tokens">Previous tokens</param>
        /// <returns></returns>
        private bool ValidateSeperatorToken(string line, char seperatorChar, string currentLexeme, ref StatementType? type, Stack<char> groupings, List<string> tokens)
        {
            if (seperatorChar == '{' || seperatorChar == '}')
            {
                if (seperatorChar == '{' && groupings.Count > 0 && groupings.Peek() == '{')
                {
                    ErrorLog.Add($"{LineNumber}: Concatenations can't be used inside of other concatenations.");
                    return false;
                }
            }
            else if (seperatorChar == '(' || seperatorChar == ')')
            {
                if (groupings.Count > 0 && groupings.Peek() == '{')
                {
                    ErrorLog.Add($"{LineNumber}: '{seperatorChar}' can't be used in a concatenation.");
                    return false;
                }

                if (currentLexeme == Design.FileName)
                {
                    type = StatementType.Module;
                }
                else if (InstantiationRegex.IsMatch(currentLexeme))
                {
                    if (!ValidateInstantiation(InstantiationRegex.Match(currentLexeme), line))
                    {
                        return false;
                    }

                    type = StatementType.Submodule;
                }

                if (type == StatementType.FormatSpecifier || type == null)
                {
                    ErrorLog.Add($"{LineNumber}: '{seperatorChar}' can't be used in a format specifier or variable list statement.");
                    return false;
                }
            }
            else if (seperatorChar == ';')
            {
                if (tokens.Count == 0 || tokens.Contains(";"))
                {
                    ErrorLog.Add($"{LineNumber}: ';' can only be used to end a statement.");
                    return false;
                }
            }
            else if (seperatorChar == ',')
            {
                // Check for misplaced comma
                if (groupings.Count == 0 || groupings.Peek() != '(')
                {
                    ErrorLog.Add($"{LineNumber}: ',' can only be used inside the () in a submodule or module statement.");
                    return false;
                }
                else
                {
                    if (!(type == StatementType.Submodule || type == StatementType.Module))
                    {
                        ErrorLog.Add($"{LineNumber}: ',' can only be used inside the () in a submodule or module statement.");
                        return false;
                    }
                }
            }
            else if (seperatorChar == ':')
            {
                if (groupings.Count == 0 || groupings.Peek() != '(')
                {
                    ErrorLog.Add($"{LineNumber}: ':' can only be used to seperate input and output variables in a module or submodule statement.");
                    return false;
                }
                else
                {
                    if (!(type == StatementType.Module || type == StatementType.Submodule))
                    {
                        ErrorLog.Add($"{LineNumber}: ':' can only be used to seperate input and output variables in a module or submodule statement.");
                        return false;
                    }
                    else
                    {
                        if (tokens.Contains(":"))
                        {
                            ErrorLog.Add($"{LineNumber}: ':' can only be used once in a module or submodule statement.");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Validates a submodule instantiation.
        /// </summary>
        /// <param name="instantiationMatch">Instantiation</param>
        /// <param name="line">Line of instantiation</param>
        /// <returns>Whether the instantiation is valid</returns>
        private bool ValidateInstantiation(Match instantiationMatch, string line)
        {
            string designName = instantiationMatch.Groups["Design"].Value;
            string instantiationName = instantiationMatch.Groups["Name"].Value;

            if (designName == Design.FileName)
            {
                ErrorLog.Add($"{LineNumber}: You cannot instantiate from the current design.");
                return false;
            }

            if (Instantiations.ContainsKey(instantiationName))
            {
                ErrorLog.Add($"{LineNumber}: Instantiation name '{instantiationName}' is already being used.");
                return false;
            }
            else
            {
                try
                {
                    if (!Subdesigns.ContainsKey(designName))
                    {
                        string file = null;
                        string[] files = Directory.GetFiles(Design.FileSource.DirectoryName, String.Concat(designName, ".vbi"));
                        if (files.Length > 0)
                        {
                            // Check for module Declaration
                            if (DesignHasModuleDeclaration(files[0]))
                            {
                                file = files[0];
                            }
                        }

                        if (file == null)
                        {
                            for (int i = 0; i < Libraries.Count; i++)
                            {
                                files = Directory.GetFiles(Libraries[i], String.Concat(designName, ".vbi"));
                                if (files.Length > 0)
                                {
                                    // Check for module Declaration
                                    if (DesignHasModuleDeclaration(files[0]))
                                    {
                                        file = files[0];
                                        break;
                                    }
                                }
                            }

                            if (file == null)
                            {
                                // Not found
                                ErrorLog.Add($"{LineNumber}: Unable to find a design named '{designName}' with a module declaration.");
                                return false;
                            }
                        }

                        Subdesigns.Add(designName, file);
                        Instantiations.Add(instantiationName, line);
                    }
                    else
                    {
                        Instantiations.Add(instantiationName, line);
                    }

                    return true;
                }
                catch (Exception)
                {
                    ErrorLog.Add($"{LineNumber}: Error locating '{designName}'.");
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns whether the design has a module declaration.
        /// </summary>
        /// <param name="designPath">Path to design</param>
        /// <returns>Whether the design has a module declaration</returns>
        private bool DesignHasModuleDeclaration(string designPath)
        {
            FileInfo fileInfo = new FileInfo(designPath);
            string name = fileInfo.Name.Split('.')[0];
            using (StreamReader reader = fileInfo.OpenText())
            {
                string nextLine = string.Empty;
                while ((nextLine = reader.ReadLine()) != null)
                {
                    if (Regex.IsMatch(nextLine, $@"^\s*{name}\({ModulePattern}\);$"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Expansion Methods

        /// <summary>
        /// Expands a vector into its components.
        /// </summary>
        /// <param name="vector">Vector to expand</param>
        /// <returns>List of vector components</returns>
        protected List<string> ExpandVector(Match vector)
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
                int step = String.IsNullOrEmpty(vector.Groups["Step"].Value) ? 1 : Convert.ToInt32(vector.Groups["Step"].Value);

                // Expand vector
                for (int i = leftBound; i <= rightBound; i += step)
                {
                    expansion.Add(String.Concat(name, i));
                }
            }
            else
            {
                int step = String.IsNullOrEmpty(vector.Groups["Step"].Value) ? -1 : -Convert.ToInt32(vector.Groups["Step"].Value);

                // Expand vector
                for (int i = leftBound; i >= rightBound; i += step)
                {
                    expansion.Add(String.Concat(name, i));
                }
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
        protected List<string> ExpandConstant(Match constant)
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
            else if (constant.Groups["Format"].Value == "b" || constant.Groups["Format"].Value == "B")
            {
                charBits = constant.Groups["Value"].Value.ToCharArray();
            }
            else
            {
                outputBinary = Convert.ToString(Convert.ToInt32(constant.Groups["Value"].Value, 10), 2);
                charBits = outputBinary.ToCharArray();
            }

            int[] bits = Array.ConvertAll(charBits, bit => (int)Char.GetNumericValue(bit));
            int specifiedBitCount = String.IsNullOrEmpty(constant.Groups["BitCount"].Value)
                ? -1
                : Convert.ToInt32(constant.Groups["BitCount"].Value);

            if (specifiedBitCount != -1 && specifiedBitCount < bits.Length)
            {
                // Error
                ErrorLog.Add($"{LineNumber}: {constant.Value} doesn't specify enough bits.");
                return null;
            }
            else if (specifiedBitCount > bits.Length)
            {
                // Add padding
                for (int i = 0; i < specifiedBitCount - bits.Length; i++)
                {
                    expansion.Add("0");
                }
            }

            // Add bits to expansion
            foreach (int bit in bits)
            {
                expansion.Add(bit.ToString());
            }

            // Save expansion
            if (!ExpansionMemo.ContainsKey(constant.Value))
            {
                ExpansionMemo.Add(constant.Value, expansion);
            }

            return expansion;
        }

        #endregion
    }
}
using System;
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
        #region Statement Types

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
        private static readonly string NamePattern = @"(?<Name>[_a-zA-Z]\w{0,19})";

        /// <summary>
        /// Pattern for identifying scalars. (No ~ or *)
        /// </summary>
        public static readonly string ScalarPattern = $@"({NamePattern}\d{{0,2}})";

        /// <summary>
        /// Pattern for identifying scalars. (Optional *)
        /// </summary>
        protected static readonly string ScalarPattern2 = $@"(?<!('|\.))(\*?{ScalarPattern})(?!\.)";

        /// <summary>
        /// Pattern for identifying vectors. (No ~ or *)
        /// </summary>
        protected static readonly string VectorPattern = $@"({NamePattern}((\[(?<LeftBound>\d{{1,2}})\.(?<Step>\d{{1,2}})?\.(?<RightBound>\d{{1,2}})\])|(\[\])))";

        /// <summary>
        /// Pattern for identifying vectors. (Optional *)
        /// </summary>
        protected static readonly string VectorPattern2 = $@"\*?{VectorPattern}";

        /// <summary>
        /// Pattern for identifying constants. (No ~)
        /// </summary>
        public static readonly string ConstantPattern = $@"((?<BitCount>\d{{1,2}})?'(((?<Format>[hH])(?<Value>[a-fA-F\d]+))|((?<Format>[dD])(?<Value>\d+))|((?<Format>[bB])(?<Value>[0-1]+))))";

        /// <summary>
        /// Pattern for identifying scalars, vectors and constants. (No ~ or *)
        /// </summary>
        protected static readonly string VariablePattern = $@"({VectorPattern}|{ConstantPattern}|{ScalarPattern})";

        /// <summary>
        /// Pattern for identifying scalars, vectors and constants. (Optional *)
        /// </summary>
        protected static readonly string VariablePattern2 = $@"({VectorPattern2}|{ConstantPattern}|{ScalarPattern2})";

        /// <summary>
        /// Pattern for identifying variable lists.
        /// </summary>
        public static readonly string VariableListPattern = $@"(?<Vars>{VariablePattern}(\s+{VariablePattern})*)";

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
        protected static readonly string InstantiationPattern = @"(?<Design>\w+)\.(?<Name>\w+)";

        /// <summary>
        /// Pattern for identifying components (inputs or outputs) in a module notation.
        /// </summary>
        protected static readonly string ModuleComponentPattern = $@"({AnyTypePattern}(,\s+{AnyTypePattern})*)";

        /// <summary>
        /// Pattern for identifying modules.
        /// </summary>
        public static readonly string ModulePattern = $@"(?<Components>(?<Inputs>{ModuleComponentPattern})\s+:\s+(?<Outputs>{ModuleComponentPattern}))";

        /// <summary>
        /// Pattern for identifying module instantiations.
        /// </summary>
        public static readonly string ModuleInstantiationPattern = $@"((?<Padding>\s*)?(?<Instantiation>{InstantiationPattern})\({ModulePattern}\))";

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
        private static readonly string InvalidPattern = @"[^\s_a-zA-Z0-9~@%^*()=+[\]{}|;'#<>,.-]";

        /// <summary>
        /// Pattern for identifying whitespace.
        /// </summary>
        public static Regex WhitespaceRegex { get; } = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying bits.
        /// </summary>
        private static Regex BitRegex { get; } = new Regex(@"\d+$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalars. (Optional ~ or *)
        /// </summary>
        private static Regex ScalarRegex { get; } = new Regex($"^[~*]*{ScalarPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalars. (Optional *)
        /// </summary>
        protected static Regex ScalarRegex2 { get; } = new Regex(ScalarPattern2, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vectors. (Optional ~ or *)
        /// </summary>
        private static Regex VectorRegex { get; } = new Regex($"^[~*]*{VectorPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vectors. (Optional *)
        /// </summary>
        private static Regex VectorRegex2 { get; } = new Regex(VectorPattern2, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying constants. (Optional ~)
        /// </summary>
        public static Regex ConstantRegex { get; } = new Regex($"^~*{ConstantPattern}$", RegexOptions.Compiled);

        private static Regex ConstantRegex2 { get; } = new Regex($"(?!(('b1)|('b0)|(1'b1)|(1'b0)))({ConstantPattern})", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalars, vectors and constants.
        /// </summary>
        private static Regex VariableRegex { get; } = new Regex(VariablePattern2, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying concatenations.
        /// </summary>
        private static Regex ConcatRegex = new Regex($@"((?<!{FormatterPattern}){ConcatPattern})", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying concatenations of any type or any type.
        /// </summary>
        protected static Regex AnyTypeRegex = new Regex(AnyTypePattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying formatters.
        /// </summary>
        private static Regex FormatterRegex = new Regex($"^{FormatterPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying submodule instantiations.
        /// </summary>
        private static Regex InstantiationRegex = new Regex($"^{InstantiationPattern}$");

        /// <summary>
        /// Regex for determining whether expansion is required.
        /// </summary>
        protected static Regex ExpansionRegex { get; } = new Regex($@"((?<!{FormatterPattern}){ConcatPattern})|{VectorPattern}|{ConstantPattern}", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying module instantiations.
        /// </summary>
        protected static Regex ModuleInstantiationRegex = new Regex(ModuleInstantiationPattern);

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
        protected static Dictionary<string, List<string>> ExpansionMemo = new Dictionary<string, List<string>>();

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

        /// <summary>
        /// Returns the type of statement for a given line.
        /// </summary>
        /// <param name="line">Line to interpret</param>
        /// <returns>Type of statement</returns>
        protected StatementType? GetStatementType(string line)
        {
            StatementType? type = StatementType.Empty;
            List<string> tokens = new List<string>();
            Stack<char> groupings = new Stack<char>();
            StringBuilder lexeme = new StringBuilder();
            string currentLexeme;
            string newChar;

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
                        ErrorLog.Add($"{LineNumber}: Invalid '\"'.");
                        return null;
                    }

                    // Make sure no other tokens exist
                    if (tokens.Any(token => token != " "))
                    {
                        ErrorLog.Add($"{LineNumber}: Invalid '\"'.");
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
                            if (!ValidateCurrentToken(line, c, currentLexeme, ref type, groupings))
                            {
                                return null;
                            }
                        }
                        else
                        {
                            if (type != StatementType.Empty || c != '(' || !InstantiationRegex.IsMatch(currentLexeme))
                            {
                                // If token is not valid and is not an instantiation
                                ErrorLog.Add($"{LineNumber}: Invalid '{currentLexeme}'.");
                                return null;
                            }
                        }

                        tokens.Add(currentLexeme);
                        lexeme.Clear();
                    }

                    // Validate ending token
                    if (!ValidateSeperatorToken(line, c, currentLexeme, ref type, groupings, tokens))
                    {
                        return null;
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
                                ErrorLog.Add($"{LineNumber}: '{top}' must be matched first.");
                                return null;
                            }
                        }
                        else
                        {
                            ErrorLog.Add($"{LineNumber}: Unmatched '{c}'.");
                            return null;
                        }
                    }

                    tokens.Add(c.ToString());
                }
                else if (InvalidRegex.IsMatch(newChar))
                {
                    // Invalid char
                    ErrorLog.Add($"{LineNumber}: Invalid character '{c}'.");
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
                            ErrorLog.Add($"{LineNumber}: Constants in concatenation fields must specify bit count.");
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
                    ErrorLog.Add($"{LineNumber}: '{grouping}' is not matched.");
                }
                return null;
            }

            // At this point, if type is Empty, type should be set to VariableList
            if (type == StatementType.Empty)
            {
                type = StatementType.VariableList;
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
            else if (FormatterRegex.IsMatch(lexeme))
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
                    ErrorLog.Add($"{LineNumber}: '{vectorNamespace}[]' notation cannot be used before the vector is initialized.");
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
                if (type != StatementType.Empty)
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
            }
            else if (currentLexeme.Contains("<="))
            {
                if (type != StatementType.Empty)
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
                if (type != StatementType.Empty && type != StatementType.FormatSpecifier)
                {
                    ErrorLog.Add($"{LineNumber}: '{currentLexeme}' can only be used in a format specifier statement.");
                    return false;
                }
                else if (type == StatementType.Empty)
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
                if (type != StatementType.Empty)
                {
                    ErrorLog.Add($"{LineNumber}: '*' can only be used in a variable list statement.");
                    return false;
                }
            }
            else if (currentLexeme.Contains("'"))
            {
                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                {
                    ErrorLog.Add($"{LineNumber}: Constants can only be used on the right side of a boolean or clock statement.");
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

                if (type == StatementType.FormatSpecifier || type == StatementType.Empty)
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

                if (tokens.Last() == " ")
                {
                    ErrorLog.Add($"{LineNumber}: Spaces cannot occur before ';'.");
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
                ErrorLog.Add($"{LineNumber}: {constant.Value} doesn't specify enough bits.");
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

            // Add bits to expansion
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
        protected List<string> ExpandToken(Match token)
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
        protected string ExpandHorizontally(string line)
        {
            string expandedLine = line;
            Match match;

            while ((match = VectorRegex2.Match(expandedLine)).Success)
            {
                // Replace matched vector with its components
                expandedLine = expandedLine.Substring(0, match.Index) + String.Join(" ", GetExpansion(match)) + expandedLine.Substring(match.Index + match.Length);
            }

            while ((match = ConstantRegex2.Match(expandedLine)).Success)
            {
                // Replace matched constants with its components
                expandedLine = expandedLine.Substring(0, match.Index) + String.Join(" ", GetExpansion(match)) + expandedLine.Substring(match.Index + match.Length);
            }

            if (line.Contains("=") || line.Contains(":"))
            {
                Regex variableListRegex = new Regex($@"{VariableListPattern}(?![^{{}}]*\}})"); // Variable lists not inside {}
                while ((match = variableListRegex.Match(expandedLine)).Success)
                {
                    // Add { } to the matched variable list
                    expandedLine = expandedLine.Substring(0, match.Index) + String.Concat("{", match.Value, "}") + expandedLine.Substring(match.Index + match.Length);
                }
            }
            else
            {
                while ((match = ConcatRegex.Match(expandedLine)).Success)
                {
                    // Replace matched concat with its components
                    expandedLine = expandedLine.Substring(0, match.Index) + String.Join(" ", GetExpansion(match)) + expandedLine.Substring(match.Index + match.Length);
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
            string expanded = String.Empty;

            // Get dependent and expression
            int start = line.ToList<char>().FindIndex(c => char.IsWhiteSpace(c) == false); // First non whitespace character
            string dependent = line.Contains("<")
                ? line.Substring(start, line.IndexOf("<") - start).TrimEnd()
                : line.Substring(start, line.IndexOf("=") - start).TrimEnd();
            string expression = line.Substring(line.IndexOf("=") + 1).TrimStart();

            // Expand dependent
            List<string> dependentExpansion = new List<string>();
            Match dependentMatch = AnyTypeRegex.Match(dependent);
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
            MatchCollection matches = ExpansionRegex.Matches(expression);
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
                    ErrorLog.Add($"{LineNumber}: Vector and/or concatenation element counts must be consistent across the entire expression.");
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
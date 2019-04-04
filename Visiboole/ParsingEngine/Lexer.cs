using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

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
        /// Pattern for identifying scalar notation.
        /// </summary>
        protected static readonly string ScalarNotationPattern = @"((?<Name>[_a-zA-Z](?<Mid>\w{0,18})?(?(Mid)[_a-zA-Z]))(?<Bit>([1-2]|(?<Bit3>3))?(?(Bit3)[0-1]|[0-9]))?)";

        /// <summary>
        /// Pattern for identifying scalars. (With ~ or *)
        /// </summary>
        private static readonly string ScalarPattern = $"^([~*]*{ScalarNotationPattern})$";

        /// <summary>
        /// Pattern for identifying index notation.
        /// </summary>
        private static readonly string IndexNotationPattern = @"((\[(?<LeftBound>\d+)\.(?<Step>[1-9]\d*)?\.(?<RightBound>\d+)\])|(\[\]))";

        /// <summary>
        /// Pattern for identifying vector notation.
        /// </summary>
        protected static readonly string VectorNotationPattern = $"({ScalarNotationPattern}{IndexNotationPattern})";

        /// <summary>
        /// Pattern for identifying vectors. (Optional ~ or *)
        /// </summary>
        private static readonly string VectorPattern = $"^([~*]*{VectorNotationPattern})$";

        /// <summary>
        /// Pattern for identifying constant notation.
        /// </summary>
        protected static readonly string ConstantNotationPattern = @"((?<BitCount>\d{1,2})?'(((?<Format>[hH])(?<Value>[a-fA-F\d]+))|((?<Format>[dD])(?<Value>\d+))|((?<Format>[bB])(?<Value>[0-1]+))))";

        /// <summary>
        /// Pattern for identifying constants. (Optional ~)
        /// </summary>
        private static readonly string ConstantPattern = $"^(~*{ConstantNotationPattern})$";

        /// <summary>
        /// Pattern for identifying format specifier notation.
        /// </summary>
        protected static readonly string FormatSpecifierNotationPattern = @"(%(?<Format>[ubhdUBHD]))";

        /// <summary>
        /// Pattern for identifying format specifiers.
        /// </summary>
        private static readonly string FormatSpecifierPattern = $@"^{FormatSpecifierNotationPattern}$";

        /// <summary>
        /// Pattern for identifying submodule instantiation notation.
        /// </summary>
        protected static readonly string InstantiationNotationPattern = @"(?<Design>\w+)\.(?<Name>\w+)";

        /// <summary>
        /// Pattern for identifying submodule instantiations.
        /// </summary>
        private static readonly string InstantiationPattern = $@"^{InstantiationNotationPattern}$";

        /// <summary>
        /// Pattern for identifying operators.
        /// </summary>
        private static readonly string OperatorPattern = @"^(([=+^|-])|(<=)|(~+)|(==))$";

        /// <summary>
        /// Pattern for identifying seperators.
        /// </summary>
        private static readonly string SeperatorPattern = @"[\s{}():,;]";

        /// <summary>
        /// Pattern for identifying invalid characters.
        /// </summary>
        private static readonly string InvalidPattern = @"[^\s_a-zA-Z0-9~%^*()=+[\]{}|;'#<>,.-]";

        /// <summary>
        /// Regex for identifying scalars. (With ~ or *)
        /// </summary>
        public static Regex ScalarRegex { get; } = new Regex(ScalarPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vectors. (With ~ or *)
        /// </summary>
        private static Regex VectorRegex { get; } = new Regex(VectorPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying constants. (Optional ~)
        /// </summary>
        private static Regex ConstantRegex { get; } = new Regex(ConstantPattern);

        /// <summary>
        /// Regex for identifying format specifiers.
        /// </summary>
        private static Regex FormatSpecifierRegex = new Regex(FormatSpecifierPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying submodule instantiations.
        /// </summary>
        private static Regex InstantiationRegex = new Regex(InstantiationPattern);

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
        /// Indicates whether the parser is initializing.
        /// </summary>
        protected bool Init;

        /// <summary>
        /// The design being parsed.
        /// </summary>
        protected Design Design;

        /// <summary>
        /// List of libraries included for this instance.
        /// </summary>
        protected List<string> Libraries;

        /// <summary>
        /// List of instantiations for this instance.
        /// </summary>
        protected List<string> Instantiations;

        /// <summary>
        /// Dictionary of submodules.
        /// </summary>
        protected Dictionary<string, Design> Submodules;

        /// <summary>
        /// Memo for vector expansions.
        /// </summary>
        protected static Dictionary<string, List<string>> ExpansionMemo = new Dictionary<string, List<string>>();

        protected Lexer()
        {
            Libraries = new List<string>();
            Submodules = new Dictionary<string, Design>();
            Instantiations = new List<string>();
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
                                Globals.Logger.Add($"Line {LineNumber}: Invalid '{currentLexeme}'.");
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
                // Get scalar name and bit
                string name = match.Groups["Name"].Value;
                int bit = String.IsNullOrEmpty(match.Groups["Bit"].Value)
                    ? -1
                    : Convert.ToInt32(match.Groups["Bit"].Value);

                // Check scalar name has at least one letter
                if (!name.Any(c => char.IsLetter(c)))
                {
                    Globals.Logger.Add($"Line {LineNumber}: Invalid scalar name.");
                    return false;
                }

                // Check to add scalar to vector namespace
                if (bit != -1)
                {
                    Design.Database.AddVectorNamespace(name, new List<string>(new string[] { String.Concat(name, bit) }));
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

                    if (!Design.Database.HasVectorNamespace(vectorNamespace) && leftBound == -1)
                    {
                        Globals.Logger.Add($"Line {LineNumber}: '{vectorNamespace}[]' notation cannot be used before the vector is initialized.");
                        return false;
                    }

                    if (leftBound != -1)
                    {
                        // Adds all variables in the range
                        string initVector = $"{vectorNamespace}[{leftBound}..{rightBound}]";
                        if (ExpansionMemo.ContainsKey(initVector))
                        {
                            Design.Database.AddVectorNamespace(vectorNamespace, ExpansionMemo[initVector]);
                        }
                        else
                        {
                            Design.Database.AddVectorNamespace(vectorNamespace, ExpandVector(VectorRegex.Match(initVector)));
                        }
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
                    Globals.Logger.Add($"Line {LineNumber}: '{currentLexeme}' can only be used after the dependent in a boolean statement.");
                    return false;
                }
                else
                {
                    type = StatementType.Boolean;
                }

                if (line.Substring(0, line.IndexOf(currentLexeme)).Contains("*"))
                {
                    Globals.Logger.Add($"Line {LineNumber}: '*' can only be used in a variable list statement.");
                    return false;
                }
            }
            else if (currentLexeme == "<=")
            {
                if (type != StatementType.Empty)
                {
                    Globals.Logger.Add($"Line {LineNumber}: '{currentLexeme}' can only be used after the dependent in a clock statement.");
                    return false;
                }
                else
                {
                    type = StatementType.Clock;
                }

                if (line.Substring(0, line.IndexOf(currentLexeme)).Contains("*"))
                {
                    Globals.Logger.Add($"Line {LineNumber}: '*' can only be used in a variable list statement.");
                    return false;
                }
            }
            else if (Regex.IsMatch(currentLexeme, @"^([+|^-])|(==)$"))
            {
                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                {
                    Globals.Logger.Add($"Line {LineNumber}: '{currentLexeme}' operator can only be used in a boolean or clock statement.");
                    return false;
                }
            }
            else if (currentLexeme.Contains("%"))
            {
                if (type != StatementType.Empty && type != StatementType.FormatSpecifier)
                {
                    Globals.Logger.Add($"Line {LineNumber}: '{currentLexeme}' can only be used in a format specifier statement.");
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
                    Globals.Logger.Add($"Line {LineNumber}: '~' must be attached to a scalar, vector, constant, parenthesis or concatenation.");
                    return false;
                }

                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                {
                    Globals.Logger.Add($"Line {LineNumber}: '~' can only be used in on the right side of a boolean or clock statement.");
                    return false;
                }

                if (groupings.Count > 0 && groupings.Peek() == '{')
                {
                    Globals.Logger.Add($"Line {LineNumber}: '~' can't be used inside a concatenation field.");
                    return false;
                }
            }
            else if (currentLexeme.Contains("*"))
            {
                if (type != StatementType.Empty)
                {
                    Globals.Logger.Add($"Line {LineNumber}: '*' can only be used in a variable list statement.");
                    return false;
                }
            }
            else if (currentLexeme.Contains("'"))
            {
                if (!(type == StatementType.Boolean || type == StatementType.Clock))
                {
                    Globals.Logger.Add($"Line {LineNumber}: Constants can only be used on the right side of a boolean or clock statement.");
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
                    Globals.Logger.Add($"Line {LineNumber}: Concatenations can't be used inside of other concatenations.");
                    return false;
                }
            }
            else if (seperatorChar == '(' || seperatorChar == ')')
            {
                if (groupings.Count > 0 && groupings.Peek() == '{')
                {
                    Globals.Logger.Add($"Line {LineNumber}: '{seperatorChar}' can't be used in a concatenation.");
                    return false;
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
                        return false;
                    }

                    if (Instantiations.Contains(instantiationName))
                    {
                        Globals.Logger.Add($"Line {LineNumber}: Instantiation name '{instantiationName}' is already being used.");
                        return false;
                    }
                    else
                    {
                        Instantiations.Add(instantiationName);
                    }

                    type = StatementType.Submodule;
                }

                if (type == StatementType.FormatSpecifier || type == StatementType.Empty)
                {
                    Globals.Logger.Add($"Line {LineNumber}: '{seperatorChar}' can't be used in a format specifier or variable list statement.");
                    return false;
                }
            }
            else if (seperatorChar == ';')
            {
                if (tokens.Count == 0 || tokens.Contains(";"))
                {
                    Globals.Logger.Add($"Line {LineNumber}: ';' can only be used to end a statement.");
                    return false;
                }

                if (tokens.Last() == " ")
                {
                    Globals.Logger.Add($"Line {LineNumber}: Spaces cannot occur before ';'.");
                    return false;
                }
            }
            else if (seperatorChar == ',')
            {
                // Check for misplaced comma
                if (groupings.Count == 0 || groupings.Peek() != '(')
                {
                    Globals.Logger.Add($"Line {LineNumber}: ',' can only be used inside the () in a submodule or module statement.");
                    return false;
                }
                else
                {
                    if (!(type == StatementType.Submodule || type == StatementType.Module))
                    {
                        Globals.Logger.Add($"Line {LineNumber}: ',' can only be used inside the () in a submodule or module statement.");
                        return false;
                    }
                }
            }
            else if (seperatorChar == ':')
            {
                if (groupings.Count == 0 || groupings.Peek() != '(')
                {
                    Globals.Logger.Add($"Line {LineNumber}: ':' can only be used to seperate input and output variables in a module or submodule statement.");
                    return false;
                }
                else
                {
                    if (!(type == StatementType.Module || type == StatementType.Submodule))
                    {
                        Globals.Logger.Add($"Line {LineNumber}: ':' can only be used to seperate input and output variables in a module or submodule statement.");
                        return false;
                    }
                    else
                    {
                        if (tokens.Contains(":"))
                        {
                            Globals.Logger.Add($"Line {LineNumber}: ':' can only be used once in a module or submodule statement.");
                            return false;
                        }
                    }
                }
            }

            return true;
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

        #endregion
    }
}
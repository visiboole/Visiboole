using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        #region Lexer Enums and Classes

        /// <summary>
        /// Statement type proposed by the lexer.
        /// </summary>
        protected enum StatementType
        {
            Boolean,
            Clock,
            Comment,
            Empty,
            Display,
            Library,
            Module,
            Submodule,
        }

        /// <summary>
        /// Token type proposed by the lexer.
        /// </summary>
        protected enum TokenType
        {
            Variable,
            Constant,
            Comment,
            Library,
            Assignment,
            Clock,
            Asterick,
            NegationOperator,
            OrOperator,
            ExclusiveOrOperator,
            EqualToOperator,
            MathOperator,
            Formatter,
            Declaration,
            Instantiation,
            Whitespace,
            Newline,
            Semicolon,
            Colon,
            Comma,
            OpenParenthesis,
            CloseParenthesis,
            OpenBrace,
            CloseBrace
        }

        /// <summary>
        /// Represents a token recognized by the lexer.
        /// </summary>
        protected class Token
        {
            /// <summary>
            /// Gets the type of the token.
            /// </summary>
            public TokenType Type { get; private set; }

            /// <summary>
            /// Gets the text of the token.
            /// </summary>
            public string Text { get; private set; }

            /// <summary>
            /// Constructs a token with the provided text and type.
            /// </summary>
            /// <param name="text">Text of token</param>
            /// <param name="type">Type of token</param>
            public Token(string text, TokenType type)
            {
                Text = text;
                Type = type;
            }
        }

        #endregion

        #region Lexer Patterns and Regular Expressions

        /// <summary>
        /// Pattern for identifying invalid characters.
        /// </summary>
        private static readonly string InvalidPattern = @"[^\sa-zA-Z0-9[\].(){}<=@~*|^+;%#'"":,-]";

        /// <summary>
        /// Pattern for identifying names.
        /// </summary>
        public static readonly string ScalarPattern = @"(?<Name>[a-zA-Z][[a-zA-Z0-9]*)";

        /// <summary>
        /// Pattern for identfying indexes.
        /// </summary>
        private static readonly string IndexPattern = @"(\[(?<LeftBound>\d+)\.(?<Step>\d+)?\.(?<RightBound>\d+)\]|\[\])";

        /// <summary>
        /// Pattern for identifying vectors. (No ~ or *)
        /// </summary>
        protected static readonly string VectorPattern = $@"({ScalarPattern}{IndexPattern})";

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
        /// Pattern for identifying operators.
        /// </summary>
        private static readonly string OperatorPattern = $@"^(([=+*^|~-])|(<=(@{ScalarPattern})?)|(==))$";

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
        public static readonly string InstantiationPattern = @"(?<Design>\w+)\.(?<Name>[a-zA-Z0-9]+)";

        /// <summary>
        /// Pattern for identifying components (inputs or outputs) in a module notation.
        /// </summary>
        private static readonly string ModuleComponentPattern = $@"(({AnyTypePattern}|{VariableListPattern})(,\s+({AnyTypePattern}|{VariableListPattern}))*)";

        /// <summary>
        /// Pattern for identifying modules.
        /// </summary>
        public static readonly string ModulePattern = $@"(?<Components>(?<Inputs>{ModuleComponentPattern})\s+:\s+(?<Outputs>{ModuleComponentPattern}))";

        /// <summary>
        /// Regex for identifying invalid characters.
        /// </summary>
        private static Regex InvalidRegex = new Regex(InvalidPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying scalar tokens.
        /// </summary>
        private static Regex ScalarTokenRegex = new Regex($"^{ScalarPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying vector tokens.
        /// </summary>
        private static Regex VectorTokenRegex = new Regex($"^{VectorPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying constant tokens.
        /// </summary>
        private static Regex ConstantTokenRegex = new Regex($"^{ConstantPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying operator tokens.
        /// </summary>
        private static Regex OperatorTokenRegex = new Regex(OperatorPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying formatters.
        /// </summary>
        private static Regex FormatterRegex = new Regex($"^{FormatterPattern}$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for identifying submodule instantiations.
        /// </summary>
        private static Regex InstantiationRegex = new Regex($"^{InstantiationPattern}$");

        #endregion

        #region Static Fields

        /// <summary>
        /// List of separators.
        /// </summary>
        private static readonly IList<char> SeperatorsList = new ReadOnlyCollection<char>(new List<char> { ' ', '\n', ';', '(', ')', '{', '}', '~', '*', ',', ':' });

        /// <summary>
        /// List of operators.
        /// </summary>
        public static readonly IList<string> OperatorsList = new ReadOnlyCollection<string>(new List<string> { "^", "|", "+", "-", "==", " " });

        /// <summary>
        /// List of exclusive operators.
        /// </summary>
        private static readonly IList<string> ExclusiveOperatorsList = new ReadOnlyCollection<string>(new List<string> { "^", "+", "-", "==" });

        /// <summary>
        /// Memo for vector expansions.
        /// </summary>
        protected static Dictionary<string, IEnumerable<string>> ExpansionMemo = new Dictionary<string, IEnumerable<string>>();

        #endregion

        #region Instance Variables

        /// <summary>
        /// The design being parsed.
        /// </summary>
        protected Design Design;

        /// <summary>
        /// Dictionary of errors.
        /// </summary>
        protected Dictionary<int, string> ErrorLog;

        /// <summary>
        /// Line number of the design being parsed.
        /// </summary>
        protected int CurrentLineNumber;

        /// <summary>
        /// Number of lines in the design being parsed.
        /// </summary>
        protected int LineNumberCount;

        /// <summary>
        /// Indicates whether the current point of execution is inside a concatenation.
        /// </summary>
        private bool InsideConcat;

        /// <summary>
        /// Indicates whether the current point of execution is inside a formatter.
        /// </summary>
        private bool InsideFormatter;

        /// <summary>
        /// Indicates whether the current point of execution is inside a module declaration or instantiation.
        /// </summary>
        private bool InsideModule;

        /// <summary>
        /// Character groupings stack for the current execution.
        /// </summary>
        private Stack<char> Groupings;

        /// <summary>
        /// Indicates whether the current execution contains a math expression.
        /// </summary>
        private bool IsMathExpression;

        /// <summary>
        /// Exclusive operators list for the current execution.
        /// </summary>
        private List<Token> ExclusiveOperators;

        /// <summary>
        /// Operators lists for the current execution.
        /// </summary>
        private List<List<Token>> Operators;

        /// <summary>
        /// List of libraries included for this instance.
        /// </summary>
        protected List<string> Libraries;

        /// <summary>
        /// Dictionary of submodules for this instance.
        /// </summary>
        public Dictionary<string, Design> Subdesigns;

        /// <summary>
        /// Dictionary of instantiations for this instance.
        /// </summary>
        public Dictionary<string, string> Instantiations;

        #endregion

        /// <summary>
        /// Constructs a lexer to verify the design.
        /// </summary>
        /// <param name="design">Design to parse</param>
        protected Lexer(Design design)
        {
            Design = design;
            ErrorLog = new Dictionary<int, string>();
            Libraries = new List<string>();
            Subdesigns = new Dictionary<string, Design>();
            Instantiations = new Dictionary<string, string>();
        }

        #region Token Identification

        /// <summary>
        /// Returns whether a lexeme is a scalar.
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <returns>Whether the lexeme is a scalar or an error message</returns>
        private bool IsLexemeScalar(string lexeme)
        {
            // Try to match lexeme as a scalar
            Match scalarMatch = ScalarTokenRegex.Match(lexeme);
            // If lexeme is a scalar
            if (scalarMatch.Success)
            {
                // Get scalar name and bit
                string name = scalarMatch.Groups["Name"].Value;
                string bitString = string.Concat(name.ToArray().Reverse().TakeWhile(char.IsNumber).Reverse());
                int bit = string.IsNullOrEmpty(bitString) ? -1 : Convert.ToInt32(bitString);
                if (bit != -1)
                {
                    name = name.Substring(0, name.Length - bitString.Length);
                }

                // If scalar bit is larger than 31
                if (bit > 31)
                {
                    ErrorLog.Add(CurrentLineNumber, $"Bit count of '{lexeme}' must be between 0 and 31.");
                }

                // If scalar doesn't contain a bit
                if (bit == -1)
                {
                    // If namespace belongs to a vector
                    if (Design.Database.NamespaceBelongsToVector(name))
                    {
                        // Add namespace error to error log
                        ErrorLog.Add(CurrentLineNumber, $"Namespace '{name}' is already being used by a vector.");
                        return false;
                    }
                    // If namespace doesn't exist
                    else if (!Design.Database.NamespaceExists(name))
                    {
                        // Update namespace with no bit
                        Design.Database.UpdateNamespace(name, bit);
                    }
                }
                // If scalar does contain a bit
                else
                {
                    // If namespace exists and doesn't belong to a vector
                    if (Design.Database.NamespaceExists(name) && !Design.Database.NamespaceBelongsToVector(name))
                    {
                        // Add namespace error to error log
                        ErrorLog.Add(CurrentLineNumber, $"Namespace '{name}' is already being used by a scalar.");
                        return false;
                    }
                    // If namespace doesn't exist or belongs to a vector
                    else
                    {
                        // Update/add namespace with bit
                        Design.Database.UpdateNamespace(name, bit);
                    }
                }
            }

            return scalarMatch.Success;
        }

        /// <summary>
        /// Returns whether a lexeme is a vector. (If so, initializes it)
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <returns>Whether the lexeme is a vector</returns>
        private bool IsLexemeVector(string lexeme)
        {
            // Try to match lexeme as a vector
            Match vectorMatch = VectorTokenRegex.Match(lexeme);
            // If lexeme is a vector
            if (vectorMatch.Success)
            {
                // Get vector name
                string name = vectorMatch.Groups["Name"].Value;

                // If vector name ends in a number
                if (char.IsDigit(name[name.Length - 1]))
                {
                    // Add vector name error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Vector name '{name}' cannot end in a number.");
                    return false;
                }

                // Get vector bounds and step
                int leftBound = string.IsNullOrEmpty(vectorMatch.Groups["LeftBound"].Value) ? -1 : Convert.ToInt32(vectorMatch.Groups["LeftBound"].Value);
                int step = string.IsNullOrEmpty(vectorMatch.Groups["Step"].Value) ? -1 : Convert.ToInt32(vectorMatch.Groups["Step"].Value);
                int rightBound = string.IsNullOrEmpty(vectorMatch.Groups["RightBound"].Value) ? -1 : Convert.ToInt32(vectorMatch.Groups["RightBound"].Value);

                // If left bound or right bound is greater than 31
                if (leftBound > 31 || rightBound > 31)
                {
                    // Add vector bounds error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Vector bounds of '{lexeme}' must be between 0 and 31.");
                    return false;
                }
                // If step is not between 1 and 31
                else if (step == 0 || step > 31)
                {
                    // Add vector step error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Vector step of '{lexeme}' must be between 1 and 31.");
                    return false;
                }

                // If namespace exists and doesn't belong to a vector
                if (Design.Database.NamespaceExists(name) && !Design.Database.NamespaceBelongsToVector(name))
                {
                    // Add namespace error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Namespace '{name}' is already being used by a scalar.");
                    return false;
                }

                // If vector is explicit
                if (leftBound != -1)
                {
                    // If left bound is least significant bit
                    if (leftBound < rightBound)
                    {
                        // Flips bounds so left bound is most significant bit
                        leftBound = leftBound + rightBound;
                        rightBound = leftBound - rightBound;
                        leftBound = leftBound - rightBound;
                    }

                    // For each bit in the vector bounds
                    for (int i = leftBound; i >= rightBound; i--)
                    {
                        // Update/add bit to namespace
                        Design.Database.UpdateNamespace(name, i);
                    }
                }
            }

            return vectorMatch.Success;
        }

        /// <summary>
        /// Returns whether a lexeme is a constant.
        /// </summary>
        /// <param name="lexeme">Lexeme to interpret</param>
        /// <returns>Whether the lexeme is a constant</returns>
        private bool IsLexemeConstant(string lexeme)
        {
            // Try to match lexeme as a constant
            Match constantMatch = ConstantTokenRegex.Match(lexeme);
            // If lexeme is a constant
            if (constantMatch.Success)
            {
                // If the provided bit count is greater than 32 bits
                if (!string.IsNullOrEmpty(constantMatch.Groups["BitCount"].Value) && Convert.ToInt32(constantMatch.Groups["BitCount"].Value) > 32)
                {
                    // Add constant bit count error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Constant '{lexeme}' can have at most 32 bits.");
                    return false;
                }
            }

            return constantMatch.Success;
        }

        /// <summary>
        /// Gets the token type of the provided lexeme or seperator
        /// </summary>
        /// <param name="lexeme">Lexeme</param>
        /// <param name="seperator">Seperator</param>
        /// <returns>Token type of the provided lexeme or seperator</returns>
        protected TokenType? GetTokenType(string lexeme, char seperator)
        {
            if (lexeme.Length > 0)
            {
                if (lexeme == Design.FileName && seperator == '(')
                {
                    return TokenType.Declaration;
                }
                else if (IsLexemeScalar(lexeme) || IsLexemeVector(lexeme))
                {
                    if (seperator != ' ' && seperator != '\n' && seperator != ';' && seperator != ')' && seperator != '}' && seperator != ',')
                    {
                        if (seperator == '~')
                        {
                            ErrorLog.Add(CurrentLineNumber, $"'~' must be attached to the start of a variable, constant, parenthesis or concatenation.");
                        }
                        else if (seperator == '*')
                        {
                            ErrorLog.Add(CurrentLineNumber, $"'~' must be attached to the start of a variable, constant, or concatenation.");
                        }
                        else
                        {
                            ErrorLog.Add(CurrentLineNumber, $"Unrecognized '{lexeme}{seperator}'. Are you missing a space?");
                        }
                        return null;
                    }

                    return TokenType.Variable;
                }
                else if (OperatorTokenRegex.IsMatch(lexeme))
                {
                    if (seperator != ' ' && seperator != '\n')
                    {
                        if (seperator == '~')
                        {
                            ErrorLog.Add(CurrentLineNumber, $"'~' must be attached to the start of a variable, constant, parenthesis or concatenation.");
                        }
                        else if (seperator == '*')
                        {
                            ErrorLog.Add(CurrentLineNumber, $"'~' must be attached to the start of a variable, constant, or concatenation.");
                        }
                        else
                        {
                            ErrorLog.Add(CurrentLineNumber, $"Unrecognized '{lexeme}{seperator}'. Are you missing a space?");
                        }
                        return null;
                    }

                    if (lexeme == "|")
                    {
                        return TokenType.OrOperator;
                    }
                    else if (lexeme == "+" || lexeme == "-")
                    {
                        return TokenType.MathOperator;
                    }
                    else if (lexeme == "^")
                    {
                        return TokenType.ExclusiveOrOperator;
                    }
                    else if (lexeme == "==")
                    {
                        return TokenType.EqualToOperator;
                    }
                    else if (lexeme == "=")
                    {
                        return TokenType.Assignment;
                    }
                    else if (lexeme.Contains("<"))
                    {
                        return TokenType.Clock;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (IsLexemeConstant(lexeme))
                {
                    if (seperator != ' ' && seperator != '\n' && seperator != ';' && seperator != ')' && seperator != '}' && seperator != ',')
                    {
                        if (seperator == '~')
                        {
                            ErrorLog.Add(CurrentLineNumber, $"'~' must be attached to the start of a variable, constant, parenthesis or concatenation.");
                        }
                        else if (seperator == '*')
                        {
                            ErrorLog.Add(CurrentLineNumber, $"'~' must be attached to the start of a variable, constant, or concatenation.");
                        }
                        else
                        {
                            ErrorLog.Add(CurrentLineNumber, $"Unrecognized '{lexeme}{seperator}'. Are you missing a space?");
                        }
                        return null;
                    }

                    return TokenType.Constant;
                }
                else if (FormatterRegex.IsMatch(lexeme))
                {
                    if (seperator != '{')
                    {
                        ErrorLog.Add(CurrentLineNumber, $"Invalid formatter '{lexeme}{seperator}'. Formatters must end with a '{{'.");
                        return null;
                    }

                    return TokenType.Formatter;
                }
                else if (InstantiationRegex.IsMatch(lexeme) && seperator == '(')
                {
                    return TokenType.Instantiation;
                }
                else
                {
                    if (!ErrorLog.ContainsKey(CurrentLineNumber))
                    {
                        ErrorLog.Add(CurrentLineNumber, $"Unrecognized '{lexeme}'.");
                    }
                    return null;
                }
            }
            else
            {
                if (seperator == ' ')
                {
                    return TokenType.Whitespace;
                }
                else if (seperator == '\n')
                {
                    return TokenType.Newline;
                }
                else if (seperator == ';')
                {
                    return TokenType.Semicolon;
                }
                else if (seperator == '(')
                {
                    return TokenType.OpenParenthesis;
                }
                else if (seperator == ')')
                {
                    return TokenType.CloseParenthesis;
                }
                else if (seperator == '{')
                {
                    return TokenType.OpenBrace;
                }
                else if (seperator == '}')
                {
                    return TokenType.CloseBrace;
                }
                else if (seperator == '~')
                {
                    return TokenType.NegationOperator;
                }
                else if (seperator == '*')
                {
                    return TokenType.Asterick;
                }
                else if (seperator == ',')
                {
                    return TokenType.Comma;
                }
                else if (seperator == ':')
                {
                    return TokenType.Colon;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region Token Validation

        /// <summary>
        /// Returns whether the provided token is valid from the provided statement type.
        /// </summary>
        /// <param name="token">Token to validate</param>
        /// <param name="statementType">Statement type</param>
        /// <param name="line">Line</param>
        /// <returns>Whether the provided token is valid from the provided statement type</returns>
        protected bool IsTokenValid(Token token, StatementType? statementType, string line)
        {
            if (token.Type == TokenType.Whitespace || token.Type == TokenType.Newline || token.Type == TokenType.Semicolon)
            {
                return true;
            }
            else if (token.Type == TokenType.Variable || token.Type == TokenType.Constant)
            {
                if (statementType == StatementType.Comment || statementType == StatementType.Library)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Variables and constants can't be used in Comment or Library statements.");
                    return false;
                }
            }
            else if (token.Type == TokenType.EqualToOperator || token.Type == TokenType.ExclusiveOrOperator
                || token.Type == TokenType.MathOperator || token.Type == TokenType.NegationOperator
                || token.Type == TokenType.OrOperator)
            {
                if (statementType != StatementType.Boolean && statementType != StatementType.Clock)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Operator '{token.Text}' can only be used in Boolean or Clock statements.");
                    return false;
                }
            }
            else if (token.Type == TokenType.CloseParenthesis || token.Type == TokenType.OpenParenthesis)
            {
                if (statementType != StatementType.Boolean && statementType != StatementType.Clock
                    && statementType != StatementType.Module && statementType != StatementType.Submodule)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Parentheses can't be used in Boolean, Clock, Module or Submodule statements.");
                    return false;
                }
            }
            else if (token.Type == TokenType.CloseBrace || token.Type == TokenType.OpenBrace)
            {
                if (statementType == StatementType.Comment || statementType == StatementType.Library)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Concatenations can't be used in Comment or Library statements.");
                    return false;
                }
            }
            else if (token.Type == TokenType.Assignment)
            {
                if (statementType == StatementType.Empty)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Assignment operator '{token.Text}' is missing a dependent.");
                    return false;
                }
                else if (statementType == StatementType.Boolean)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Assignment operator '{token.Text}' can only precede dependent(s) once in a Boolean statement.");
                    return false;
                }
                else if (statementType != StatementType.Display)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Assignment operators can only be used in Boolean statements.");
                    return false;
                }
            }
            else if (token.Type == TokenType.Clock)
            {
                if (statementType == StatementType.Empty)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Sequential assignment operator '{token.Text}' is missing a dependent.");
                    return false;
                }
                else if (statementType == StatementType.Clock)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Sequential assignment operator '{token.Text}' can only precede dependent(s) once in a Clock statement.");
                    return false;
                }
                else if (statementType != StatementType.Display)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Sequential assignment operators can only be used in Clock statements.");
                    return false;
                }
            }
            else if (token.Type == TokenType.Formatter)
            {
                if (statementType != StatementType.Empty && statementType != StatementType.Display)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Format specifiers can only be used in Display statements.");
                    return false;
                }
            }
            else if (token.Type == TokenType.Asterick)
            {
                if (statementType != StatementType.Empty && statementType != StatementType.Display)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"'*' can only be used in Display statements.");
                    return false;
                }
            }
            else if (token.Type == TokenType.Comment)
            {
                if (statementType != StatementType.Empty)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Comments must be their own statement.");
                    return false;
                }
            }
            else if (token.Type == TokenType.Colon)
            {
                if (statementType != StatementType.Module && statementType != StatementType.Submodule)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"':' can only be used to seperate input and output variables in a Module or Submodule statement.");
                    return false;
                }
            }
            else if (token.Type == TokenType.Comma)
            {
                if (statementType != StatementType.Module && statementType != StatementType.Submodule)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"',' can only be used to separate variables in a Module or Submodule statement.");
                    return false;
                }
            }
            else if (token.Type == TokenType.Instantiation)
            {
                if (statementType == StatementType.Display)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"All variables or constants in a Submodule statement must be in a module instantiaton.");
                    return false;
                }
                else if (statementType != StatementType.Empty)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Module instantiations must be there own statement.");
                    return false;
                }

                return ValidateInstantiation(InstantiationRegex.Match(token.Text), line);
            }
            else if (token.Type == TokenType.Declaration)
            {
                if (statementType == StatementType.Display)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"All variables or constants in a Module statement must be in a module declaration.");
                    return false;
                }
                else if (statementType != StatementType.Empty)
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Module declarations must be there own statement.");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Token Helper Functions

        /// <summary>
        /// Return whether the token is a left operand.
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Whether the token is a left operand</returns>
        private bool IsTokenLeftOperand(Token token)
        {
            return token != null && (token.Type == TokenType.Variable || token.Type == TokenType.Constant
                || token.Type == TokenType.CloseBrace || token.Type == TokenType.CloseParenthesis);
        }

        /// <summary>
        /// Returns whether the next character belongs to a variable or constant.
        /// </summary>
        /// <param name="nextChar">Next character</param>
        /// <returns>Whether the next character belongs to a variable or constant</returns>
        private bool DoesNextCharBelongToVariableOrConstant(char nextChar)
        {
            return char.IsLetterOrDigit(nextChar) || nextChar == '\'';
        }

        /// <summary>
        /// Return whether the next character is a right operand.
        /// </summary>
        /// <param name="nextChar">Next character</param>
        /// <returns>Whether the next character is a right operand</returns>
        private bool IsNextCharRightOperand(char nextChar)
        {
            return DoesNextCharBelongToVariableOrConstant(nextChar) || nextChar == '(' || nextChar == '{' || nextChar == '~';
        }

        /// <summary>
        /// Returns whether the new token is an operator.
        /// </summary>
        /// <param name="newToken">New token</param>
        /// <param name="previousToken">Previous token</param>
        /// <param name="nextChar">Next character</param>
        /// <returns>Whether the new token is an operator</returns>
        private bool IsNewTokenOperator(Token newToken, Token previousToken, char nextChar, StatementType? type)
        {
            // If new token is a boolean or math operator
            if (newToken.Type == TokenType.OrOperator || newToken.Type == TokenType.NegationOperator
                || newToken.Type == TokenType.ExclusiveOrOperator || newToken.Type == TokenType.MathOperator
                || newToken.Type == TokenType.EqualToOperator)
            {
                return true;
            }

            // If new token is whitespace or newline and seperates two operands
            if ((newToken.Type == TokenType.Whitespace || newToken.Type == TokenType.Newline)
                && (type == StatementType.Boolean || type == StatementType.Clock)
                && IsTokenLeftOperand(previousToken) && IsNextCharRightOperand(nextChar))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the next character that is not whitespace or a newline character.
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="index">Index in line</param>
        /// <returns>Next character that is not whitespace or a newline character</returns>
        private char GetNextChar(string line, int index)
        {
            // For all characters in the rest of the line
            for (int i = index; i < line.Length; i++)
            {
                // If the current character is not a whitespace or newline character
                if (line[i] != ' ' && line[i] != '\n')
                {
                    // Return current character
                    return line[i];
                }
            }
            return '\0';
        }

        #endregion

        #region Token Verification

        /// <summary>
        /// Updates the statement type from the knowledge of the new token and the previous tokens.
        /// </summary>
        /// <param name="previousTokens">Previous tokens</param>
        /// <param name="newToken">New token</param>
        /// <param name="statementType">Current statement type</param>
        /// <returns>New statement type from the knowledge of the new token and the previous tokens</returns>
        private StatementType? UpdateStatementType(List<Token> previousTokens, Token newToken, StatementType? statementType)
        {
            if (newToken.Type == TokenType.Whitespace || newToken.Type == TokenType.Newline || newToken.Type == TokenType.Semicolon)
            {
                return statementType;
            }
            else if (statementType == StatementType.Empty)
            {
                if (newToken.Type == TokenType.Variable || newToken.Type == TokenType.Constant || newToken.Type == TokenType.Asterick
                    || newToken.Type == TokenType.Formatter || newToken.Type == TokenType.OpenBrace || newToken.Type == TokenType.CloseBrace)
                {
                    return StatementType.Display;
                }
                else if (newToken.Type == TokenType.Comment)
                {
                    return StatementType.Comment;
                }
                else if (newToken.Type == TokenType.Instantiation)
                {
                    InsideModule = true;
                    return StatementType.Submodule;
                }
                else if (newToken.Type == TokenType.Declaration)
                {
                    InsideModule = true;
                    return StatementType.Module;
                }
                else if (newToken.Type == TokenType.Library)
                {
                    return StatementType.Library;
                }
                else
                {
                    return null;
                }
            }
            else if (statementType == StatementType.Display)
            {
                if (newToken.Type == TokenType.Assignment || newToken.Type == TokenType.Clock)
                {
                    int groupingCount = 0;
                    bool insideGroup = false;
                    int variableOutsideGroupCount = 0;
                    foreach (Token previousToken in previousTokens)
                    {
                        if (previousToken.Type == TokenType.Variable)
                        {
                            if (variableOutsideGroupCount == 1)
                            {
                                // Add invalid token error to error log
                                ErrorLog.Add(CurrentLineNumber, $"Multiple dependents can only be used inside a concatenation.");
                                return null;
                            }

                            if (!insideGroup)
                            {
                                if (groupingCount == 1)
                                {
                                    // Add invalid token error to error log
                                    ErrorLog.Add(CurrentLineNumber, $"Multiple dependents can only be used inside a concatenation.");
                                    return null;
                                }

                                variableOutsideGroupCount++;
                            }
                        }
                        else if (previousToken.Type == TokenType.OpenBrace)
                        {
                            if (groupingCount == 1)
                            {
                                // Add invalid token error to error log
                                ErrorLog.Add(CurrentLineNumber, $"Only one concatenation can't be used on the left side of a boolean or clock statement.");
                                return null;
                            }

                            insideGroup = true;
                            groupingCount++;
                        }
                        else if (previousToken.Type == TokenType.CloseBrace)
                        {
                            insideGroup = false;
                        }
                        else if (previousToken.Type == TokenType.Constant)
                        {
                            // Add invalid token error to error log
                            ErrorLog.Add(CurrentLineNumber, $"Constants can't be used on the left side of a boolean or clock statement.");
                            return null;
                        }
                        else if (previousToken.Type == TokenType.Asterick)
                        {
                            // Add invalid token error to error log
                            ErrorLog.Add(CurrentLineNumber, $"'*' can only be used in Variable List statements.");
                            return null;
                        }
                    }

                    // Add new operator list
                    Operators.Add(new List<Token>());
                    // Add empty exclusive operator
                    ExclusiveOperators.Add(null);
                    return newToken.Type == TokenType.Assignment ? StatementType.Boolean : StatementType.Clock;
                }
                else if (newToken.Type == TokenType.Variable || newToken.Type == TokenType.Constant
                    || newToken.Type == TokenType.Asterick || newToken.Type == TokenType.OpenBrace
                    || newToken.Type == TokenType.CloseBrace)
                {
                    return statementType;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return statementType;
            }
        }

        /// <summary>
        /// Verifies whether the new token in the current execution follows valid syntax.
        /// </summary>
        /// <param name="previousTokens">Tokens before the new token</param>
        /// <param name="newToken">New token</param>
        /// <param name="nextChar">Next non-whitespace character</param>
        /// <param name="statementType">Current statement type</param>
        /// <returns>Whether the new token follows valid syntax.</returns>
        private bool VerifySyntax(List<Token> previousTokens, Token newToken, char nextChar, StatementType? statementType)
        {
            if (newToken.Type == TokenType.Semicolon)
            {
                return true;
            }

            // If new token is inside a concat, check the new token is allowed to be inside a concat
            if (InsideConcat && newToken.Type != TokenType.Whitespace && newToken.Type != TokenType.Newline
                && newToken.Type != TokenType.Variable && newToken.Type != TokenType.Constant && newToken.Type != TokenType.CloseBrace)
            {
                if (InsideFormatter)
                {
                    ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' can't be used inside a format specifier.");
                }
                else
                {
                    ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' can't be used inside a concatenation.");
                }
                
                return false;
            }

            // Init last token
            Token lastToken = null;
            // For all previous tokens in reverse
            foreach (Token previousToken in previousTokens.ToArray().Reverse())
            {
                // If token isn't a whitespace or newline token
                if (previousToken.Type != TokenType.Whitespace && previousToken.Type != TokenType.Newline)
                {
                    // Set last token to this token
                    lastToken = previousToken;
                    break;
                }
            }

            // Get whether the new token is an operator
            bool isNewTokenOperator = IsNewTokenOperator(newToken, lastToken, nextChar, statementType);

            // If the new token is a whitespace or newline token and isn't an operator
            if ((newToken.Type == TokenType.Whitespace || newToken.Type == TokenType.Newline) && !isNewTokenOperator)
            {
                return true;
            }

            if (newToken.Type == TokenType.Variable || newToken.Type == TokenType.Constant)
            {
                if ((statementType == StatementType.Module || statementType == StatementType.Submodule)
                    && !InsideModule)
                {
                    ErrorLog.Add(CurrentLineNumber, $"Variables and constants in Module or Submodule statements must be inside an instantiation or declaration.");
                    return false;
                }

                if (newToken.Type == TokenType.Constant)
                {
                    bool containsBitCount = char.IsDigit(newToken.Text[0]) && newToken.Text.Contains('\'');
                    if (InsideConcat && !containsBitCount)
                    {
                        ErrorLog.Add(CurrentLineNumber, $"Constants in concatenations must specify a bit count.");
                        return false;
                    }
                    else if (InsideModule)
                    {
                        if (!containsBitCount)
                        {
                            ErrorLog.Add(CurrentLineNumber, $"Constants in module declarations or instantiations must specify a bit count.");
                            return false;
                        }
                        else if (previousTokens.Any(t => t.Type == TokenType.Colon))
                        {
                            ErrorLog.Add(CurrentLineNumber, $"Constants can't be used as outputs in module declarations or instantiations.");
                            return false;
                        }
                    }
                }
            }
            else if (isNewTokenOperator)
            {
                if (newToken.Type == TokenType.NegationOperator)
                {
                    if (!DoesNextCharBelongToVariableOrConstant(nextChar) && nextChar != '(' && nextChar != '{' && nextChar != '~')
                    {
                        ErrorLog.Add(CurrentLineNumber, $"'~' must be attached to the start of a variable, constant, parenthesis or concatenation.");
                        return false;
                    }
                }
                else
                {
                    if (!IsTokenLeftOperand(lastToken))
                    {
                        ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' is missing a left operand.");
                        return false;
                    }

                    if (!IsNextCharRightOperand(nextChar))
                    {
                        ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' is missing a right operand.");
                        return false;
                    }
                }

                // Get current exclusive operator
                Token currentExclusiveOperator = ExclusiveOperators[ExclusiveOperators.Count - 1];
                if (ExclusiveOperatorsList.Contains(newToken.Text) && currentExclusiveOperator == null)
                {
                    if (Operators[Operators.Count - 1].Any(o => o.Type != newToken.Type))
                    {
                        ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' operator must be the only operator in its parentheses level.");
                        return false;
                    }

                    // Save exclusive operator
                    ExclusiveOperators[ExclusiveOperators.Count - 1] = newToken;

                    if (!IsMathExpression && newToken.Type == TokenType.MathOperator)
                    {
                        foreach (List<Token> tokenOperators in Operators)
                        {
                            if (tokenOperators.Count > 0)
                            {
                                ErrorLog.Add(CurrentLineNumber, $"Math operators (+ and -) cannot be used with boolean operators in a boolean or clock statement.");
                                return false;
                            }
                        }

                        IsMathExpression = true;
                    }
                }
                else if (currentExclusiveOperator != null)
                {
                    if (currentExclusiveOperator.Type != newToken.Type)
                    {
                        ErrorLog.Add(CurrentLineNumber, $"'{currentExclusiveOperator.Text}' operator must be the only operator in its parentheses level.");
                        return false;
                    }
                }

                if (!Operators[Operators.Count - 1].Any(o => o.Type == newToken.Type))
                {
                    Operators[Operators.Count - 1].Add(newToken);
                }
            }
            else if (newToken.Type == TokenType.OpenParenthesis || newToken.Type == TokenType.OpenBrace)
            {
                if (Groupings.Count > 0 && Groupings.Peek() == '{')
                {
                    if (newToken.Type == TokenType.OpenBrace)
                    {
                        // Concatenation inside concatenation error
                        ErrorLog.Add(CurrentLineNumber, $"Concatenations can't be used inside other concatenations.");
                    }
                    else
                    {
                        // Parenthesis inside concatenation error
                        ErrorLog.Add(CurrentLineNumber, $"Parenthesis can't be used inside concatenations.");
                    }
                    return false;
                }

                Groupings.Push(newToken.Text[0]);

                if (newToken.Type == TokenType.OpenParenthesis)
                {
                    InsideModule = lastToken != null && (lastToken.Type == TokenType.Declaration || lastToken.Type == TokenType.Instantiation);
                    Operators.Add(new List<Token>());
                    ExclusiveOperators.Add(null);
                }
                else
                {
                    InsideConcat = true;
                    InsideFormatter = lastToken != null && lastToken.Type == TokenType.Formatter;
                }
            }
            else if (newToken.Type == TokenType.CloseParenthesis || newToken.Type == TokenType.CloseBrace)
            {
                char top = Groupings.Count > 0 ? Groupings.Pop() : '\0';
                if ((newToken.Type == TokenType.CloseParenthesis && top != '(') || (newToken.Type == TokenType.CloseBrace && top != '{'))
                {
                    if (top == '\0')
                    {
                        // New grouping error
                        ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' doesn't have a matching opening.");
                    }
                    else
                    {
                        // Unmatched grouping error
                        ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' cannot be matched. '{top}' must be closed first.");
                    }
                    return false;
                }

                if (newToken.Type == TokenType.CloseParenthesis)
                {
                    if (lastToken != null && lastToken.Type == TokenType.OpenParenthesis)
                    {
                        ErrorLog.Add(CurrentLineNumber, $"() can't be empty.");
                        return false;
                    }

                    InsideModule = false;
                    Operators.RemoveAt(Operators.Count - 1);
                    ExclusiveOperators.RemoveAt(ExclusiveOperators.Count - 1);
                }
                else
                {
                    if (lastToken != null && lastToken.Type == TokenType.OpenBrace)
                    {
                        ErrorLog.Add(CurrentLineNumber, $"{{}} can't be empty.");
                        return false;
                    }

                    InsideConcat = false;
                    InsideFormatter = false;
                }
            }
            else if (newToken.Type == TokenType.Asterick)
            {
                if (!char.IsLetter(nextChar) && nextChar != '{' && nextChar != '*')
                {
                    ErrorLog.Add(CurrentLineNumber, $"'*' must be attached to the start of a variable, constant, or concatenation.");
                    return false;
                }
            }
            else if (newToken.Type == TokenType.Assignment)
            {
                if (nextChar == '\0' || nextChar == ';')
                {
                    ErrorLog.Add(CurrentLineNumber, $"Assignment operator '{newToken.Text}' must have a right-hand expression.");
                    return false;
                }
            }
            else if (newToken.Type == TokenType.Clock)
            {
                if (nextChar == '\0' || nextChar == ';')
                {
                    ErrorLog.Add(CurrentLineNumber, $"Sequential assignment operator '{newToken.Text}' must have a right-hand expression.");
                    return false;
                }
            }
            else if (newToken.Type == TokenType.Formatter)
            {
                return true;
            }
            else if (newToken.Type == TokenType.Comment)
            {
                if (nextChar != '\0')
                {
                    // Add invalid token error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Comments must be their own statement.");
                    return false;
                }
            }
            else if (newToken.Type == TokenType.Comma)
            {
                // Make sure comma is inside a module, doesn't start a module, doesn't end a module
                // and seperates variables, constants and concatenations.
                if (!InsideModule || (lastToken != null && lastToken.Type != TokenType.Variable
                    && lastToken.Type != TokenType.Constant && lastToken.Type != TokenType.CloseBrace))
                {
                    ErrorLog.Add(CurrentLineNumber, $"',' can only be used to separate variables, constants or concatenations in a module or submodule statement.");
                    return false;
                }
            }
            else if (newToken.Type == TokenType.Colon)
            {
                // Make sure colon is inside a module, doesn't start a module, doesn't end a module
                // and seperates variables, constants and concatenations.
                if (!InsideModule || (!DoesNextCharBelongToVariableOrConstant(nextChar) && nextChar != '{')
                    || (lastToken != null && lastToken.Type != TokenType.Variable
                    && lastToken.Type != TokenType.Constant && lastToken.Type != TokenType.CloseBrace))
                {
                    ErrorLog.Add(CurrentLineNumber, $"':' can only be used to seperate input and output variables in a module or submodule statement.");
                    return false;
                }

                if (previousTokens.Any(t => t.Type == TokenType.Colon))
                {
                    ErrorLog.Add(CurrentLineNumber, $"':' can only be used once in a module or submodule statement.");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Token Validation

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
                ErrorLog.Add(CurrentLineNumber, $"You cannot instantiate from the current design.");
                return false;
            }

            if (Instantiations.ContainsKey(instantiationName))
            {
                ErrorLog.Add(CurrentLineNumber, $"Instantiation name '{instantiationName}' is already being used.");
                return false;
            }
            else
            {
                try
                {
                    if (!Subdesigns.ContainsKey(designName))
                    {
                        Design subDesign = null;
                        string[] files = Directory.GetFiles(Design.FileSource.DirectoryName, String.Concat(designName, ".vbi"));
                        if (files.Length > 0)
                        {
                            // Check for module Declaration
                            subDesign = DesignHasModuleDeclaration(files[0]);
                        }

                        if (subDesign == null)
                        {
                            for (int i = 0; i < Libraries.Count; i++)
                            {
                                files = Directory.GetFiles(Libraries[i], String.Concat(designName, ".vbi"));
                                if (files.Length > 0)
                                {
                                    // Check for module Declaration
                                    subDesign = DesignHasModuleDeclaration(files[0]);
                                    if (subDesign != null)
                                    {
                                        break;
                                    }
                                }
                            }

                            // If file is not found
                            if (subDesign == null)
                            {
                                // Add file not found to error log
                                ErrorLog.Add(CurrentLineNumber, $"Unable to find a design named '{designName}' with a module declaration.");
                                return false;
                            }
                        }

                        Subdesigns.Add(designName, subDesign);
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
                    ErrorLog.Add(CurrentLineNumber, $"Error locating '{designName}'.");
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns the design if it has a module declaration.
        /// </summary>
        /// <param name="designPath">Path to design</param>
        /// <returns>Design if it has a module declaration.</returns>
        private Design DesignHasModuleDeclaration(string designPath)
        {
            Design newDesign = new Design(designPath);
            return newDesign.ModuleDeclaration != null ? newDesign : null;
        }

        #endregion

        /// <summary>
        /// Returns the statement type of the provided line.
        /// </summary>
        /// <param name="line">Line</param>
        /// <returns>Statement type of the provided line</returns>
        protected StatementType? GetStatementType(string line)
        {
            // If line is a comment statement
            if (Parser.CommentStmtRegex.IsMatch(line))
            {
                // Return comment statement type
                return StatementType.Comment;
            }
            // If line is a library statement
            else if (Parser.LibraryStmtRegex.IsMatch(line))
            {
                // Return library statement type
                return StatementType.Library;
            }

            // Statement type to return
            StatementType? statementType = StatementType.Empty;
            // Create tokens list
            List<Token> tokens = new List<Token>();
            // Create string builder for current lexeme
            StringBuilder lexeme = new StringBuilder();
            // Create groupings stack
            Stack<char> groupings = new Stack<char>();
            // Save current line number
            int lineNumber = CurrentLineNumber;

            // Reset all instance variables
            InsideConcat = false;
            InsideFormatter = false;
            InsideModule = false;
            Groupings = new Stack<char>();
            IsMathExpression = false;
            ExclusiveOperators = new List<Token>();
            Operators = new List<List<Token>>();

            for (int i = 0; i < line.Length; i++)
            {
                // Get current character
                char c = line[i];
                // Get character as a string
                string newChar = c.ToString();
                // Get current lexme
                string currentLexeme = lexeme.ToString();

                if (statementType == StatementType.Comment || statementType == StatementType.Library)
                {
                    lexeme.Append(c);
                }
                else if (c == '"')
                {
                    // Make sure current lexeme is empty, + or -
                    if (!(currentLexeme == "+" || currentLexeme == "-" || currentLexeme == ""))
                    {
                        ErrorLog.Add(CurrentLineNumber, $"Invalid '\"'.");
                        return null;
                    }

                    // Make sure no other tokens exist
                    if (tokens.Any(token => token.Text != " "))
                    {
                        ErrorLog.Add(CurrentLineNumber, $"Invalid '\"'.");
                        return null;
                    }

                    statementType = StatementType.Comment;
                    lexeme.Append(c);
                }
                else if (currentLexeme == "#library")
                {
                    statementType = StatementType.Library;
                }
                // If the character is an invalid character
                else if (InvalidRegex.IsMatch(newChar))
                {
                    // Add invalid character error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Unrecognized character '{c}'.");
                    // Return null
                    return null;
                }
                // If the character is a seperator character
                else if (SeperatorsList.Contains(c))
                {
                    TokenType? tokenType;
                    Token newToken;
                    char nextChar = GetNextChar(line, i + 1);

                    // If current lexeme contains characters
                    if (currentLexeme.Length > 0)
                    {
                        // Get token type of current lexeme
                        tokenType = GetTokenType(currentLexeme, c);
                        // If token type is null
                        if (tokenType == null)
                        {
                            // Return null for statement type
                            return null;
                        }
                        // Create new token from current lexeme and its type
                        newToken = new Token(currentLexeme, (TokenType)tokenType);
                        // If new token isn't valid
                        if (!IsTokenValid(newToken, statementType, line))
                        {
                            // Return null for statement type
                            return null;
                        }

                        // Update statement type with the new token
                        statementType = UpdateStatementType(tokens, newToken, statementType);
                        // If new statement type is null
                        if (statementType == null)
                        {
                            // Return for error
                            return null;
                        }
                        // If new token doesn't follow syntax
                        if (!VerifySyntax(tokens, newToken, nextChar, statementType))
                        {
                            // Return for error
                            return null;
                        }

                        // Add new token to tokens list
                        tokens.Add(newToken);
                        // Clear current lexeme
                        lexeme = lexeme.Clear();
                    }

                    // Get token type of seperator char
                    tokenType = GetTokenType("", c);
                    // If token type is null
                    if (tokenType == null)
                    {
                        // Return null for statement type
                        return null;
                    }

                    // Create new token from seperator and its type
                    newToken = new Token(newChar, (TokenType)tokenType);
                    // If new token isn't valid
                    if (!IsTokenValid(newToken, statementType, line))
                    {
                        // Return null for statement type
                        return null;
                    }
                    // Update statement type with the new token
                    statementType = UpdateStatementType(tokens, newToken, statementType);
                    // If new statement type is null
                    if (statementType == null)
                    {
                        // Return for error
                        return null;
                    }
                    // If new token doesn't follow syntax
                    if (!VerifySyntax(tokens, newToken, nextChar, statementType))
                    {
                        // Return for error
                        return null;
                    }

                    // If seperator token is a new line token
                    if (tokenType == TokenType.Newline)
                    {
                        CurrentLineNumber++;
                    }

                    // Add new token to tokens list
                    tokens.Add(newToken);
                }
                // If the character is not a seperator character
                else
                {
                    // Append new character to the current lexeme
                    lexeme.Append(c);
                }
            }

            // Check for unmatched groupings
            if (groupings.Count > 0)
            {
                // Unmatched grouping error
                ErrorLog.Add(CurrentLineNumber, $"'{groupings.Peek()}' was not matched.");
                return null;
            }

            // Return statement type
            return statementType;
        }

        #region Expansion Methods

        /// <summary>
        /// Expands a vector into its components.
        /// </summary>
        /// <param name="vector">Vector to expand</param>
        /// <returns>List of vector components</returns>
        protected List<string> ExpandVector(Match vector)
        {
            // Create expansion return list
            List<string> expansion = new List<string>();

            // Get vector name, bounds and step
            string name = vector.Value.Contains("*")
                ? string.Concat(vector.Value[0], vector.Groups["Name"].Value)
                : vector.Groups["Name"].Value;
            int leftBound = Convert.ToInt32(vector.Groups["LeftBound"].Value);
            int rightBound = Convert.ToInt32(vector.Groups["RightBound"].Value);

            // If left bound is least significant bit
            if (leftBound < rightBound)
            {
                // Get vector step
                int step = string.IsNullOrEmpty(vector.Groups["Step"].Value) ? 1 : Convert.ToInt32(vector.Groups["Step"].Value);

                // For each bit in the step
                for (int i = leftBound; i <= rightBound; i += step)
                {
                    // Add bit to expansion
                    expansion.Add(string.Concat(name, i));
                }
            }
            // If right bound is least significant bit
            else
            {
                // Get vector step
                int step = string.IsNullOrEmpty(vector.Groups["Step"].Value) ? -1 : -Convert.ToInt32(vector.Groups["Step"].Value);

                // For each bit in the step
                for (int i = leftBound; i >= rightBound; i += step)
                {
                    // Add bit to expansion
                    expansion.Add(string.Concat(name, i));
                }
            }

            // If expansion hasn't been done before
            if (!ExpansionMemo.ContainsKey(vector.Value))
            {
                // Save expansion value in expansion memo
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
            // Create expansion return list
            List<string> expansion = new List<string>();
            // Init output binary string
            string outputBinary;
            // Init char bits
            char[] charBits;

            // If constant format is hex
            if (constant.Groups["Format"].Value == "h" || constant.Groups["Format"].Value == "H")
            {
                // Get output binary in hex format
                outputBinary = Convert.ToString(Convert.ToInt32(constant.Groups["Value"].Value, 16), 2);
                // Convert binary to char bits
                charBits = outputBinary.ToCharArray();
            }
            // If constant format is decimal
            else if (constant.Groups["Format"].Value == "d" || constant.Groups["Format"].Value == "D")
            {
                // Get output binary in decimal format
                outputBinary = Convert.ToString(Convert.ToInt32(constant.Groups["Value"].Value, 10), 2);
                // Convert binary to char bits
                charBits = outputBinary.ToCharArray();
            }
            // If constant format is binary
            else if (constant.Groups["Format"].Value == "b" || constant.Groups["Format"].Value == "B")
            {
                // Convert binary to char bits
                charBits = constant.Groups["Value"].Value.ToCharArray();
            }
            // If no constant format is specified
            else
            {
                // Get output binary in decimal format
                outputBinary = Convert.ToString(Convert.ToInt32(constant.Groups["Value"].Value, 10), 2);
                // Convert binary to char bits
                charBits = outputBinary.ToCharArray();
            }

            // Get binary bits
            int[] bits = Array.ConvertAll(charBits, bit => (int)char.GetNumericValue(bit));
            // Get bit count
            int bitCount = String.IsNullOrEmpty(constant.Groups["BitCount"].Value)
                ? bits.Length
                : Convert.ToInt32(constant.Groups["BitCount"].Value);

            // If padding is necessary
            if (bitCount > bits.Length)
            {
                // Get padding count
                int padding = bitCount - bits.Length;
                // For each padding
                for (int i = 0; i < padding; i++)
                {
                    // Add padding of 0
                    expansion.Add("0");
                }
                // Remove padding count from bit count
                bitCount -= padding;
            }

            // For each specified bit
            for (int i = bits.Length - bitCount; i < bits.Length; i++)
            {
                // Add bit to expansion
                expansion.Add(bits[i].ToString());
            }

            // If expansion hasn't been done before
            if (!ExpansionMemo.ContainsKey(constant.Value))
            {
                // Save expansion value in expansion memo
                ExpansionMemo.Add(constant.Value, expansion);
            }

            return expansion;
        }

        #endregion
    }
}
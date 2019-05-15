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
        #region Utility Enums and Classes

        public enum StatementType
        {
            Empty,
            Comment,
            VariableDisplay,
            Assignment,
            ClockAssignment,
            Header,
            Instantiation,
            Library
        }

        /// <summary>
        /// Token type proposed by the lexer.
        /// </summary>
        private enum TokenType
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
            Space,
            NewLine,
            Semicolon,
            Colon,
            Comma,
            OpenParenthesis,
            CloseParenthesis,
            OpenConcatenation,
            CloseConcatenation
        }

        private class Token
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

        /// <summary>
        /// Class for parentheses error detection
        /// </summary>
        private class ParenthesesLevel
        {
            /// <summary>
            /// List of operators in this parenthesis level.
            /// </summary>
            public List<Token> Operators { get; private set; }

            /// <summary>
            /// Exclusive operator of this parenthesis level. (if any)
            /// </summary>
            public Token ExclusiveOperator { get; private set; }

            /// <summary>
            /// Constructs a parentheses level.
            /// </summary>
            public ParenthesesLevel()
            {
                // Start operators list
                Operators = new List<Token>();
                // Start exclusive operator with none
                ExclusiveOperator = null;
            }

            /// <summary>
            /// Attempts to add the new operator to the list of operators. Returns whether the operation was successful.
            /// </summary>
            /// <param name="newOperator">New operator to add</param>
            /// <returns>Whether the operation was successfu0.l</returns>
            public bool TryAddOperator(Token newOperator)
            {
                // If there is an exclusive operator
                if (ExclusiveOperator != null)
                {
                    // Return whether the operators are the same
                    return ExclusiveOperator.Type == newOperator.Type;
                }
                // If there isn't an exclusive operator
                else
                {
                    // If the new operator is an exclusive operator and there were past non-exclusive opeators
                    if (newOperator.Type == TokenType.EqualToOperator || newOperator.Type == TokenType.ExclusiveOrOperator)
                    {
                        // If there were previous operators
                        if (Operators.Count > 0)
                        {
                            // Return failure
                            return false;
                        }
                        // If there wasn't any previous operators
                        else
                        {
                            // Set exclusive operator to the new operator
                            ExclusiveOperator = newOperator;
                        }
                    }

                    // If operators list doesn't contain the new operator
                    if (!Operators.Contains(newOperator))
                    {
                        // Add operator to operator list
                        Operators.Add(newOperator);
                    }

                    // Return success
                    return true;
                }
            }
        }

        #endregion

        #region Lexer Patterns and Regular Expressions

        /// <summary>
        /// Pattern for identifying invalid characters.
        /// </summary>
        private static readonly string InvalidPattern = @"[^\sa-zA-Z0-9[\].(){}<=@~*|^+;%':,-]";

        /// <summary>
        /// Pattern for identifying names.
        /// </summary>
        public static readonly string ScalarPattern = @"(?<Name>[a-zA-Z][[a-zA-Z0-9]*)";

        /// <summary>
        /// Pattern for identfying indexes.
        /// </summary>
        private static readonly string IndexPattern = @"(\[((?<LeftBound>\d+)(\.(?<Step>\d+)?\.(?<RightBound>\d+))?)?\])";

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
        private static readonly string ModuleComponentPattern = $@"(({AnyTypePattern}|{VariableListPattern})(,\s*({AnyTypePattern}|{VariableListPattern}))*)";

        /// <summary>
        /// Pattern for identifying modules.
        /// </summary>
        public static readonly string ModulePattern = $@"(?<Components>(?<Inputs>{ModuleComponentPattern})\s*:\s*(?<Outputs>{ModuleComponentPattern}))";

        /// <summary>
        /// Regex for identifying invalid characters.
        /// </summary>
        private static Regex InvalidRegex = new Regex(InvalidPattern, RegexOptions.Compiled);

        /// <summary>
        /// Regex for indentifying operators
        /// </summary>
        private static Regex OperatorRegex = new Regex(OperatorPattern, RegexOptions.Compiled);

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
        /// Regex for identifiyng appended operator lexemes.
        /// </summary>
        private static Regex MulticharacterOperatorLexemeRegex = new Regex(@"^(\~+|\*+|=|(==)|(<=))$", RegexOptions.Compiled);

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
        private static readonly IList<char> SeperatorsList = new ReadOnlyCollection<char>(
            new List<char> { ' ', '\n', ';', '(', ')', '{', '}', ',', ':', '^', '|', '+', '-' });

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
        /// Indicates whether math operators are valid.
        /// </summary>
        private bool MathOperatorsValid;

        /// <summary>
        /// Operators lists for the current execution.
        /// </summary>
        //private List<List<Token>> Operators;

        /// <summary>
        /// List of parentheses levels.
        /// </summary>
        private List<ParenthesesLevel> Parentheses;

        private List<TokenType> Operations;

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
            // Save design to perform lexical analysis on
            Design = design;
            // Init error log dictionary
            ErrorLog = new Dictionary<int, string>();
            // Init libraries list
            Libraries = new List<string>();
            // Init subdesigns dictionary
            Subdesigns = new Dictionary<string, Design>();
            // Init instantiations dictionary
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
                // Get scalar's name
                string name = scalarMatch.Groups["Name"].Value;
                // Get scalar's bit as a string
                string bitString = string.Concat(name.ToArray().Reverse().TakeWhile(char.IsNumber).Reverse());
                // Get scalar's bit as an integer
                int bit = string.IsNullOrEmpty(bitString) ? -1 : Convert.ToInt32(bitString);
                // If scalar contains a bit
                if (bit != -1)
                {
                    // Trim bit from scalar's name
                    name = name.Substring(0, name.Length - bitString.Length);

                    // If scalar bit is larger than 31
                    if (bit > 31)
                    {
                        // Add scalar bit count error to error log
                        ErrorLog.Add(CurrentLineNumber, $"Bit of '{lexeme}' must be between 0 and 31.");
                        // Return lexeme isn't a valid scalar
                        return false;
                    }

                    // Add/update vector namespace
                    Design.Database.UpdateNamespace(name, bit);
                }
            }

            // Return whether the lexeme was matched as a scalar
            return scalarMatch.Success;
        }

        /// <summary>
        /// Returns whether the provided lexeme is a vector.
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
                // Get vector's name
                string name = vectorMatch.Groups["Name"].Value;
                // If vector name ends in a number
                if (char.IsDigit(name[name.Length - 1]))
                {
                    // Add vector name error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Vector name '{name}' cannot end in a number.");
                    // Return lexeme isn't a valid vector
                    return false;
                }

                // Get vector's left bound
                int leftBound = string.IsNullOrEmpty(vectorMatch.Groups["LeftBound"].Value) ? -1 : Convert.ToInt32(vectorMatch.Groups["LeftBound"].Value);
                // Get vector's step
                int step = string.IsNullOrEmpty(vectorMatch.Groups["Step"].Value) ? -1 : Convert.ToInt32(vectorMatch.Groups["Step"].Value);
                // Get vector's right bound
                int rightBound = string.IsNullOrEmpty(vectorMatch.Groups["RightBound"].Value) ? -1 : Convert.ToInt32(vectorMatch.Groups["RightBound"].Value);
                // If left bound or right bound is greater than 31
                if (leftBound > 31 || rightBound > 31)
                {
                    // Add vector bounds error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Vector bounds of '{lexeme}' must be between 0 and 31.");
                    // Return lexeme isn't a valid vector
                    return false;
                }
                // If step is not between 1 and 31
                else if (step == 0 || step > 31)
                {
                    // Add vector step error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Vector step of '{lexeme}' must be between 1 and 31.");
                    // Return lexeme isn't a valid vector
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
                        // Add/update vector namespace
                        Design.Database.UpdateNamespace(name, i);
                    }
                }
            }

            // Return whether the lexeme was matched as a vector
            return vectorMatch.Success;
        }

        /// <summary>
        /// Returns whether the provided lexeme is a constant.
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
                // Get constant's bit count
                int bitCount = string.IsNullOrEmpty(constantMatch.Groups["BitCount"].Value) ? -1 : Convert.ToInt32(constantMatch.Groups["BitCount"].Value);
                // If bit count is greater than 32
                if (bitCount > 32)
                {
                    // Add constant bit count error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Specified bit count of '{lexeme}' exceeds the 32 bit limit.");
                    // Return lexeme isn't a valid constant
                    return false;
                }

                // Get constant's format
                char format = string.IsNullOrEmpty(constantMatch.Groups["Format"].Value) ? 'D' : char.ToUpper(constantMatch.Groups["Format"].Value[0]);
                // Get constant's value
                string value = constantMatch.Groups["Value"].Value;
                // If format is hex or binary and exceeds the 32 bit value limit
                if ((format == 'H' && value.Length > 8) || (format == 'B' && value.Length > 32))
                {
                    // Add constant value error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Value of '{lexeme}' exceeds the 32 bit limit.");
                    // Return lexeme isn't a valid constant
                    return false;
                }
                // If format is binary or unsigned and exceeds the 32 bit value limit
                else if ((format == 'D' || format == 'U') && Convert.ToDouble(value) > 4294967295)
                {
                    // Add constant value error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Value of '{lexeme}' exceeds the 32 bit limit.");
                    // Return lexeme isn't a valid constant
                    return false;
                }
            }

            // Return whether the lexeme was matched as a constant
            return constantMatch.Success;
        }

        /// <summary>
        /// Returns the token type of the provided lexeme.
        /// </summary>
        /// <param name="lexeme">Current lexeme</param>
        /// <param name="nextLexeme">Next lexeme</param>
        /// <returns>Token type of the provided lexeme</returns>
        private TokenType? GetTokenType(string currentLexeme, string nextLexeme)
        {
            // Get first character in current lexeme
            char firstCharInCurrentLexeme = currentLexeme[0];
            // Get first character in next lexeme
            char firstCharInNextLexeme = nextLexeme[0];

            // If current lexeme is a space
            if (firstCharInCurrentLexeme == ' ')
            {
                // Return token type space
                return TokenType.Space;
            }
            // If current lexeme is a new line character
            else if (firstCharInCurrentLexeme == '\n')
            {
                // Return token type new line
                return TokenType.NewLine;
            }
            // If current lexeme is a semicolon
            else if (firstCharInCurrentLexeme == ';')
            {
                // Return token type semicolon
                return TokenType.Semicolon;
            }
            // If current lexeme is an open parenthesis
            else if (firstCharInCurrentLexeme == '(')
            {
                // Return token type open parenthesis
                return TokenType.OpenParenthesis;
            }
            // If current lexeme is a close parenthesis
            else if (firstCharInCurrentLexeme == ')')
            {
                // Return token type close parenthesis
                return TokenType.CloseParenthesis;
            }
            // If current lexeme is an open concatenation
            else if (firstCharInCurrentLexeme == '{')
            {
                // Return token type open concatenation
                return TokenType.OpenConcatenation;
            }
            // If current lexeme is a close concatenation
            else if (firstCharInCurrentLexeme == '}')
            {
                // Return token type close concatenation
                return TokenType.CloseConcatenation;
            }
            // If current lexeme is an operator
            else if (OperatorRegex.IsMatch(currentLexeme))
            {
                // If current lexeme is an or operator
                if (firstCharInCurrentLexeme == '|')
                {
                    // Return token type or operator
                    return TokenType.OrOperator;
                }
                // If current lexeme is an exclusive or operator
                else if (firstCharInCurrentLexeme == '^')
                {
                    // Return token type exclusive or operator
                    return TokenType.ExclusiveOrOperator;
                }
                // If current lexeme is a math operator
                else if (firstCharInCurrentLexeme == '+' || firstCharInCurrentLexeme == '-')
                {
                    // Return token type math operator
                    return TokenType.MathOperator;
                }
                // If current lexeme is an equal to sign
                else if (currentLexeme == "==")
                {
                    // Return token type equal to operator
                    return TokenType.EqualToOperator;
                }
                // If current lexeme is an equal sign
                else if (firstCharInCurrentLexeme == '=')
                {
                    // Return token type assignemnt
                    return TokenType.Assignment;
                }
                // If current lexeme is a negation operator
                else if (firstCharInCurrentLexeme == '~')
                {
                    // If next lexeme isn"t a variable, opepn parenthesis or open concatenation
                    if (!char.IsLetter(firstCharInNextLexeme) && firstCharInNextLexeme != '(' && firstCharInNextLexeme != '{')
                    {
                        // Add unattached negation opreator error to error log
                        ErrorLog.Add(CurrentLineNumber, $"'~' must be attached to the start of a variable, parenthesis or concatenation.");
                        // Return null for error token
                        return null;
                    }

                    // Return token type negation opereator
                    return TokenType.NegationOperator;
                }
                // If current lexeme is an asterick
                else if (firstCharInCurrentLexeme == '*')
                {
                    // If next lexeme isn"t a variable or open concatenation
                    if (!char.IsLetter(firstCharInNextLexeme) && firstCharInNextLexeme != '{')
                    {
                        // Add unattached asterick error to error log
                        ErrorLog.Add(CurrentLineNumber, $"'*' must be attached to the start of a variable or concatenation.");
                        // Return null for error token
                        return null;
                    }

                    // Return token type asterick
                    return TokenType.Asterick;
                }
                // If current lexeme is a clock symbol
                else
                {
                    // Return token type clock
                    return TokenType.Clock;
                }
            }
            // If current lexeme is a comma
            else if (firstCharInCurrentLexeme == ',')
            {
                // Return token type comma
                return TokenType.Comma;
            }
            // If current lexeme is a colon
            else if (firstCharInCurrentLexeme == ':')
            {
                // Return token type colon
                return TokenType.Colon;
            }
            // If current lexeme is the design name and next lexeme is an open parenthesis
            else if (currentLexeme == Design.FileName && firstCharInNextLexeme == '(')
            {
                // Return token type declaration
                return TokenType.Declaration;
            }
            // If current lexeme is a scalar or vector
            else if (IsLexemeScalar(currentLexeme) || IsLexemeVector(currentLexeme))
            {
                // Return token type variable
                return TokenType.Variable;
            }
            // If current lexeme is a constant
            else if (IsLexemeConstant(currentLexeme))
            {
                /*
                if (nextLexeme != " " && nextLexeme != "\n" && nextLexeme != ";" && nextLexeme != ")" && nextLexeme != "}" && nextLexeme != ",")
                {
                    ErrorLog.Add(CurrentLineNumber, $"Unrecognized "{currentLexeme}{nextLexeme}". Are you missing a space?");
                    // Return null for error token
                    return null;
                }
                */

                // Return token type constant
                return TokenType.Constant;
            }
            // If current lexeme is a formatter
            else if (FormatterRegex.IsMatch(currentLexeme))
            {
                // If next lexeme is not an open concatenation
                if (firstCharInNextLexeme != '{')
                {
                    // Add invalid formatter error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Invalid formatter '{currentLexeme}{nextLexeme}'. Formatters must end with a '{{'.");
                    // Return null for error token
                    return null;
                }

                // Return token type formatter
                return TokenType.Formatter;
            }
            // If current lexeme is an instantiation and next lexeme is an open parenthesis
            else if (InstantiationRegex.IsMatch(currentLexeme) && firstCharInNextLexeme == '(')
            {
                // Return token type instantiation
                return TokenType.Instantiation;
            }
            // If current lexeme is none of the above
            else
            {
                // If this line doesn"t already contain an error
                if (!ErrorLog.ContainsKey(CurrentLineNumber))
                {
                    // Add unrecognized lexeme error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Unrecognized '{currentLexeme}'.");
                }
                // Return null for error token
                return null;
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
        private bool IsTokenValid(Token token, StatementType? statementType, string line)
        {
            // If token type is space, newline or semicolon
            if (token.Type == TokenType.Space || token.Type == TokenType.NewLine || token.Type == TokenType.Semicolon)
            {
                // Return valid
                return true;
            }
            // If token type is an operator
            else if (token.Type == TokenType.EqualToOperator || token.Type == TokenType.ExclusiveOrOperator
                || token.Type == TokenType.MathOperator || token.Type == TokenType.NegationOperator
                || token.Type == TokenType.OrOperator)
            {
                // If statement type is not assignment
                if (statementType != StatementType.Assignment && statementType != StatementType.ClockAssignment)
                {
                    // Add invalid operator error to error log
                    ErrorLog.Add(CurrentLineNumber, $"'{token.Text}' can only be used in an assignment statement.");
                    // Return invalid
                    return false;
                }
            }
            // If token type is open or close parenthesis
            else if (token.Type == TokenType.OpenParenthesis || token.Type == TokenType.CloseParenthesis)
            {
                // If statement type is variable display
                if (statementType == StatementType.VariableDisplay)
                {
                    // Add invalid parenthesis error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Parentheses can't be used in variable display statements.");
                    // Return invalid
                    return false;
                }
            }
            // If token type is assignment or clock
            else if (token.Type == TokenType.Assignment || token.Type == TokenType.Clock)
            {
                // If statement type is empty
                if (statementType == StatementType.Empty)
                {
                    // Add invalid assignment opreator error to error log
                    ErrorLog.Add(CurrentLineNumber, $"'{token.Text}' can only be used after a dependent variable or state variable in assignment statements.");
                    // Return invalid
                    return false;
                }
                // If statement type is assignment
                else if (statementType == StatementType.Assignment || statementType == StatementType.ClockAssignment)
                {
                    // Add extra assignment operator error to error log
                    ErrorLog.Add(CurrentLineNumber, "Only one assignment operator can appear in an assignment statement.");
                    // Return invalid
                    return false;
                }
                // If statement type is not variable display
                else if (statementType != StatementType.VariableDisplay)
                {
                    // Add invalid assignment operator error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Assignment operator '{token.Text}' can only be used in assignment statements.");
                    // Return invalid
                    return false;
                }
            }
            // If token type is formatter
            else if (token.Type == TokenType.Formatter)
            {
                // If statement is not empty and not variable display
                if (statementType != StatementType.Empty && statementType != StatementType.VariableDisplay)
                {
                    // Add invalid formatter error to error log
                    ErrorLog.Add(CurrentLineNumber, $"'{token.Text}' can only be used in a variable display statement.");
                    // Return invalid
                    return false;
                }
            }
            // If token type is asterick
            else if (token.Type == TokenType.Asterick)
            {
                // If statement type is not empty and not variable display
                if (statementType != StatementType.Empty && statementType != StatementType.VariableDisplay)
                {
                    // Add invalid asterick error to error log
                    ErrorLog.Add(CurrentLineNumber, $"'*' can only be used in a variable display statement.");
                    // Return invalid
                    return false;
                }
            }
            // If token type is colon
            else if (token.Type == TokenType.Colon)
            {
                // If statement type is not header and not instantiation
                if (statementType != StatementType.Header && statementType != StatementType.Instantiation)
                {
                    // Add invalid colon error to error log
                    ErrorLog.Add(CurrentLineNumber, $"':' can only be used to separate input variables from output variables in a header or instantiation statement.");
                    // Return invalid
                    return false;
                }
            }
            // If token type is comma
            else if (token.Type == TokenType.Comma)
            {
                // If statement type is not header and not instantiation
                if (statementType != StatementType.Header && statementType != StatementType.Instantiation)
                {
                    // Add invalid comma error to error log
                    ErrorLog.Add(CurrentLineNumber, $"',' can only be used to separate variables in a header or instantiation statement.");
                    // Return invalid
                    return false;
                }
            }
            // If token type is instantiation
            else if (token.Type == TokenType.Instantiation)
            {
                // If statement type is variable display
                if (statementType == StatementType.VariableDisplay)
                {
                    // Add misplaced variable error to error log
                    ErrorLog.Add(CurrentLineNumber, $"All variables in instantiation statements must be inside instantiation parentheses.");
                    // Return invalid
                    return false;
                }
                // If statement is not empty
                else if (statementType != StatementType.Empty)
                {
                    // Add invalid instantiation error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Instantiations must be there own statement.");
                    // Return invalid
                    return false;
                }

                // Return whether the line is a valid instantiation statement
                return ValidateInstantiation(InstantiationRegex.Match(token.Text), line);
            }
            // If token type is declaration
            else if (token.Type == TokenType.Declaration)
            {
                // If statement type is variable display
                if (statementType == StatementType.VariableDisplay)
                {
                    // Add invalid misplaced variable error to error log
                    ErrorLog.Add(CurrentLineNumber, $"All variables in header statements must be inside header parentheses.");
                    // Return invald
                    return false;
                }
                // If statement type is not empty
                else if (statementType != StatementType.Empty)
                {
                    // Add invalid declaration error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Headers must be there own statement.");
                    // Return invalid
                    return false;
                }
            }

            // If no errors thrown, return valid
            return true;
        }

        #endregion

        #region Token Helper Functions

        /// <summary>
        /// Returns whether the provided token is a left operand.
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Whether the provided token is a left operand</returns>
        private bool IsTokenLeftOperand(Token token)
        {
            // Return true if token is a variable, constant, close concatenation or close parenthesis
            return token != null && (token.Type == TokenType.Variable || token.Type == TokenType.Constant
                || token.Type == TokenType.CloseConcatenation || token.Type == TokenType.CloseParenthesis);
        }

        /// <summary>
        /// Returns whether the next lexeme is a right operand.
        /// </summary>
        /// <param name="nextLexeme">Next lexeme</param>
        /// <returns>Whether the next lexeme is a right operand</returns>
        private bool IsNextCharRightOperand(string nextLexeme)
        {
            // Get first character of next lexeme
            char firstCharacter = nextLexeme[0];
            // Return true if next lexeme is a variable, constant, open parenthesis, open concatenation opreator or a negation operator
            return char.IsLetterOrDigit(firstCharacter) || firstCharacter == '\'' || firstCharacter == '('
                || firstCharacter == '{' || firstCharacter == '~';
        }

        /// <summary>
        /// Returns whether the new token is an operator.
        /// </summary>
        /// <param name="newToken">New token</param>
        /// <param name="previousToken">Previous token</param>
        /// <param name="nextChar">Next lexeme</param>
        /// <returns>Whether the new token is an operator</returns>
        private bool IsNewTokenOperator(Token newToken, Token previousToken, string nextLexeme, StatementType? type)
        {
            // If new token is a boolean or math operator
            if (newToken.Type == TokenType.OrOperator || newToken.Type == TokenType.NegationOperator
                || newToken.Type == TokenType.ExclusiveOrOperator || newToken.Type == TokenType.MathOperator
                || newToken.Type == TokenType.EqualToOperator)
            {
                // Return is operator
                return true;
            }

            // If new token is space or newline and seperates two operands
            if ((newToken.Type == TokenType.Space || newToken.Type == TokenType.NewLine)
                && (type == StatementType.Assignment || type == StatementType.ClockAssignment)
                && IsTokenLeftOperand(previousToken) && IsNextCharRightOperand(nextLexeme) && !InsideConcat)
            {
                // Return is operator
                return true;
            }

            // Return not operator
            return false;
        }

        /// <summary>
        /// Gets the next character that is not a space or a newline character.
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="index">Index in line</param>
        /// <returns>Next character that is not a space or a newline character</returns>
        private char GetNextChar(string line, int index)
        {
            // For each character after the provided index to the end of the line
            for (int i = index + 1; i < line.Length; i++)
            {
                // If the current character is not a space or newline character
                if (line[i] != ' ' && line[i] != '\n')
                {
                    // Return current character
                    return line[i];
                }
            }
            // Return null character if no non-empty character remains
            return '\0';
        }

        #endregion

        #region Token Verification

        /// <summary>
        /// Updates the statement type from the new token, previous tokens and the current statement type.
        /// </summary>
        /// <param name="previousTokens">Previous tokens</param>
        /// <param name="newToken">New token</param>
        /// <param name="statementType">Current statement type</param>
        /// <returns>Updated statement type</returns>
        private StatementType? UpdateStatementType(List<Token> previousTokens, Token newToken, StatementType? statementType)
        {
            // If new token has a token type of space, newline or semicolon
            if (newToken.Type == TokenType.Space || newToken.Type == TokenType.NewLine || newToken.Type == TokenType.Semicolon)
            {
                // Return current statement type
                return statementType;
            }
            // If statement type is empty
            else if (statementType == StatementType.Empty)
            {
                // If new token has a token type of variable, constant, asterick, formatter, open concatenation or close concatenator opreator
                if (newToken.Type == TokenType.Variable || newToken.Type == TokenType.Constant || newToken.Type == TokenType.Asterick
                    || newToken.Type == TokenType.Formatter || newToken.Type == TokenType.OpenConcatenation || newToken.Type == TokenType.CloseConcatenation)
                {
                    // Return variable display statement type
                    return StatementType.VariableDisplay;
                }
                // If new token has a token type of instantiation
                else if (newToken.Type == TokenType.Instantiation)
                {
                    // Indicate current execution is inside a module
                    InsideModule = true;
                    // Return instantiation statement type
                    return StatementType.Instantiation;
                }
                // If new token has a token type of declaration
                else if (newToken.Type == TokenType.Declaration)
                {
                    // Indicate current execution is inside a module
                    InsideModule = true;
                    // Return header statement type
                    return StatementType.Header;
                }
                // Otherwise
                else
                {
                    // Return error statement type
                    return null;
                }
            }
            // If statement type is variable display
            else if (statementType == StatementType.VariableDisplay)
            {
                // If new token has a token type of assignment or clock
                if (newToken.Type == TokenType.Assignment || newToken.Type == TokenType.Clock)
                {
                    // Start grouping count at 0
                    int groupingCount = 0;
                    // Start inside concat bool at 0
                    bool insideConcat = false;
                    // Start variable outside group count at 0
                    int variableOutsideConcatCount = 0;
                    // For each previous token in previous tokens
                    foreach (Token previousToken in previousTokens)
                    {
                        // If previous token has a token type of variable
                        if (previousToken.Type == TokenType.Variable)
                        {
                            // If there is a previous token that is outside of a concatenation opreator 
                            if (variableOutsideConcatCount == 1)
                            {
                                // Add invalid variable grouping error to error log
                                ErrorLog.Add(CurrentLineNumber, $"In order to assign multiple variables in a single statement, you must place the variables inside a concatenation operator.");
                                // Return error statement type
                                return null;
                            }
                            
                            // If variable is not inside a concatenation operator
                            if (!insideConcat)
                            {
                                // If there was a concatenation operator already
                                if (groupingCount == 1)
                                {
                                    // Add invalid concatenation operator error to error log
                                    ErrorLog.Add(CurrentLineNumber, $"In order to assign multiple variables in a single statement, you must place the variables inside a single concatenation operator.");
                                    // Return error statement type
                                    return null;
                                }

                                // Increment variables outside concatenation opreator count
                                variableOutsideConcatCount++;
                            }
                        }
                        // If previous token has a token type of open concatenation
                        else if (previousToken.Type == TokenType.OpenConcatenation)
                        {
                            // If concatenation count is 1
                            if (groupingCount == 1)
                            {
                                // Add invalid concatenation error to error log
                                ErrorLog.Add(CurrentLineNumber, $"Only one concatenation can be used on the left side of an assignment statement.");
                                // Return error statement type
                                return null;
                            }

                            // Set inside concatenation to true
                            insideConcat = true;
                            // Increment concatenation count
                            groupingCount++;
                        }
                        // If previous token has a token type of close concatenation
                        else if (previousToken.Type == TokenType.CloseConcatenation)
                        {
                            // Set inside concatenation to false
                            insideConcat = false;
                        }
                        // If previous token has a token type of constant
                        else if (previousToken.Type == TokenType.Constant)
                        {
                            // Add invalid constant error to error log
                            ErrorLog.Add(CurrentLineNumber, $"Constants can’t be used on the left side of an assignment statement.");
                            // Return error statement type
                            return null;
                        }
                        // If previous token has a token type of asterick
                        else if (previousToken.Type == TokenType.Asterick)
                        {
                            // Add invalid asterick error to error log
                            ErrorLog.Add(CurrentLineNumber, $"'*' can only be used in variable display statements.");
                            // Return error statement type
                            return null;
                        }
                    }

                    // Add new parentheses level
                    Parentheses.Add(new ParenthesesLevel());
                    // Return assignment statement type
                    return newToken.Type == TokenType.Assignment ? StatementType.Assignment : StatementType.ClockAssignment;
                }
                // If new token has a token type of variable, constant, asterick,
                // open concatenation, close concatenation or formatter
                else if (newToken.Type == TokenType.Variable || newToken.Type == TokenType.Constant
                    || newToken.Type == TokenType.Asterick || newToken.Type == TokenType.OpenConcatenation
                    || newToken.Type == TokenType.CloseConcatenation || newToken.Type == TokenType.Formatter)
                {
                    // Return current statement type
                    return statementType;
                }
                // If new token has a token type of open parenthesis or close parenthesis
                else if (newToken.Type == TokenType.OpenParenthesis || newToken.Type == TokenType.CloseParenthesis)
                {
                    // Add invalid parenthesis error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Parentheses can't be used in variable display statements.");
                    // Return error statement type
                    return null;
                }
                // In any other case
                else
                {
                    // Return error statement type
                    return null;
                }
            }
            // In any other case
            else
            {
                // Return current statement type
                return statementType;
            }
        }

        /// <summary>
        /// Verifies whether the new token in the current execution follows valid syntax.
        /// </summary>
        /// <param name="previousTokens">Tokens before the new token</param>
        /// <param name="newToken">New token</param>
        /// <param name="nextLexeme">Next lexeme</param>
        /// <param name="statementType">Current statement type</param>
        /// <returns>Whether the new token follows valid syntax.</returns>
        private bool VerifySyntax(List<Token> previousTokens, Token newToken, string nextLexeme, StatementType? statementType)
        {
            // If new token is a semicolon
            if (newToken.Type == TokenType.Semicolon || newToken.Type == TokenType.Asterick || newToken.Type == TokenType.Formatter)
            {
                // Semicolons are verified when breaking lines into statements
                // Astericks are verified when updating statement type
                // Formatters are verified when updating statement type
                // Return valid syntax
                return true;
            }

            // If new token is inside a concatenation operator and is not allowed to be inside a concatenation opereator
            if (InsideConcat && newToken.Type != TokenType.Space && newToken.Type != TokenType.NewLine
                && newToken.Type != TokenType.Variable && newToken.Type != TokenType.Constant && newToken.Type != TokenType.CloseConcatenation)
            {
                // If inside formatted field
                if (InsideFormatter)
                {
                    // Add invalid token inside formatted field error to error log
                    ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' is not allowed inside a formatted field.");
                }
                // If just inside a concatenation opreator
                else
                {
                    // Add invalid token inside concatenation error to error log
                    ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' is not allowed inside concatenations.");
                }

                // Return invalid syntax
                return false;
            }

            // Init last token
            Token lastToken = null;
            // For all previous tokens in reverse
            foreach (Token previousToken in previousTokens.ToArray().Reverse())
            {
                // If token isn't a space or newline token
                if (previousToken.Type != TokenType.Space && previousToken.Type != TokenType.NewLine)
                {
                    // Set last token to this token
                    lastToken = previousToken;
                    // Break out of loop
                    break;
                }
            }

            // Get whether the new token is an operator
            bool isNewTokenOperator = IsNewTokenOperator(newToken, lastToken, nextLexeme, statementType);

            // If is a space or newline that isn't an operator
            if ((newToken.Type == TokenType.Space || newToken.Type == TokenType.NewLine) && !isNewTokenOperator)
            {
                // Return valid syntax
                return true;
            }

            // If new token is a variable
            if (newToken.Type == TokenType.Variable || newToken.Type == TokenType.Constant)
            {
                // If statement type is header and the current execution is not inside module parentheses
                if (statementType == StatementType.Header && !InsideModule)
                {
                    // Add misplaced variable error to error log
                    ErrorLog.Add(CurrentLineNumber, $"All variables in module header statements must be inside module parentheses.");
                    // Return invalid syntax
                    return false;
                }
                // If statement type is instantiation and the current execution is not inside module parentheses
                else if (statementType == StatementType.Instantiation && !InsideModule)
                {
                    // Add misplaced variable error to error log
                    ErrorLog.Add(CurrentLineNumber, $"All variables in instantiation statements must be inside instantiation parentheses.");
                    // Return invalid syntax
                    return false;
                }
            }
            // If new token is a constant
            else if (newToken.Type == TokenType.Constant)
            {
                // Get whether the constant has a size specification
                bool containsBitCount = char.IsDigit(newToken.Text[0]) && newToken.Text.Contains('\'');
                // If constant is inside a concatenation operator and doesn't have a size specification
                if (InsideConcat && !containsBitCount)
                {
                    // Add invalid constant error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Constants in concatenations must include a size specification.");
                    // Return invalid syntax
                    return false;
                }
                // If constant is inside module parentheses
                else if (InsideModule)
                {
                    // If current statement type is header
                    if (statementType == StatementType.Header)
                    {
                        // Add invalid constant error to error log
                        ErrorLog.Add(CurrentLineNumber, $"Constants are not allowed in header statements.");
                        // Return invalid syntax
                        return false;
                    }
                    // If previous tokens list contains any colon
                    else if (previousTokens.Any(t => t.Type == TokenType.Colon))
                    {
                        // Add invalid constant error to error log
                        ErrorLog.Add(CurrentLineNumber, $"Constants are not allowed as outputs in instantiation statements.");
                        // Return invalid syntax
                        return false;
                    }
                    // If constant doesn't include a size specification
                    else if (!containsBitCount)
                    {
                        // Add invalid constant error to error log
                        ErrorLog.Add(CurrentLineNumber, $"Constants in instantiation statements must include a size specification.");
                        // Return invalid syntax
                        return false;
                    }
                }
                // If statement type is instantiation and the current execution is not inside module parentheses
                else if (statementType == StatementType.Instantiation && !InsideModule)
                {
                    // Add misplaced variable error to error log
                    ErrorLog.Add(CurrentLineNumber, $"All variables in instantiation statements must be inside instantiation parentheses.");
                    // Return invalid syntax
                    return false;
                }
            }
            // If new token is an operator
            else if (isNewTokenOperator)
            {
                // If the new  operator is a math operator
                if (newToken.Type == TokenType.MathOperator)
                {
                    // If math operators aren't valid
                    if (!MathOperatorsValid)
                    {
                        // Add mixing of boolean and math operators error to error log
                        ErrorLog.Add(CurrentLineNumber, $"An assignment statement can not contain both Boolean and math operators in its expression.");
                        // Return invalid syntax
                        return false;
                    }
                }
                // If the new operator is not a math operator
                else
                {
                    // If this operator is the first operator
                    if (Operations.Count == 0)
                    {
                        // Set math operators to invalid
                        MathOperatorsValid = false;
                    }

                    // If this isn't the first operator and math operators are still valid
                    if (Operations.Count != 0 && MathOperatorsValid)
                    {
                        // Add mixing of boolean and math operators error to error log
                        ErrorLog.Add(CurrentLineNumber, $"An assignment statement can not contain both Boolean and math operators in its expression.");
                        // Return invalid syntax
                        return false;
                    }

                    // If operator is not a negation operator
                    if (newToken.Type != TokenType.NegationOperator)
                    {
                        // If new token is an exclusive or equal to operator and it is not inside any parentheses
                        if ((newToken.Type == TokenType.ExclusiveOrOperator || newToken.Type == TokenType.EqualToOperator) && Parentheses.Count == 1)
                        {
                            // Add invalid operator error to error log
                            ErrorLog.Add(CurrentLineNumber, $"Missing () for '{newToken.Text}' operation.");
                            // Return invalid syntax
                            return false;
                        }

                        // If left token isn't an operand
                        if (!IsTokenLeftOperand(lastToken))
                        {
                            // Add missing left operand error to error log
                            ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' is missing a left operand.");
                            // Return invalid syntax
                            return false;
                        }

                        // If next token isn't an operand
                        if (!IsNextCharRightOperand(nextLexeme))
                        {
                            // Add missing right operand error to error log
                            ErrorLog.Add(CurrentLineNumber, $"'{newToken.Text}' is missing a right operand.");
                            // Return invalid syntax
                            return false;
                        }
                    }

                    // If the parentheses level is unable to add the new operator (failures due to exclusive operators)
                    if (!Parentheses[Parentheses.Count - 1].TryAddOperator(newToken))
                    {
                        string exclusiveOperator = Parentheses[Parentheses.Count - 1].ExclusiveOperator != null ? Parentheses[Parentheses.Count - 1].ExclusiveOperator.Text : newToken.Text;
                        // Add invalid operator error to error log
                        ErrorLog.Add(CurrentLineNumber, $"'{exclusiveOperator}' operator must be the only operators within its enclosing parentheses.");
                        // Return invalid syntax
                        return false;
                    }
                }

                // If the current operators list doesn't have the current operator
                if (!Operations.Contains(newToken.Type))
                {
                    // Add current operator to the operator list
                    Operations.Add(newToken.Type);
                }
            }
            // If new token is an open parenthesis
            else if (newToken.Type == TokenType.OpenParenthesis)
            {
                // Set inside module to true if last token was a declaration or instantiation
                InsideModule = lastToken != null && (lastToken.Type == TokenType.Declaration || lastToken.Type == TokenType.Instantiation);
                // Add new parentheses level
                Parentheses.Add(new ParenthesesLevel());
            }
            // If new token is an open concatenation
            else if (newToken.Type == TokenType.OpenConcatenation)
            {
                // Set inside concatenation to true
                InsideConcat = true;
                // Set inside formatter to true if last token was a formatter
                InsideFormatter = lastToken != null && lastToken.Type == TokenType.Formatter;
            }
            // If new token is a close parenthesis
            else if (newToken.Type == TokenType.CloseParenthesis)
            {
                // If last token is not empty and is an open parenthesis
                if (lastToken != null && lastToken.Type == TokenType.OpenParenthesis)
                {
                    // Add empty parenthesis error to error log
                    ErrorLog.Add(CurrentLineNumber, $"() can not be empty.");
                    // Return invalid syntax
                    return false;
                }

                // Set inside module to false
                InsideModule = false;
                // Remove current parentheses
                Parentheses.RemoveAt(Parentheses.Count - 1);
            }
            // If new token is a close concatenation
            else if (newToken.Type == TokenType.CloseConcatenation)
            {
                // If last token isn't empty and it is an open concatenation
                if (lastToken != null && lastToken.Type == TokenType.OpenConcatenation)
                {
                    // Add empty concatenation error to error log
                    ErrorLog.Add(CurrentLineNumber, $"{{}} can not be empty.");
                    // Return invalid syntax
                    return false;
                }

                // Set inside concatenation to false
                InsideConcat = false;
                // Set inside formatter to false
                InsideFormatter = false;
            }
            // If new token is an assignment
            else if (newToken.Type == TokenType.Assignment)
            {
                // If next lexeme is empty or a semicolon
                if (nextLexeme == null || nextLexeme[0] == ';')
                {
                    // Add invalid assignment operator error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Assignment operator '{newToken.Text}' must have a right-hand expression.");
                    // Return invalid syntax
                    return false;
                }
            }
            // If new token is a clock assignment
            else if (newToken.Type == TokenType.Clock)
            {
                // If next lexeme is empty or a semicolon
                if (nextLexeme == null || nextLexeme[0] == ';')
                {
                    // Add invalid assignment operator error to error log
                    ErrorLog.Add(CurrentLineNumber, $"Assignment operator '{newToken.Text}' must have a right-hand expression.");
                    // Return invalid syntax
                    return false;
                }
            }
            // If new token is a comma
            else if (newToken.Type == TokenType.Comma)
            {
                // Make sure comma is inside a module, doesn't start a module, doesn't end a module
                // and seperates variables, constants and concatenations.
                if (!InsideModule || (lastToken.Type != TokenType.Variable && lastToken.Type != TokenType.Constant
                    && lastToken.Type != TokenType.CloseConcatenation))
                {
                    // Add invalid comma error to error log
                    ErrorLog.Add(CurrentLineNumber, $"',' can only be used to separate variables in a module header or instantiation statement.");
                    // Return invalid syntax
                    return false;
                }
            }
            // If new token is a colon
            else if (newToken.Type == TokenType.Colon)
            {
                // Make sure colon is inside a module, doesn't start a module, doesn't end a module
                // and seperates variables, constants and concatenations.
                if (!InsideModule || nextLexeme == null || (!char.IsLetterOrDigit(nextLexeme[0]) && nextLexeme[0] != '\'' && nextLexeme[0] != '{')
                    || (lastToken.Type != TokenType.Variable && lastToken.Type != TokenType.Constant && lastToken.Type != TokenType.CloseConcatenation))
                {
                    // Add invalid colon error to error log
                    ErrorLog.Add(CurrentLineNumber, $"':' can only be used to separate input variables from output variables in a module header or instantiation statement.");
                    // Return invalid syntax
                    return false;
                }

                // If there is a previous colon
                if (previousTokens.Any(t => t.Type == TokenType.Colon))
                {
                    // Add invalid colon error to error log
                    ErrorLog.Add(CurrentLineNumber, $"':' can only be used once in a module header or instantiation statement.");
                    // Return invalid syntax
                    return false;
                }
            }

            // Return valid if no syntax errors occurred
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
                ErrorLog.Add(CurrentLineNumber, $"You cannot instantiate a copy of the current design.");
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
                        string[] files = Directory.GetFiles(Design.FileSource.DirectoryName, string.Concat(designName, ".vbi"));
                        if (files.Length > 0)
                        {
                            // Create new design from the design path
                            Design newDesign = new Design(files[0]);
                            // Set sub design if it has a header line
                            subDesign = newDesign.HeaderLine != null ? newDesign : null;
                        }

                        if (subDesign == null)
                        {
                            for (int i = 0; i < Libraries.Count; i++)
                            {
                                files = Directory.GetFiles(Libraries[i], string.Concat(designName, ".vbi"));
                                if (files.Length > 0)
                                {
                                    // Create new design from the design path
                                    Design newDesign = new Design(files[0]);
                                    // Set sub design if it has a header line
                                    subDesign = newDesign.HeaderLine != null ? newDesign : null;
                                    // If the sub design was found
                                    if (subDesign != null)
                                    {
                                        // Break out of loop
                                        break;
                                    }
                                }
                            }

                            // If file is not found
                            if (subDesign == null)
                            {
                                // Add file not found to error log
                                ErrorLog.Add(CurrentLineNumber, $"Unable to find a module named '{designName}' in the current directory or any specified library.");
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
                    ErrorLog.Add(CurrentLineNumber, $"An error has occured while locating the module named '{designName}'. Please check your module name.");
                    return false;
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns a list of lexemes from the provided line.
        /// </summary>
        /// <param name="line">Line to get lexemes of</param>
        /// <returns>List of lexemes</returns>
        private List<string> GetLexemes(string line)
        {
            // Create lexeme list
            var lexemes = new List<string>();
            // Create string builder for current lexeme
            var currentLexeme = new StringBuilder();
            // Create groupings stack
            var groupings = new Stack<char>();

            // For each character in the line
            for (int i = 0; i < line.Length; i++)
            {
                // Get current character
                char currentChar = line[i];
                // Get current character as a string
                string currentCharString = currentChar.ToString();

                // If current character is an invalid character
                if (InvalidRegex.IsMatch(currentCharString))
                {
                    // If invalid character is a # or "
                    if (currentChar == '#' || currentChar == '"')
                    {
                        // Add invalid character error to error log
                        ErrorLog.Add(CurrentLineNumber, $"Invalid character '{currentChar}'.");
                    }
                    // If invalid character is any other character
                    else
                    {
                        // Add invalid character error to error log
                        ErrorLog.Add(CurrentLineNumber, $"Unrecognized character '{currentChar}'.");
                    }
                    // Return null for error
                    return null;
                }
                // If current character is a seperator character
                else if (SeperatorsList.Contains(currentChar))
                {
                    // If the current character is an open group character
                    if (currentChar == '(' || currentChar == '{')
                    {
                        // Get current grouping character
                        char currentGrouping = groupings.Count > 0 ? groupings.Peek() : '\0';
                        
                        // If current character is open parenthesis and current grouping is open concatenation
                        if (currentChar == '(' && currentGrouping == '{')
                        {
                            // Add parenthesis inside concatenation error to error log
                            ErrorLog.Add(CurrentLineNumber, $"Parenthesis are not allowed inside concatenations.");
                            // Return null for error
                            return null;
                        }
                        // If current character is open concatenation and current grouping is open concatenation
                        else if (currentChar == '{' && currentGrouping == '{')
                        {
                            // Add nested concatenation error to error log
                            ErrorLog.Add(CurrentLineNumber, $"Nested concatenations are not allowed.");
                            // Return null for error
                            return null;
                        }

                        // Push grouping character to the grouping stack
                        groupings.Push(currentChar);
                    }
                    // If the current character is a close group character
                    else if (currentChar == ')' || currentChar == '}')
                    {
                        // Get current grouping character
                        char currentGrouping = groupings.Count > 0 ? groupings.Pop() : '\0';

                        // If current opening grouping doesn't match the closing current character
                        if ((currentChar == ')' && currentGrouping != '(') || (currentChar == '}' && currentGrouping != '{'))
                        {
                            // If there is no current opening grouping
                            if (currentGrouping == '\0')
                            {
                                // Add unmatched grouping error to error log
                                ErrorLog.Add(CurrentLineNumber, $"'{currentChar}' does not have a matching opening.");
                            }
                            // If the current character doesn't close the current opening grouping
                            else
                            {
                                // Add unmatched grouping error to error log
                                ErrorLog.Add(CurrentLineNumber, $"'{currentGrouping}' does not have a matching closing.");
                            }
                            // Return null for error
                            return null;
                        }
                    }

                    // If current lexeme is not empty
                    if (currentLexeme.Length > 0)
                    {
                        // Add current lexeme to lexeme list
                        lexemes.Add(currentLexeme.ToString());
                        // Clear current lexeme
                        currentLexeme.Clear();
                    }

                    // Add seperator character to lexeme list
                    lexemes.Add(currentCharString);
                }
                // If current character is an appending character
                else
                {
                    // Get current lexeme value
                    string currentLexemeValue = currentLexeme.ToString();

                    // If the current lexeme is not empty and the current character is <
                    if (currentLexeme.Length > 0)
                    {
                        // Get last character
                        char lastChar = currentLexeme[currentLexeme.Length - 1];

                        // If current character is <
                        if (currentChar == '<')
                        {
                            // Add current lexeme to lexeme list
                            lexemes.Add(currentLexemeValue);
                            // Clear current lexeme
                            currentLexeme.Clear();
                        }
                        // If current character is ~
                        else if (currentLexemeValue == "~")
                        {
                            // If current character is not ~
                            if (currentChar != '~')
                            {
                                // Add current lexeme to lexeme list
                                lexemes.Add(currentLexemeValue);
                                // Clear current lexeme
                                currentLexeme.Clear();
                            }
                            else
                            {
                                // Clear current lexeme
                                currentLexeme.Clear();
                                // Continue and don't append
                                continue;
                            }
                        }
                        // If current character is *
                        else if (currentLexemeValue == "*")
                        {
                            // If current character is not *
                            if (currentChar != '*')
                            {
                                // Add current lexeme to lexeme list
                                lexemes.Add(currentLexemeValue);
                                // Clear current lexeme
                                currentLexeme.Clear();
                            }
                            else
                            {
                                // Clear current lexeme
                                currentLexeme.Clear();
                                // Continue and don't append
                                continue;
                            }
                        }
                        else if (currentLexemeValue == "==")
                        {
                            // Add current lexeme to lexeme list
                            lexemes.Add(currentLexemeValue);
                            // Clear current lexeme
                            currentLexeme.Clear();
                        }
                        else
                        {
                            if (currentLexemeValue == "=" && currentChar != '=')
                            {
                                // Add current lexeme to lexeme list
                                lexemes.Add(currentLexemeValue);
                                // Clear current lexeme
                                currentLexeme.Clear();
                            }
                            else if (currentLexemeValue == "<=" && currentChar != '@')
                            {
                                // Add current lexeme to lexeme list
                                lexemes.Add(currentLexemeValue);
                                // Clear current lexeme
                                currentLexeme.Clear();
                            }
                            else if (currentChar == '=' && lastChar != '=' && lastChar != '<')
                            {
                                // Add current lexeme to lexeme list
                                lexemes.Add(currentLexemeValue);
                                // Clear current lexeme
                                currentLexeme.Clear();
                            }
                        }
                    }

                    // Append current character to current lexeme
                    currentLexeme.Append(currentChar);
                }
            }

            // If groupings stack is not empty
            if (groupings.Count > 0)
            {
                // Unmatched grouping error
                ErrorLog.Add(CurrentLineNumber, $"Unmatched '{groupings.Peek()}'.");
                // Return null for error
                return null;
            }

            // Return lexemes list
            return lexemes;
        }

        /// <summary>
        /// Returns the next non-empty lexeme in the provided lexeme list from the provided index.
        /// </summary>
        /// <param name="lexemes">Lexeme list</param>
        /// <param name="index">Index of current lexeme</param>
        /// <returns>Next non-empty lexeme</returns>
        private string GetNextLexeme(List<string> lexemes, int index)
        {
            for (int i = index + 1; i < lexemes.Count; i++)
            {
                string lexeme = lexemes[i];
                if (lexeme[0] != '\n' && lexeme[0] != ' ')
                {
                    return lexeme;
                }
            }
            return '\0'.ToString();
        }
        
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
            // Save current line number
            int lineNumber = CurrentLineNumber;

            // Init all instance variables
            InsideConcat = false;
            InsideFormatter = false;
            InsideModule = false;
            MathOperatorsValid = true;
            Parentheses = new List<ParenthesesLevel>();
            Operations = new List<TokenType>();

            // Get lexeme list for the current line
            var lexemes = GetLexemes(line);
            // If lexeme list is null
            if (lexemes == null)
            {
                // Return null for statement type
                return null;
            }

            // For each lexeme in the lexeme list
            for (int i = 0; i < lexemes.Count; i++)
            {
                // Get the current lexeme
                string currentLexeme = lexemes[i];
                // Get the next non-empty lexeme
                string nextLexeme = GetNextLexeme(lexemes, i);

                // Get token type of current lexeme
                TokenType? tokenType = GetTokenType(currentLexeme, nextLexeme);
                // If token type is null
                if (tokenType == null)
                {
                    // Return null for statement type
                    return null;
                }
                // Create new token from current lexeme and its type
                Token newToken = new Token(currentLexeme, (TokenType)tokenType);
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
                if (!VerifySyntax(tokens, newToken, nextLexeme, statementType))
                {
                    // Return for error
                    return null;
                }

                // If seperator token is a new line token
                if (tokenType == TokenType.NewLine)
                {
                    // Increment current line number
                    CurrentLineNumber++;
                }

                // Add new token to tokens list
                tokens.Add(newToken);
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
            int rightBound = string.IsNullOrEmpty(vector.Groups["RightBound"].Value) ? -1 : Convert.ToInt32(vector.Groups["RightBound"].Value);

            if (rightBound == -1)
            {
                return new List<string>(new string[] { string.Concat(name, leftBound) });
            }
            
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
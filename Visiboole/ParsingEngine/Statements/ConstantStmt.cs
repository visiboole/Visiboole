using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VisiBoole.ParsingEngine.Statements
{
    public class ConstantStmt : Statement
    {
        public static Regex HexPattern { get; } = new Regex(@"^(([a-zA-Z]+\[[0-9]+\.\.[0-9]+\])|([a-zA-Z]+\[[0-9]+\.[0-9]+\.[0-9]+\]))\s*\=\s*\'[hH][a-fA-F0-9]+\;$");

        public static Regex DecPattern { get; } = new Regex(@"^(([a-zA-Z]+\[[0-9]+\.\.[0-9]+\])|([a-zA-Z]+\[[0-9]+\.[0-9]+\.[0-9]+\]))\s*\=\s*\'[dD][0-9]+\;$");

        public static Regex BinPattern { get; } = new Regex(@"^(([a-zA-Z]+\[[0-9]+\.\.[0-9]+\])|([a-zA-Z]+\[[0-9]+\.[0-9]+\.[0-9]+\]))\s*\=\s*\'[bB][0-1]+\;$");

        /// <summary>
        /// Constructs an instance of ConstantStmt
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
        public ConstantStmt(int lnNum, string txt) : base(lnNum, txt)
        {
        }

        public override void Parse()
        {
            try
            {
                if (HexPattern.Match(Text).Success)
                {

                }
                else if (DecPattern.Match(Text).Success)
                {

                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Globals.DisplayException(ex);
            }
        }
    }
}
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

                Regex regexLeft = new Regex(@"^(([a-zA-Z]+\[[0-9]+\.\.[0-9]+\])|([a-zA-Z]+\[[0-9]+\.[0-9]+\.[0-9]+\]))", RegexOptions.None); // Get left side
                string leftSide = regexLeft.Match(Text).Value; // Left side of equal sign
                List<string> leftVars = Expand(leftSide); // Expand left side to get all left variables

                if (HexPattern.Match(Text).Success)
                {
                    //TODO push left side variable as an output
                    //Get right side
                    Regex regexRight = new Regex(@"[a-fA-F0-9]+", RegexOptions.None);
                    string rightSide = regexRight.Match(Text).Value;

                    string outputHex = Convert.ToString(Convert.ToInt32(rightSide, 16), 2);
                    string[] outputArray = outputHex.Split();

                    outputArray = Array.ConvertAll<string, int>(tokens, int.Parse);

                    // Assign Loop'
                    foreach (string v in leftVars)
                    {
                        // grab variable from variable name
                        // variable = outputArray[leftVars.IndexOf(v)];
                    }
                }
                else if (DecPattern.Match(Text).Success)
                {
                    Regex regexRight = new Regex(@"[0-9]+\;", RegexOptions.None);
                    string rightSide = regexRight.Match(Text).Value;
                    rightSide.Remove(';');

                    string outputBin = Convert.ToString(Convert.ToInt32(rightSide, 10), 2);
                    string[] outputArray = outputBin.Split();

                    outputArray = Array.ConvertAll<string, int>(tokens, int.Parse);

                    // Assign Loop

                }
                else
                {
                    Regex regexRight = new Regex(@"[0-1]+\;", RegexOptions.None);
                    string rightSide = regexRight.Match(Text).Value;
                    rightSide.Remove(';');

                    string[] outputArray = rightSide.Split();

                    outputArray = Array.ConvertAll<string, int>(tokens, int.Parse);

                    // Assign Loop
                }
            }
            catch (Exception ex)
            {
                Globals.DisplayException(ex);
            }
        }
    }
}
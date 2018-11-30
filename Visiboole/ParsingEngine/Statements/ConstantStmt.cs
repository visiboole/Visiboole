using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisiBoole.Models;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.ParsingEngine.Statements
{
    public class ConstantStmt : Statement
    {
        /// <summary>
        /// Hex Constant Pattern
        /// </summary>
        public static Regex HexPattern { get; } = new Regex(@"^(" + Globals.regexVariable + @"|" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")\s*\=\s*\'[hH][a-fA-F0-9]+\;$");

        /// <summary>
        /// Decimal Constant Pattern
        /// </summary>
        public static Regex DecPattern { get; } = new Regex(@"^(" + Globals.regexVariable + @"|" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")\s*\=\s*\'[dD][0-9]+\;$");

        /// <summary>
        /// Binary Constant Pattern
        /// </summary>
        public static Regex BinPattern { get; } = new Regex(@"^(" + Globals.regexVariable + @"|" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")\s*\=\s*\'[bB][0-1]+\;$");

        public VariableListStmt VariableStmt;

        /// <summary>
        /// Constructs an instance of ConstantStmt
        /// </summary>
        /// <param name="lnNum">The line number that this statement is located on within edit mode - not simulation mode</param>
        /// <param name="txt">The raw, unparsed text of this statement</param>
        public ConstantStmt(SubDesign sd, int lnNum, string txt) : base(sd, lnNum, txt)
        {
            Parse();
        }

        public override void Parse()
        {
            Regex regexLeft = new Regex(@"^(" + Globals.regexVariable + @"|" + Globals.regexArrayVariables + @"|" + Globals.regexStepArrayVariables + @")", RegexOptions.None); // Get left side
            string leftSide = regexLeft.Match(Text).Value; // Left side of equal sign
            List<string> leftVars = new List<string>(); // Expand left side to get all left variables
            leftVars.Add(leftSide);
            char[] charBinary; // Each binary from right side
            int[] rightValues; // Converted values form right side

            if (HexPattern.Match(Text).Success)
            {
                Regex regexRight = new Regex(@"[hH][a-fA-F0-9]+", RegexOptions.None);
                string rightSide = regexRight.Match(Text).Value;

                string outputHex = Convert.ToString(Convert.ToInt32(rightSide.Substring(1), 16), 2);
                charBinary = outputHex.ToCharArray();
            }
            else if (DecPattern.Match(Text).Success)
            {
                Regex regexRight = new Regex(@"[dD][0-9]+", RegexOptions.None);
                string rightSide = regexRight.Match(Text).Value;

                string outputBin = Convert.ToString(Convert.ToInt32(rightSide.Substring(1), 10), 2);
                charBinary = outputBin.ToCharArray();
            }
            else
            {
                Regex regexRight = new Regex(@"[bB][0-1]+", RegexOptions.None);
                string rightSide = regexRight.Match(Text).Value;

                charBinary = rightSide.Substring(1).ToCharArray();
            }

            if (charBinary.Length != leftVars.Count)
            {
                MessageBox.Show("Number of values is not equal to the number of variables. Line: " + (LineNumber + 1), "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            rightValues = Array.ConvertAll(charBinary, c => (int)Char.GetNumericValue(c));

            string line = "";
            foreach (string var in leftVars)
            {
                int value = SubDesign.Database.TryGetValue(var);
                if (value != -1)
                {
                    SubDesign.Database.SetValue(var, rightValues[leftVars.IndexOf(var)] == 1);
                }
                else
                {
                    IndependentVariable newVar = new IndependentVariable(var, rightValues[leftVars.IndexOf(var)] == 1);
                    SubDesign.Database.AddVariable<IndependentVariable>(newVar);
                }

                if (rightValues[leftVars.IndexOf(var)] == 1)
                {
                    line = String.Concat(line, "*", var);
                }
                else
                {
                    line = String.Concat(line, var);
                }
                
                if (leftVars.IndexOf(var) != (leftVars.Count - 1)) line = String.Concat(line, " ");
            }

            line = String.Concat(line, ";");

            VariableStmt = new VariableListStmt(SubDesign, LineNumber, line);
        }
    }
}
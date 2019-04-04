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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VisiBoole.ParsingEngine.Boolean;
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.ParsingEngine.Statements;

namespace VisiBoole.Models
{
    public class HtmlBuilder
	{
		public string HtmlText = "";
		public string currentLine = "";
        private string trueColor = "'crimson'";
        private string falseColor = (Properties.Settings.Default.Colorblind) ? "'royalblue'" : "'green'";

        public HtmlBuilder(Design design, List<IObjectCodeElement> output)
        {
            List<List<IObjectCodeElement>> newOutput = PreParseHTML(output);
            int lineNumber = 0;

            foreach (List<IObjectCodeElement> line in newOutput)
            {
                lineNumber++;
                currentLine = "<p style=\"font-size:" + (Properties.Settings.Default.FontSize + 6) + "px\">";

                if (!(line.Count == 1 && line[0] is CommentStmt))
                {
                    // Do all this if not a comment statement
                    Dictionary<int, int> parenIndexes = new Dictionary<int, int>();

                    string fullLine = "";
                    int indexer = 0;
                    foreach (var token in line)
                    {
                        if (token.ObjCodeText == "(" || token.ObjCodeText == ")")
                        {
                            parenIndexes[fullLine.Length] = line.IndexOf(token, indexer);
                            indexer = line.IndexOf(token, indexer) + 1;
                        }

                        fullLine += token.ObjCodeText + " ";
                    }

                    #region Indexes which positions will be assigned which color in html
                    if (fullLine.Contains("("))
                    {
                        Stack<int> parenthesis = new Stack<int>();
                        for (int i = 0; i < fullLine.Length; i++)
                        {
                            char c = fullLine[i];
                            if (c == '(')
                            {
                                parenthesis.Push(i); // Push index
                            }
                            else if (c == ')')
                            {
                                int start = parenthesis.Pop();
                                string inner = fullLine.Substring(start, i - start + 1);
                                bool colorValue = ExpressionSolver.Solve(design.Database, inner) == 1;

                                line[parenIndexes[start]].ObjCodeValue = colorValue;
                                line[parenIndexes[start]].MatchingIndex = start;
                                line[parenIndexes[start]].Match = i;
                                line[parenIndexes[i]].ObjCodeValue = colorValue;
                                line[parenIndexes[i]].Match = start;
                                line[parenIndexes[i]].MatchingIndex = i;
                            }
                        }
                    }
                    #endregion
                }

                bool nextLineOverBarForParentheses = false;
                List<int> overBarList = new List<int>();

                if(line.Count == 0)
                {
                    currentLine += "<br>>";
                }

                foreach (IObjectCodeElement token in line)
                {
                    if (token is CommentStmt)
                    {
                        // Add coloring tags to comment
                        currentLine += String.Concat(ColorComment(token.ObjCodeText), " ");
                        continue;
                    }

                    #region Checks for bars to be put over parenthesis, and what color to assign them

                    string variable = token.ObjCodeText;
                    if (variable.Contains(';'))
                    {
                        variable = variable.Substring(0, variable.IndexOf(';'));
                    }
                    bool? value = token.ObjCodeValue;
                    Type varType = token.GetType();

                    if (variable.Contains('('))
                    {
                        if(nextLineOverBarForParentheses == true && token.ObjCodeValue == true)
                        {
                            overBarList.Add(token.MatchingIndex);
                            currentLine += "<font color=" + falseColor + " style=\"cursor: no-drop; text-decoration: overline;\" >" + variable + "</font>";
                            currentLine += " ";
                            nextLineOverBarForParentheses = false;
                        }
                        else if (nextLineOverBarForParentheses == true && token.ObjCodeValue == false)
                        {
                            overBarList.Add(token.MatchingIndex);
                            currentLine += "<font color=" + trueColor + " style=\"cursor: no-drop; text-decoration: overline;\" >" + variable + "</font>";
                            currentLine += " ";
                            nextLineOverBarForParentheses = false;
                        }
                        else if(token.ObjCodeValue == true)
                        {
                            currentLine += "<font color=" + trueColor + " style=\"cursor: no-drop;\" >" + variable + "</font>";
                            currentLine += " ";
                        }
                        else
                        {
                            currentLine += "<font color=" + falseColor + " style=\"cursor: no-drop;\" >" + variable + "</font>";
                            currentLine += " ";
                        }
                        continue;
                    }

                    if (variable.Contains(')'))
                    {
                        if (overBarList.Contains(token.Match) && token.ObjCodeValue == true)
                        {
                            currentLine += "<font color=" + falseColor + " style=\"cursor: no-drop; text-decoration: overline;\" >" + variable + "</font>";
                            currentLine += " ";
                        }
                        else if(overBarList.Contains(token.Match) && token.ObjCodeValue == false)
                        {
                            currentLine += "<font color=" + trueColor + " style=\"cursor: no-drop; text-decoration: overline;\" >" + variable + "</font>";
                            currentLine += " ";
                        }
                        else if (token.ObjCodeValue == true)
                        {
                            currentLine += "<font color=" + trueColor + " style=\"cursor: no-drop;\" >" + variable + "</font>";
                            currentLine += " ";
                        }
                        else
                        {
                            currentLine += "<font color=" + falseColor + " style=\"cursor: no-drop;\" >" + variable + "</font>";
                            currentLine += " ";
                        }
                        continue;
                    }

                    if(variable == "~")
                    {
                        nextLineOverBarForParentheses = true;
                        continue;
                    }

                    if (variable.Contains('~'))
                    {
                        if (value.Equals(null))
                        {
                            currentLine += "<font color='black' style=\"cursor: no-drop;\" >" + variable + "</font>";
                        }
                        else
                        {
                            if (value.Equals(true))
                            {
                                if (varType == typeof(DependentVariable)) //if variable is dependent
                                {
                                    currentLine += "<font color=" + falseColor + " style=\"cursor: no-drop; text-decoration: overline;\">" + variable.Substring(1) + "</font>";
                                }
                                else if (varType == typeof(IndependentVariable)) //if variable is independent
                                {
                                    currentLine += "<font color=" + falseColor + " style=\"cursor: hand; text-decoration: overline;\" onclick=\"window.external.Variable_Click('" + variable.Substring(1) + "')\" >" + variable.Substring(1) + "</font>";
                                }
                                else if (varType == typeof(Constant))
                                {
                                    currentLine += $"<font color={falseColor} style=\"cursor: no-drop; text-decoration: overline;\">{variable.Substring(1)}</font>";
                                }
                                currentLine += " ";
                            }
                            else
                            {
                                if (varType == typeof(DependentVariable)) //if variable is dependent
                                {
                                    currentLine += "<font color=" + trueColor + " style=\"cursor: no-drop; text-decoration: overline;\" >" + variable.Substring(1) + "</font>";
                                }
                                else if (varType == typeof(IndependentVariable)) //if variable is independent
                                {
                                    currentLine += "<font color=" + trueColor + " style=\"cursor: hand; text-decoration: overline;\" onclick=\"window.external.Variable_Click('" + variable.Substring(1) + "')\" >" + variable.Substring(1) + "</font>";
                                }
                                else if (varType == typeof(Constant))
                                {
                                    currentLine += $"<font color={trueColor} style=\"cursor: no-drop; text-decoration: overline;\">{variable.Substring(1)}</font>";
                                }
                                currentLine += " ";
                            }
                        }
                    }
                    else
                    {
                        if (value.Equals(null))
                        {
                            if (variable.Equals("&nbsp"))
                            {
                                currentLine += "&nbsp;";
                            }
                            else if (varType == typeof(Instantiation))
                            {
                                Instantiation instantiation = (Instantiation)token;
                                string encodedPath = instantiation.DesignPath.Replace("&", "&amp;").Replace("\\", "&back;").Replace("'", "&apos;");
                                currentLine += $"<font color='black' style=\"cursor: hand;\" onclick=\"window.external.Instantiation_Click('{variable}', '{instantiation.DesignName}', '{encodedPath}')\">{variable}</font>";
                                currentLine += " ";
                            }
                            else
                            {
                                currentLine += "<font color='black' style=\"cursor: no-drop;\" >" + variable + "</font>";
                                currentLine += " ";
                            }
                            
                        }
                        else
                        {
                            if (value.Equals(true))
                            {
                                if (varType == typeof(DependentVariable)) //if variable is dependent
                                {
                                    currentLine += "<font color=" + trueColor + " style=\"cursor: no-drop;\" >" + variable + "</font>";
                                }
                                else if (varType == typeof(IndependentVariable)) //if variable is independent
                                {
                                    currentLine += "<font color=" + trueColor + " style=\"cursor: hand;\" onclick=\"window.external.Variable_Click('" + variable + "')\" >" + variable + "</font>";
                                }
                                else if (varType == typeof(Constant))
                                {
                                    currentLine += "<font color=" + trueColor + " style=\"cursor: no-drop;\" >" + variable + "</font>";
                                }
                                currentLine += " ";
                            }
                            else
                            {
                                if (varType == typeof(DependentVariable)) //if variable is dependent
                                {
                                    currentLine += "<font color=" + falseColor + " style=\"cursor: no-drop;\" >" + variable + "</font>";
                                }
                                else if (varType == typeof(IndependentVariable))
                                {
                                    currentLine += "<font color=" + falseColor + " style=\"cursor: hand;\" onclick=\"window.external.Variable_Click('" + variable + "')\" >" + variable + "</font>";
                                }
                                else if (varType == typeof(Constant))
                                {
                                    currentLine += "<font color=" + falseColor + " style=\"cursor: no-drop;\" >" + variable + "</font>";
                                }
                                currentLine += " ";
                            }
                        }
                    }
                    #endregion
                }

                currentLine = currentLine.Substring(0, currentLine.Length - 1);
                currentLine += "</p>";
                HtmlText += currentLine + "\n";
            }
        }

        /// <summary>
        /// Adds coloring to comments.
        /// </summary>
        /// <param name="comment">Base comment</param>
        /// <returns>The comment with html coloring added</returns>
        private string ColorComment(string comment)
        {
            MatchCollection matches = Regex.Matches(comment, @"(<#?[a-zA-Z0-9]+>)|(<\/>)");
            if (matches.Count == 0)
            {
                // No special coloring specified
                return EncodeText(comment);
            }

            StringBuilder commentHTML = new StringBuilder();
            string appendText = "";
            string color = "";

            if (matches[0].Index != 0)
            {
                // Start comment
                appendText = comment.Substring(0, matches[0].Index);
                commentHTML.Append(EncodeText(appendText));
            }

            int indexOfMatch = 0;
            int indexAfterMatch = 0;
            int indexOfNextMatch = 0;
            bool coloring = false;
            bool isValidColor = true;
            bool isClosingTag = false;

            // Iterate through all matches and construct html
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i]; // Get match
                indexOfMatch = match.Index; // Get match index
                indexAfterMatch = indexOfMatch + match.Length; // Get index after match
                indexOfNextMatch = (i + 1 < matches.Count) ? matches[i + 1].Index : -1; // Get index of next match
                isClosingTag = match.Value == "</>";

                if (!isClosingTag)
                {
                    color = match.Value.Substring(1, match.Length - 2); // Get color by removing <>

                    // Check for true or false color
                    if (color.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        color = trueColor.Replace("'", "");
                    }
                    else if (color.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                    {
                        color = falseColor.Replace("'", "");
                    }

                    isValidColor = IsValidHTMLColor(color); // Check whether the provided color is valid

                    if (isValidColor)
                    {
                        if (coloring)
                        {
                            commentHTML.Append("</font>"); // Append closing color tag
                        }
                        commentHTML.Append($"<font color='{color}'>"); // Append color tag
                        coloring = true;
                    }
                    else
                    {
                        commentHTML.Append(EncodeText(match.Value)); // Append bad color tag
                    }
                }
                else
                {
                    if (coloring)
                    {
                        commentHTML.Append("</font>"); // Append closing color tag
                        coloring = false;
                    }
                    else
                    {
                        commentHTML.Append(EncodeText(match.Value)); // Append bad closing tag
                    }
                }

                if (indexOfNextMatch != -1)
                {
                    // There is another match
                    appendText = comment.Substring(indexAfterMatch, indexOfNextMatch - indexAfterMatch);
                    commentHTML.Append(EncodeText(appendText));
                }
                else
                {
                    // There are no other matches
                    appendText = comment.Substring(indexAfterMatch, comment.Length - indexAfterMatch);
                    commentHTML.Append(EncodeText(appendText));
                    if (coloring)
                    {
                        commentHTML.Append("</font>");
                    }
                }
            }

            return commentHTML.ToString();
        }

        /// <summary>
        /// Encodes specific characters with their html encoding values. (This allows certain characters to show up in the simulator)
        /// </summary>
        /// <param name="comment">Comment to encode</param>
        /// <returns>Comment with specific characters encoded.</returns>
        private string EncodeText(string comment)
        {
            // Replace specific characters with their encodings so they show in text
            comment = Regex.Replace(comment, @"(?<=\s)\s", "&nbsp;");
            comment = comment.Replace("<", "&lt;");
            comment = comment.Replace(">", "&gt;");
            return comment;
        }

        /// <summary>
        /// Returns whether the provided color is a recognized color.
        /// </summary>
        /// <param name="color">Color to check</param>
        /// <returns>Whether the color is a valid html color</returns>
        private bool IsValidHTMLColor(string color)
        {
            if (Regex.IsMatch(color, @"^#(?:[0-9a-fA-F]{3}){1,2}$"))
            {
                return true;
            }

            return System.Drawing.Color.FromName(color).IsKnownColor;
        }

        private List<List<IObjectCodeElement>> PreParseHTML(List<IObjectCodeElement> output)
        {
            List<List<IObjectCodeElement>> fullText = new List<List<IObjectCodeElement>>();
            List<IObjectCodeElement> subText = new List<IObjectCodeElement>();
            foreach (IObjectCodeElement element in output)
            {
                Type elementType = element.GetType();
                if (elementType == typeof(LineFeed))
                {
                    fullText.Add(subText);
                    subText = new List<IObjectCodeElement>();
                }
                else
                {
                    subText.Add(element);
                }
            }
            return fullText;
        }

		/// <summary>
		/// Returns the generated HTML text
		/// </summary>
		/// <returns>Returns the generated HTML text</returns>
		public string GetHTML()
		{
			return HtmlText;
		}

        /// <summary>
        /// Displays the html text within the give WebBrowser object
        /// </summary>
        /// <param name="html">The html text to display</param>
        /// <param name="browser">The WebBrowser object to display the HTML in</param>
        public void DisplayHtml(string html, WebBrowser browser)
		{
            browser.Refresh();
            browser.Navigate("about:blank");

			if (browser.Document != null)
			{
				browser.Document.Write(string.Empty);
			}
            
            string styles = "<html><head><style type=\"text/css\"> p { margin: 0;} </style ></head >";
            html = styles + html;
            browser.DocumentText = html;
        }
	}
}
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
using System.Security.Permissions;
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

        public HtmlBuilder(SubDesign sd, List<IObjectCodeElement> output)
        {
            List<List<IObjectCodeElement>> newOutput = PreParseHTML(output);
            int lineNumber = 0;
            string trueColor = "'crimson'";
            string falseColor = (Properties.Settings.Default.Colorblind) ? "'royalblue'" : "'green'";

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
                    string outermost = "";
                    if (fullLine.Contains("(") && fullLine.Contains(")"))
                    {
                        int startIndex = fullLine.IndexOf('(');
                        int endIndex = fullLine.IndexOf(')');
                        List<int> previousStartingIndexes = new List<int>();

                        while (startIndex < endIndex)
                        {
                            int holdingIndex = startIndex;
                            startIndex = fullLine.IndexOf('(', startIndex + 1);

                            if (startIndex == -1 || previousStartingIndexes.Contains(startIndex))
                            {
                                startIndex = holdingIndex;

                                outermost = fullLine.Substring(startIndex, endIndex - startIndex + 1);
                                Expression exp = new Expression();
                                bool colorValue = exp.Solve(outermost);

                                line[parenIndexes[startIndex]].ObjCodeValue = colorValue;
                                line[parenIndexes[startIndex]].MatchingIndex = startIndex;
                                line[parenIndexes[startIndex]].Match = endIndex;
                                line[parenIndexes[endIndex]].ObjCodeValue = colorValue;
                                line[parenIndexes[endIndex]].Match = startIndex;
                                line[parenIndexes[endIndex]].MatchingIndex = endIndex;

                                previousStartingIndexes.Add(startIndex);

                                endIndex = fullLine.IndexOf(')', endIndex + 1);
                                startIndex = 0;

                                if (endIndex == -1)
                                {
                                    break;
                                }
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
                        // Replace possible colors and provide normal spacing
                        string commentHTML = Regex.Replace(token.ObjCodeText, @"(?<=\s)\s", "&nbsp;");
                        commentHTML = ColorComment(commentHTML);
                        currentLine += String.Concat(commentHTML, " ");
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
                                else //if variable is independent
                                {
                                    currentLine += "<font color=" + falseColor + " style=\"cursor: hand; text-decoration: overline;\" onclick=\"window.external.Variable_Click('" + variable.Substring(1) + "')\" >" + variable.Substring(1) + "</font>";
                                }
                                currentLine += " ";
                            }
                            else
                            {
                                if (varType == typeof(DependentVariable)) //if variable is dependent
                                {
                                    currentLine += "<font color=" + trueColor + " style=\"cursor: no-drop; text-decoration: overline;\" >" + variable.Substring(1) + "</font>";
                                }
                                else //if variable is independent
                                {
                                    currentLine += "<font color=" + trueColor + " style=\"cursor: hand; text-decoration: overline;\" onclick=\"window.external.Variable_Click('" + variable.Substring(1) + "')\" >" + variable.Substring(1) + "</font>";
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
                                else //if variable is independent
                                {
                                    currentLine += "<font color=" + trueColor + " style=\"cursor: hand;\" onclick=\"window.external.Variable_Click('" + variable + "')\" >" + variable + "</font>";
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
                                else
                                {
                                    // Comments were here. Keep this in case something else ends up here. Note what does.
                                    bool test = true;                                    
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
            bool coloring = false;
            string color = "";

            MatchCollection matches = Regex.Matches(comment, @"(<#?[a-zA-Z0-9]+>)|(<\/>)");
            if (matches.Count == 0 || (matches.Count > 0 && matches[0].Index != 0))
            {
                // Comment starts with black
                comment = comment.Insert(0, "<font color='black'>");
                color = "black";
                coloring = true;

                if (matches.Count == 0)
                {
                    comment = String.Concat(comment, "</font>");
                    return comment;
                }
            }

            // Replace all coloring tags with proper html color tags
            while (Regex.IsMatch(comment, @"(<#?[a-zA-Z0-9]+>)|(<\/>)"))
            {
                Match match = Regex.Match(comment, @"(<#?[a-zA-Z0-9]+>)|(<\/>)");

                int indexAfterMatch = match.Index + match.Length;
                int lengthAfterMatch = comment.Length - indexAfterMatch;

                if (coloring || match.Value.Equals("</>"))
                {
                    // Close color
                    comment = String.Concat(comment.Substring(0, match.Index), "</font>", comment.Substring(indexAfterMatch, lengthAfterMatch));
                    coloring = false;

                    // Move index and length values if adding a color
                    indexAfterMatch = match.Index + 7; // Moves index past </font>
                    lengthAfterMatch = comment.Length - indexAfterMatch;
                }

                // New color
                coloring = true;
                int indexBeforeMatch = color.Equals("") ? match.Index : indexAfterMatch;
                string newColor = match.Value.Substring(1, match.Length - 2); // Remove <>
                bool isValidColor = IsValidHTMLColor(newColor);
                color = isValidColor ? newColor : "black"; // Color is set to new color if valid and black if not valid
                comment = String.Concat(comment.Substring(0, indexBeforeMatch), $"<font color='{color}'>", comment.Substring(indexAfterMatch, lengthAfterMatch));
            }

            if (!comment.EndsWith("</font>"))
            {
                comment = String.Concat(comment, "</font>");
            }

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
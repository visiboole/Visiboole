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
using VisiBoole.ParsingEngine.ObjectCode;
using VisiBoole.ParsingEngine.Statements;

namespace VisiBoole.Models
{
    public class HtmlBuilder
	{
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
        /// Gets the html output from the provided object code.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="isSubdesign">Indicates whether a subdesign is being outputted</param>
        /// <returns></returns>
        public string GetHTML(List<IObjectCodeElement> output, bool isSubdesign = false)
        {
            string html = "";
            string currentLine = "";
            List<List<IObjectCodeElement>> newOutput = PreParseHTML(output);

            foreach (List<IObjectCodeElement> line in newOutput)
            {
                currentLine = "<p style=\"font-size:@FONTSIZE@pt;\">";

                if (line.Count == 0)
                {
                    currentLine += "<br>";
                }

                for (int i = 0; i < line.Count; i++)
                {
                    IObjectCodeElement token = line[i];

                    if (token is Comment)
                    {
                        // Add comment to the current line
                        currentLine += EncodeText(token.ObjCodeText);
                        continue;
                    }
                    else if (token is Operator && token.ObjCodeText == ";")
                    {
                        // Add comment to the current line
                        currentLine += "<span style=\"display: @SEMICOLONDISPLAY@; font-size:@FONTSIZE@pt;\">;</span>";
                        continue;
                    }

                    string variable = token.ObjCodeText;
                    bool? value = token.ObjCodeValue;
                    bool hasNegation = token.ObjHasNegation;
                    Type varType = token.GetType();

                    string color = "";
                    string cursor = "";
                    string decoration = "";
                    string action = "";
                    bool fontTag = false;
                    string template = "<font {0} {1} {2} {3}>{4}</font>";

                    if (value == null)
                    {
                        if (varType == typeof(Formatter))
                        {
                            fontTag = true;
                            Formatter formatter = (Formatter)token;
                            color = "color=\"black\"";
                            cursor = formatter.NextValue != null && !isSubdesign ? "style=\"cursor:hand\"" : "style=\"cursor:no-drop\"";
                            action = $"onclick=\"window.external.Variable_Click('{formatter.Variables}', '{formatter.NextValue}')\"";
                        }
                        else if (varType == typeof(Instantiation))
                        {
                            fontTag = true;
                            color = "color=\"black\"";
                            cursor = "style=\"cursor:hand\"";
                            action = $"onclick=\"window.external.Instantiation_Click('{variable}')\"";
                        }
                    }
                    else
                    {
                        fontTag = true;
                        color = ((bool)value) ? $"color=\"@TRUECOLOR@\"" : $"color=\"@FALSECOLOR@\"";
                        cursor = varType == typeof(IndependentVariable) && !isSubdesign ? "style=\"cursor:hand;" : "style=\"cursor:no-drop;";
                        decoration = hasNegation ? " text-decoration: overline;\"" : " \"";
                        action = cursor[14] == 'h' ? $"onclick=\"window.external.Variable_Click('{variable}')\"" : "";
                    }

                    if (fontTag)
                    {
                        currentLine += string.Format(template, color, cursor, decoration, action, variable);
                    }
                    else
                    {
                        currentLine += variable;
                    }
                }

                currentLine += "</p>";
                html += currentLine;
            }

            return html;
        }

        /// <summary>
        /// Encodes specific characters with their html encoding values. (This allows certain characters to show up in the simulator)
        /// </summary>
        /// <param name="comment">Comment to encode</param>
        /// <returns>Comment with specific characters encoded.</returns>
        private string EncodeText(string comment)
        {
            // Replace specific characters with their encodings so they show in text
            comment = comment.Replace(" ", "&nbsp;");
            comment = comment.Replace("<", "&lt;");
            comment = comment.Replace(">", "&gt;");
            return string.Concat("<span style=\"display: @COMMENTDISPLAY@; font-size:@FONTSIZE@pt;\">", comment, "</span>");
        }
	}
}
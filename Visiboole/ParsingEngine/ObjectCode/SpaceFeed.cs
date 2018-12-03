using System;

namespace VisiBoole.ParsingEngine.ObjectCode
{
    /// <summary>
    /// A discrete element of output representing a spacefeed
    /// </summary>
	public class SpaceFeed : IObjectCodeElement
	{
        /// <summary>
        /// The text representation of this outpute element, a space character
        /// </summary>
		public string ObjCodeText { get { return "&nbsp"; } set { } }

        /// <summary>
        /// The value of this element is null as it is a newline character, not a variable
        /// </summary>
		public bool? ObjCodeValue { get { return null; }set { } }

        public int Match { get; set; }
        public int MatchingIndex { get; set; }
    }
}
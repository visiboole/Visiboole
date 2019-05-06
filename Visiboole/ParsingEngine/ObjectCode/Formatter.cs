using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisiBoole.ParsingEngine.ObjectCode
{
    public class Formatter : IObjectCodeElement
    {
        /// <summary>
        /// String representation of this output element.
        /// </summary>
		public string ObjCodeText { get; private set; }

        /// <summary>
        /// Boolean value of this output element.
        /// </summary>
		public bool? ObjCodeValue { get; private set; }

        /// <summary>
        /// Indicates whether this output element contains a negation.
        /// </summary>
        public bool ObjHasNegation { get; private set; }

        public string Variables { get; private set; }

        public string NextValue { get; private set; }

        /// <summary>
        /// Constructs a formatter instance with the provided text.
        /// </summary>
        /// <param name="text">String representation of this output element</param>
		public Formatter(string value, string variables, string nextValue)
        {
            ObjCodeText = value;
            Variables = variables;
            NextValue = nextValue;
            ObjHasNegation = false;
            ObjCodeValue = null;
        }
    }
}

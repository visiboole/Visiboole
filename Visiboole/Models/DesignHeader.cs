using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisiBoole.Controllers;
using VisiBoole.ParsingEngine;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.Models
{
    public class DesignHeader
    {
        /// <summary>
        /// Indicates whether the design header is valid.
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// Message describing the invalid header.
        /// </summary>
        public string InvalidMessage { get; set; }

        /// <summary>
        /// Inputs of the design header.
        /// </summary>
        private List<string> Inputs;

        /// <summary>
        /// Number of input slots in the header.
        /// </summary>
        public Dictionary<int, int> InputSlots { get; private set; }

        /// <summary>
        /// Outputs of the design header.
        /// </summary>
        private List<string> Outputs;

        /// <summary>
        /// Number of output slots in the header.
        /// </summary>
        public Dictionary<int, int> OutputSlots { get; private set; }

        /// <summary>
        /// Constructs a design header with the no inputs or outputs.
        /// </summary>
        public DesignHeader()
        {
            Inputs = new List<string>();
            InputSlots = new Dictionary<int, int>();
            Outputs = new List<string>();
            OutputSlots = new Dictionary<int, int>();
            Valid = true;
            InvalidMessage = null;
        }

        /// <summary>
        /// Attempts to add the provided input variable to the provided slot.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="inputVariable"></param>
        /// <returns>Whether the variable was added or not.</returns>
        public bool AddInput(int slot, string inputVariable)
        {
            if (!Inputs.Contains(inputVariable))
            {
                Inputs.Add(inputVariable);

                if (!InputSlots.ContainsKey(slot))
                {
                    InputSlots.Add(slot, 1);
                }
                else
                {
                    InputSlots[slot]++;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to add the provided output variable to the provided slot.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="outputVariable"></param>
        /// <returns>Whether the variable was added or not.</returns>
        public bool AddOutput(int slot, string outputVariable)
        {
            if (!Outputs.Contains(outputVariable))
            {
                Outputs.Add(outputVariable);

                if (!OutputSlots.ContainsKey(slot))
                {
                    OutputSlots.Add(slot, 1);
                }
                else
                {
                    OutputSlots[slot]++;
                }

                return true;
            }
            else
            {
                // Set header to invalid
                Valid = false;
                // Set invalid message
                InvalidMessage = $"'{outputVariable}' can not be used as an output twice.";
                // Return false for error
                return false;
            }
        }

        /// <summary>
        /// Returns all input variables in the design header.
        /// </summary>
        /// <returns></returns>
        public string[] GetInputs()
        {
            return Inputs.ToArray();
        }

        /// <summary>
        /// Returns whether the header has the provided input variable.
        /// </summary>
        /// <param name="variable">Variable to look for</param>
        /// <returns></returns>
        public bool HasInputVariable(string variable)
        {
            return Inputs.Contains(variable);
        }

        /// <summary>
        /// Checks whether all header inputs were used respectively.
        /// </summary>
        /// <returns>Variable that wasn't used.</returns>
        public string VerifyInputs()
        {
            foreach (string input in Inputs)
            {
                var indVar = DesignController.ActiveDesign.Database.TryGetVariable<IndependentVariable>(input) as IndependentVariable;
                if (indVar == null)
                {
                    return input;
                }
            }

            return null;
        }
        /// <summary>
        /// Returns all output variables in the design header.
        /// </summary>
        /// <returns></returns>
        public string[] GetOutputs()
        {
            return Outputs.ToArray();
        }

        /// <summary>
        /// Checks whether all header outputs were used respectively.
        /// </summary>
        /// <returns>Variable that wasn't used.</returns>
        public string VerifyOutputs()
        {
            foreach (string output in Outputs)
            {
                if (DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>(output) as DependentVariable == null)
                {
                    if (DesignController.ActiveDesign.Database.TryGetVariable<DependentVariable>($"{output}.d") as DependentVariable == null)
                    {
                        return output;
                    }
                }
            }

            return null;
        }
    }
}
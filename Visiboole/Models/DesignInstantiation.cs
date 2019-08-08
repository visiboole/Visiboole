using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiBoole.Controllers;
using VisiBoole.ParsingEngine;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisiBoole.Models
{
    /// <summary>
    /// Class representing a design instantiation.
    /// </summary>
    public class DesignInstantiation
    {
        /// <summary>
        /// Parser of the design instantiation.
        /// </summary>
        private Parser Parser;

        /// <summary>
        /// Design of the design instantiation.
        /// </summary>
        private Design Design;

        /// <summary>
        /// Input variables for the instantiation.
        /// </summary>
        private string[] Inputs;

        /// <summary>
        /// Output variables for the instantiation.
        /// </summary>
        private string[] Outputs;

        /// <summary>
        /// List of no contact values for the instantiation.
        /// </summary>
        public List<bool> NoContactValues { get; private set; }

        /// <summary>
        /// List of previous input values for the instantiation.
        /// </summary>
        public bool[] CachedInputValues { get; private set; }

        /// <summary>
        /// Constructs a design instantiation with the provided design, inputs and outputs.
        /// </summary>
        /// <param name="design">Design of the design instantiation</param>
        /// <param name="inputs">Input variables for the instantiation</param>
        /// <param name="outputs">Output variables for the instantiation</param>
        public DesignInstantiation(Design design, string[] inputs, string[] outputs)
        {
            Design = design;
            Inputs = inputs;
            Outputs = outputs;
            NoContactValues = new List<bool>();
            CachedInputValues = null;
        }

        /// <summary>
        /// Returns whether an input's value has changed
        /// </summary>
        /// <returns></returns>
        private bool HasInputChanged()
        {
            // Get current design
            var currentDesign = DesignController.ActiveDesign;

            // If there are cached input values
            if (CachedInputValues != null)
            {
                // Start cached input index at 0
                int cachedInputIndex = 0;

                // For each input
                foreach (string input in Inputs)
                {
                    // If the input has changed value
                    if ((currentDesign.Database.GetValue(input) == 1) != CachedInputValues[cachedInputIndex])
                    {
                        // Return true for an input changing
                        return true;
                    }

                    // Increment cached input index
                    cachedInputIndex++;
                }

                // Return false for no input changing
                return false;
            }
            // If there aren't cached input values
            else
            {
                // Return true to run instantiation
                return true;
            }
        }

        /// <summary>
        /// Runs and returns the output values of the instantiation.
        /// </summary>
        /// <returns>Output values of the instantiation.</returns>
        private List<bool> RunInstantiation()
        {
            var currentDesign = DesignController.ActiveDesign;
            var inputVariables = new List<Variable>();
            CachedInputValues = new bool[Inputs.Length];
            var headerInputs = Design.Database.Header.GetInputs();
            for (int i = 0; i < headerInputs.Length; i++)
            {
                bool value = currentDesign.Database.GetValue(Inputs[i]) == 1;
                CachedInputValues[i] = value;
                inputVariables.Add(new IndependentVariable(headerInputs[i], value));
            }

            // If the parser hasn't be used
            if (Parser == null)
            {
                // Create the parser
                Parser = new Parser();
            }
            // Set the subdesign to be the active design
            DesignController.ActiveDesign = Design;
            // Parse subdesign
            List<bool> outputValues = Parser.ParseInstantiation(Design, inputVariables);
            // Reset active design
            DesignController.ActiveDesign = currentDesign;

            return outputValues;
        }

        /// <summary>
        /// Updates the output values from the ran instantiation.
        /// </summary>
        /// <param name="outputValues">Values to use in the update.</param>
        private void UpdateOutputs(List<bool> outputValues)
        {
            var currentDesign = DesignController.ActiveDesign;
            NoContactValues = new List<bool>();
            int outputValueIndex = 0;
            foreach (var output in Outputs)
            {
                if (output != "NC")
                {
                    currentDesign.Database.SetValue(output, outputValues[outputValueIndex++]);
                }
                else
                {
                    NoContactValues.Add(outputValues[outputValueIndex++]);
                }
            }
        }

        /// <summary>
        /// Runs the design instantiation and returns whether the instance was successful or not.
        /// </summary>
        /// <returns>Whether the instance was successful or not.</returns>
        /// <param name="tick">Indicates whether the instantiation design needs to be ticked</param>
        /// <param name="forceRun">Indicates whether the instantiation should be forced to run</param>
        public bool TryRun(bool tick, bool forceRun = false)
        {
            // Get current design
            var currentDesign = DesignController.ActiveDesign;

            // If not tick
            if (!tick)
            {
                // If instantiation needs to be ran
                if (forceRun || HasInputChanged())
                {
                    // Run the instantiation for the outputs
                    var outputValues = RunInstantiation();
                    // If no output was returned
                    if (outputValues == null)
                    {
                        // Return false for error
                        return false;
                    }
                    // Update the outputs
                    UpdateOutputs(outputValues);
                }
            }
            // If tick
            else
            {
                // Set the subdesign to be the active design
                DesignController.ActiveDesign = Design;
                // Tick the clocks in the design
                Design.TickClocks();
                // Run the instantiation for the outputs
                var outputValues = RunInstantiation();
                // Reset active design
                DesignController.ActiveDesign = currentDesign;
                // Update the outputs
                UpdateOutputs(outputValues);
            }

            // Return true for success
            return true;
        }

        /// <summary>
        /// Returns whether the instantiation was reran due to new input values.
        /// </summary>
        /// <returns>Whether the instantiation was reran due to new input values.</returns>
        public bool CheckRerun()
        {
            // If instantiation needs to be reran
            if (HasInputChanged())
            {
                // Rerun instance
                TryRun(false, true);
                // Return true for the instantiation being ran again
                return true;
            }
            else
            {
                // Return false for the instantiation was not ran again
                return false;
            }
        }

        /// <summary>
        /// Returns the instantiation design.
        /// </summary>
        /// <returns>Instantiation design.</returns>
        public Design GetDesign(string instantiation = null)
        {
            return instantiation == null ? Design : Design.GetInstantiationDesign(instantiation);
        }

        /// <summary>
        /// Returns the output of an instantiation.
        /// </summary>
        /// <returns>Output of an instantiation.</returns>
        public List<IObjectCodeElement> OpenInstantiation(string instantiation = null)
        {
            var currentDesign = DesignController.ActiveDesign;
            DesignController.ActiveDesign = Design;
            var output = instantiation != null ? Design.OpenInstantiation(instantiation) : Design.GetOutput();
            DesignController.ActiveDesign = currentDesign;
            return output;
        }

        /// <summary>
        /// Closes the provided instantiation in the design.
        /// </summary>
        /// <param name="instantiation">Instantiation to close.</param>
        public void CloseInstantiation(string instantiation)
        {
            Design.CloseInstantiation(instantiation);
        }

        /// <summary>
        /// Closes all instantiations within this instantiation.
        /// </summary>
        public void CloseInstantiations()
        {
            Design.CloseActiveInstantiation();
        }
    }
}
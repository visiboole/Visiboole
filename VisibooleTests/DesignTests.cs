using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiBoole.Models;
using VisiBoole.ParsingEngine;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisibooleTests
{
    [TestClass()]
    public class DesignTests
    {
        /// <summary>
        /// Tests whether the provided design successfully parses and outputs its html.
        /// </summary>
        /// <param name="name">Design name</param>
        /// <returns>Whether the provided design successfully parses and outputs its html</returns>
        private Design TestDesign(string name)
        {
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            string fileName = Path.Combine(path, "Resources", "Testing Files", name);
            Design design = new Design(fileName, delegate { });
            try
            {
                Parser parser = new Parser();
                List <IObjectCodeElement> output = parser.Parse(design);
                if (output == null)
                {
                    return null;
                }

                HtmlBuilder html = new HtmlBuilder(design, output);
                if (html.HtmlText == null)
                {
                    return null;
                }

                return design;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Tests whether the provided variable exists and has the provided type and value.
        /// </summary>
        /// <typeparam name="T">Variable type</typeparam>
        /// <param name="design">Design</param>
        /// <param name="name">Variable name</param>
        /// <param name="expectedValue">Expected value</param>
        /// <returns>Whether the provided variable exists and has the provided type and value</returns>
        private bool TestVariable<T>(Design design, string name, bool expectedValue) where T : Variable
        {
            try
            {
                T var = (T)design.Database.TryGetVariable<T>(name);
                if (var == null || var.Value != expectedValue)
                {
                    // Variable must be type T and have a value of expectedValue
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [TestMethod()]
        public void TestEmptyDesign()
        {
            // Test design parsing and html output
            Design design = TestDesign("Empty.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 0) Assert.Fail();
        }

        [TestMethod()]
        public void TestVariableList()
        {
            // Test design parsing and html output
            Design design = TestDesign("VarList.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 2) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<IndependentVariable>(design, "a", false)) Assert.Fail();
            if (!TestVariable<IndependentVariable>(design, "b", true)) Assert.Fail();
        }

        [TestMethod()]
        public void TestBasicAssignment()
        {
            // Test design parsing and html output
            Design design = TestDesign("BasicAssignment.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 2) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<DependentVariable>(design, "a", false)) Assert.Fail();
            if (!TestVariable<IndependentVariable>(design, "b", false)) Assert.Fail();

            // Test expressions
            if (design.Database.Expressions.Count != 1 || !design.Database.Expressions.ContainsKey("a") || design.Database.Expressions["a"] != "b")
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestBasicNotOperation()
        {
            // Test design parsing and html output
            Design design = TestDesign("BasicNot.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 2) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<DependentVariable>(design, "a", true)) Assert.Fail();
            if (!TestVariable<IndependentVariable>(design, "b", false)) Assert.Fail();
        }

        [TestMethod()]
        public void TestBasicAndOperation()
        {
            // Test design parsing and html output
            Design design = TestDesign("BasicAnd.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 8) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<DependentVariable>(design, "e", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "f", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "g", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "h", true)) Assert.Fail();
        }

        [TestMethod()]
        public void TestBasicOrOperation()
        {
            // Test design parsing and html output
            Design design = TestDesign("BasicOr.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 8) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<DependentVariable>(design, "e", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "f", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "g", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "h", true)) Assert.Fail();
        }

        [TestMethod()]
        public void TestBasicXorOperation()
        {
            // Test design parsing and html output
            Design design = TestDesign("BasicXor.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 8) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<DependentVariable>(design, "e", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "f", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "g", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "h", false)) Assert.Fail();
        }

        [TestMethod()]
        public void TestBasicEqualTo()
        {
            // Test design parsing and html output
            Design design = TestDesign("BasicEqualTo.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 8) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<DependentVariable>(design, "e", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "f", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "g", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "h", true)) Assert.Fail();
        }

        [TestMethod()]
        public void TestOperationPrecedence()
        {
            // Test design parsing and html output
            Design design = TestDesign("OperationPrecedence.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 14) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<DependentVariable>(design, "g", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "h", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "i", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "j", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "k", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "l", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "m", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "n", true)) Assert.Fail();
        }

        [TestMethod()]
        public void TestOperationPrecedence2()
        {
            // Test design parsing and html output
            Design design = TestDesign("OperationPrecedence2.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 24) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<DependentVariable>(design, "i", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "j", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "k", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "l", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "m", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "n", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "o", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "p", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "q", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "r", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "s", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "t", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "u", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "v", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "w", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "x", true)) Assert.Fail();
        }

        [TestMethod()]
        public void TestOperationPrecedence3()
        {
            // Test design parsing and html output
            Design design = TestDesign("OperationPrecedence3.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 24) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<DependentVariable>(design, "i", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "j", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "k", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "l", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "m", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "n", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "o", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "p", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "q", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "r", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "s", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "t", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "u", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "v", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "w", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "x", true)) Assert.Fail();
        }

        [TestMethod()]
        public void TestOperationPrecedence4()
        {
            // Test design parsing and html output
            Design design = TestDesign("OperationPrecedence4.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 14) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<DependentVariable>(design, "g", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "h", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "i", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "j", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "k", false)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "l", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "m", true)) Assert.Fail();
            if (!TestVariable<DependentVariable>(design, "n", true)) Assert.Fail();
        }

        public void TestHorizontalExpansion()
        {
            // Test design parsing and html output
            Design design = TestDesign("HorizontalExpansion.vbi");
            if (design == null) Assert.Fail();

            // Test variable count
            if (design.Database.AllVars.Count != 7) Assert.Fail();

            // Test variable types and values
            if (!TestVariable<IndependentVariable>(design, "a1", false)) Assert.Fail();
            if (!TestVariable<IndependentVariable>(design, "a0", false)) Assert.Fail();
            if (!TestVariable<IndependentVariable>(design, "b1", true)) Assert.Fail();
            if (!TestVariable<IndependentVariable>(design, "b0", true)) Assert.Fail();
            if (!TestVariable<IndependentVariable>(design, "c2", false)) Assert.Fail();
            if (TestVariable<IndependentVariable>(design, "c1", false)) Assert.Fail(); // Should not be created
            if (!TestVariable<IndependentVariable>(design, "c0", false)) Assert.Fail();
            if (!TestVariable<IndependentVariable>(design, "d1", false)) Assert.Fail();
            if (TestVariable<IndependentVariable>(design, "d0", false)) Assert.Fail(); // Should not be created
        }
    }
}
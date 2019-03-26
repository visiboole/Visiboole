using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisiBoole.Models;
using VisiBoole.ParsingEngine;
using VisiBoole.ParsingEngine.ObjectCode;

namespace VisibooleTests.ParsingEngine
{
    [TestClass()]
    public class ParsingTests
    {
        private bool TestDesign(string name)
        {
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            string fileName = Path.Combine(path, "Resources", "Testing Files", name);
            Design design = new Design(fileName, delegate { });
            Parser parser = new Parser();
            try
            {
                return parser.Parse(design, null, false) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [TestMethod()]
        public void TestEmptyDesign()
        {
            if (!TestDesign("Empty.vbi"))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestBasicVariableList()
        {
            if (!TestDesign("BasicVarList.vbi"))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestBasicBooleanExpression()
        {
            if (!TestDesign("BasicBoolExp.vbi"))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestBasicClockExpression()
        {
            if (!TestDesign("BasicClockExp.vbi"))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression()
        {
            if (!TestDesign("CompBoolExp.vbi"))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression2()
        {
            if (!TestDesign("CompBoolExp2.vbi"))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression3()
        {
            if (!TestDesign("CompBoolExp3.vbi"))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression4()
        {
            if (!TestDesign("CompBoolExp4.vbi"))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression5()
        {
            if (!TestDesign("CompBoolExp5.vbi"))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression6()
        {
            if (!TestDesign("CompBoolExp6.vbi"))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression7()
        {
            if (!TestDesign("CompBoolExp7.vbi"))
            {
                Assert.Fail();
            }
        }
        [TestMethod()]
        public void TestComplexBooleanExpression8()
        {
            if (!TestDesign("CompBoolExp8.vbi"))
            {
                Assert.Fail();
            }
        }
        [TestMethod()]
        public void TestComplexBooleanExpression9()
        {
            if (!TestDesign("CompBoolExp9.vbi"))
            {
                Assert.Fail();
            }
        }
    }
}

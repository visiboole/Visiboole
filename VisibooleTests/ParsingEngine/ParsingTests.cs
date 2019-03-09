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
        [TestMethod()]
        public void TestEmptySubdesign()
        {
            string filename = "EmptyTestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, false);
            }
            catch(Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestBasicVariableList()
        {
            string filename = "BasicVarListTestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestBasicBooleanExpression()
        {
            string filename = "BasicBoolExpTestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestBasicClockExpression()
        {
            string filename = "BasicClockTestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression0()
        {
            string filename = "Comp0TestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression1()
        {
            string filename = "Comp1TestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression2()
        {
            string filename = "Comp2TestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression3()
        {
            string filename = "Comp3TestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression4()
        {
            string filename = "Comp4TestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression5()
        {
            string filename = "Comp5TestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TestComplexBooleanExpression6()
        {
            string filename = "Comp6TestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }
        [TestMethod()]
        public void TestComplexBooleanExpression7()
        {
            string filename = "Comp7TestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }
        [TestMethod()]
        public void TestComplexBooleanExpression8()
        {
            string filename = "Comp8TestingVisiboole.vbi";
            Design subDesign = new Design(filename, delegate { });
            Parser parser = new Parser();
            try
            {
                parser.Parse(subDesign, null, new bool());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }
    }
}

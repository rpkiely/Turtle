using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace TestMyTurtle
{
    public class TestBase
    {

        [SetUp]
        public void Test_SetUp()
        {
            TestContext.Out.WriteLine("Test Method: " + TestContext.CurrentContext.Test.MethodName);
        }


        [TearDown]
        public void Test_TearDown()
        {
            TestContext.Out.WriteLine("Test Outcome: " + TestContext.CurrentContext.Result.Outcome);
            TestContext.Out.WriteLine("Test Message: " + TestContext.CurrentContext.Result.Message);
        }



    }
}

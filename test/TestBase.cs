using Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ImageMetadataUT
{
    [TestClass]
    public class TestBase
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            LogSource.SetSourceLevel(string.Empty, SourceLevels.Verbose);
            LogSource.AddDefaultListener(new ConsoleTraceListener());
        }

        protected static void AssertException(Action action, Type exceptionType)
        {
            bool exceptionThrown = false;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                Assert.IsTrue(ex.GetType() == exceptionType);
            }

            Assert.IsTrue(exceptionThrown);
        }
    }
}

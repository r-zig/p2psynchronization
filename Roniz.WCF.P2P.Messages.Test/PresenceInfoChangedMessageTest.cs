using Roniz.WCF.P2P.Messages.Presence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Roniz.WCF.P2P.Messages.Test
{
    
    
    /// <summary>
    ///This is a test class for PresenceInfoChangedMessageTest and is intended
    ///to contain all PresenceInfoChangedMessageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PresenceInfoChangedMessageTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for ToString with default empty values
        ///</summary>
        [TestMethod()]
        public void ToStringDefaultEmptyTest()
        {
            PresenceInfoChangedMessage target = new PresenceInfoChangedMessage();
            string expected =
                "CurrentHopCount: 2147483647, OriginalHopCount: 2147483647, HopCount: 2147483647, HopCountDistance: 0, IsNeighbourMessage: False, IsOwnMessage: True , PresenceInfo: null";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }
    }
}

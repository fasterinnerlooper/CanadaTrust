using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BankLocator.Models;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BankLocator_Test
{
    [TestClass]
    public class TestBranch
    {
        Branch sut;

        [TestInitialize]
        public void SetUp()
        {
            sut = new Branch();
        }

        [TestMethod]
        public void testShouldGetFormattedBranchID()
        {
            sut.BranchID = 0;
            Assert.AreEqual("0000", sut.getFormattedBranchID());
        }
    }
}

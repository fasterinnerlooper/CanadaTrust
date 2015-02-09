using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankLocator.Models;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BankLocator_Test
{
    [TestClass]
    public class TestTDLocatorRequest
    {
        TDLocatorRequest sut;

        [TestInitialize]
        public void SetUp()
        {
            sut = new TDLocatorRequest();
        }

        [TestMethod]
        public void TestShouldThrowErrorOnEmptyAddress()
        {
            Assert.ThrowsException<Exception>(() => { sut.compileUri(); });
        }

        [TestMethod]
        public void TestShouldReturnAddressInUri()
        {
            sut.FullAddress = "Test Address";
            Uri responseUri = sut.compileUri();
            Assert.IsTrue(responseUri.AbsoluteUri.Contains("Test%20Address"),"Output: " + responseUri.AbsoluteUri);
        }
    }
}

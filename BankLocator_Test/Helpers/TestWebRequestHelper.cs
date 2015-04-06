using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BankLocator_Common.Helpers;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BankLocator_Test.Helpers
{
    [TestClass]
    public class TestWebRequestHelper
    {
        WebRequestHelper sut;
        AutoResetEvent testLock;

        [TestInitialize]
        public void SetUp()
        {
            sut = new WebRequestHelper("http://www.google.com");
            testLock = new AutoResetEvent(false);
        }

        [TestMethod]
        public void TestShouldCreateWebRequestHelper()
        {
            Assert.AreEqual("http://www.google.com/", sut.RequestUri.AbsoluteUri);
        }

        [TestMethod]
        public void TestShouldCreateCookieContainer()
        {
            Assert.IsNotNull(sut.CookieContainer);
        }
        private bool isCompleted;

        [TestMethod]
        public void TestShouldBeginWebRequest()
        {
            sut.BeginWebRequest(new AsyncCallback(webRequestCallback));
            testLock.WaitOne();

            Assert.IsTrue(isCompleted);
        }

        private void webRequestCallback(IAsyncResult result)
        {
            isCompleted = result.IsCompleted;
            testLock.Set();
        }
    }
}

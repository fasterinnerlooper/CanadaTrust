using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankLocator_Common.Locators;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BankLocator_Test.Integration
{
    [TestClass]
    public class TestBMOLocator
    {
        BMOLocator sut;
        
        [TestInitialize]
        public void SetUp()
        {
            sut = new BMOLocator();
        }
        
        [TestMethod]
        public async Task ShouldCorrectlySendRequest()
        {
            var latitude = 43.656840;
            var longitude = -79.384190;
            sut.SetLocation(latitude, longitude);
            sut.InitializeHttpContent();
            await sut.BeginHttpClientRequest();
            Assert.IsTrue(sut.HttpResponseMessage.IsSuccessStatusCode);
        }
    }
}

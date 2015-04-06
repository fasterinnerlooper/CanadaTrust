using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankLocator_Common.Helpers;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BankLocator_Test.Helpers
{
    [TestClass]
    public class TestTrackingHelper
    {
        [TestMethod]
        public void ShouldCorrectlyCalculateDistance()
        {
            var loc1 = new Location() {Latitude = 51.01, Longitude = -114};
            var loc2 = new Location() {Latitude = 51.02, Longitude = -114};
            var result = TrackingHelper.CalculateDistance(loc1, loc2);
            Assert.IsTrue(result > 1);
            Assert.IsTrue(result < 1.2);
        }
    }
}

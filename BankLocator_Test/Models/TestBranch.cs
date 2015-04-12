using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BankLocator.Models;
using BankLocator_Common.Models;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;

namespace BankLocator_Test.Models
{
    [TestClass]
    public class TestBranch
    {
        Branch sut;
        List<BMOBranch> branches;
        string json = @"{""d"":[{""Name"":""TOWER LANE MALL SAFEWAY"",""AbmCount"":1,""IsBranch"":false,""WheelChair"":false,""Hours"":[],""AbmHours"":["" 8-11"","" 8-11"","" 8-11"","" 8-11"","" 8-11"","" 8-11"","" 8-11""],""OpenHoursDescription"":""Mon-Sun 8-11"",""ClosedHoursDescription"":"""",""TellerServices"":false,""Region"":"" "",""SafetyDepositLarge"":0,""SafetyDepositMedium"":0,""SafetyDepositSmall"":0,""BranchCentreType"":"""",""ModelNumber"":""M2100"",""CustomBranchFeatures"":"""",""CustomBranchFeatures_French"":null,""UniqueId"":17350,""Id"":676,""Transit"":""676"",""SecondaryId"":""A465"",""StreetNumber"":""505"",""Unit"":"""",""Street"":""MAIN STREET SOUTH"",""City"":""AIRDRIE"",""ProvinceState"":""AB"",""PostalCode"":""T4B3K3"",""Location"":{""__type"":""Infusion.BMO.VirtualEarth.Locators.BusinessLogicLayer.BusinessObjects.LatLong"",""Latitude"":51.2880211,""Longitude"":-114.0136414},""Properties"":[{""Key"":""name"",""Value"":""TOWER LANE MALL SAFEWAY""},{""Key"":""safedepositlarge"",""Value"":""0""},{""Key"":""safedepositmedisum"",""Value"":""0""},{""Key"":""abmcount"",""Value"":""1""},{""Key"":""safedepositsmall"",""Value"":""0""},{""Key"":""wheelchair"",""Value"":""False""},{""Key"":""tellerservices"",""Value"":""False""},{""Key"":""region"",""Value"":"" ""}],""Phone"":"""",""Fax"":"""",""Languages"":[]}, {""Name"":""TELUS TOWER (AGT TOWER)"",""AbmCount"":1,""IsBranch"":true,""WheelChair"":true,""Hours"":["" 8:30-5"","" 8:30-5"","" 8:30-5"","" 8:30-5"","" 8:30-5"","""",""""],""AbmHours"":["" 24 h"","" 24 h"","" 24 h"","" 24 h"","" 24 h"","" 24 h"","" 24 h""],""OpenHoursDescription"":""Mon-Fri 8:30-5"",""ClosedHoursDescription"":""Sat-Sun"",""TellerServices"":true,""Region"":""9"",""SafetyDepositLarge"":1,""SafetyDepositMedium"":1,""SafetyDepositSmall"":1,""BranchCentreType"":""FULLSRV"",""ModelNumber"":""M2150"",""CustomBranchFeatures"":"""",""CustomBranchFeatures_French"":null,""UniqueId"":8535,""Id"":2512,""Transit"":""2512"",""SecondaryId"":""A201"",""StreetNumber"":"""",""Unit"":"""",""Street"":""411 1ST ST SE"",""City"":""CALGARY"",""ProvinceState"":""AB"",""PostalCode"":""T2G4Y5"",""Location"":{""__type"":""Infusion.BMO.VirtualEarth.Locators.BusinessLogicLayer.BusinessObjects.LatLong"",""Latitude"":51.048792,""Longitude"":-114.061291},""Properties"":[{""Key"":""name"",""Value"":""TELUS TOWER (AGT TOWER)""},{""Key"":""safedepositlarge"",""Value"":""1""},{""Key"":""safedepositmedisum"",""Value"":""1""},{""Key"":""abmcount"",""Value"":""1""},{""Key"":""safedepositsmall"",""Value"":""1""},{""Key"":""wheelchair"",""Value"":""True""},{""Key"":""tellerservices"",""Value"":""True""},{""Key"":""region"",""Value"":""9""}],""Phone"":""4032341800"",""Fax"":""4035037169"",""Languages"":[""English"",""Cantonese"",""Mandarin""]}]}";

        [TestInitialize]
        public void SetUp()
        {
            sut = new Branch();
            var rootObject = JsonConvert.DeserializeObject<RootObject>(json);
            branches = rootObject.d;
        }

        [TestMethod]
        public void ShouldCaptureAllFieldsWhenCreatingAnATMBranch()
        {
            var branch = branches[0];
            Assert.IsTrue(branch.Name == "TOWER LANE MALL SAFEWAY");
            Assert.IsTrue(branch.getFormattedBranchID() == "676");
            Assert.IsTrue(branch.Address == "505 MAIN STREET SOUTH");
            Assert.IsTrue(branch.AddressLine2 == "AIRDRIE, AB, T4B3K3");
            Assert.AreEqual(branch.OpenHoursDescription, "Mon-Sun 8-11");
            CollectionAssert.AreEqual(branch.Hours, new List<string>());
            CollectionAssert.AreEqual(branch.AbmHours,  new List<string>() {" 8-11"," 8-11"," 8-11"," 8-11"," 8-11"," 8-11"," 8-11"});
            Assert.IsTrue(String.IsNullOrEmpty(branch.Phone));
            Assert.IsFalse(branch.IsBranch);
            Assert.AreEqual(1, branch.AbmCount);
            Assert.AreEqual("8-11", branch.AbmHours.First().Trim());
        }

        [TestMethod]
        public void ShouldCaptureAllFieldsWhenCreatingAPhysicalBranch()
        {
            var branch = branches[1];
            Assert.AreEqual("TELUS TOWER (AGT TOWER)", branch.Name);
            Assert.AreEqual("2512", branch.getFormattedBranchID());
            Assert.AreEqual("411 1ST ST SE", branch.Address);
            Assert.AreEqual("CALGARY, AB, T2G4Y5", branch.AddressLine2);
            Assert.AreEqual(branch.OpenHoursDescription, "Mon-Fri 8:30-5");
            CollectionAssert.AreEqual(branch.Hours, new List<string>() {" 8:30-5", " 8:30-5", " 8:30-5", " 8:30-5", " 8:30-5", "", ""});
            CollectionAssert.AreEqual(branch.AbmHours, new List<string>() { " 24 h", " 24 h", " 24 h", " 24 h", " 24 h", " 24 h", " 24 h" });
            Assert.AreEqual("4032341800", branch.Phone);
            Assert.IsTrue(branch.IsBranch);
            Assert.AreEqual(1, branch.AbmCount);
            Assert.AreEqual("24 h", branch.AbmHours.First().Trim());
        }

        [TestMethod]
        public void testShouldGetFormattedBranchID()
        {
            sut.BranchID = 0;
            Assert.AreEqual("0000", sut.getFormattedBranchID());
        }
    }
}

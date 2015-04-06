using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BankLocator.Models;

namespace BankLocator_Common.Models
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class Property
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class BMOBranch : Branch
    {
        public string Name { get; set; }
        public int AbmCount { get; set; }
        public bool IsBranch { get; set; }
        public bool WheelChair { get; set; }
        public List<string> Hours { get; set; }
        public List<string> AbmHours { get; set; }
        public string OpenHoursDescription { get; set; }
        public string ClosedHoursDescription { get; set; }
        public bool TellerServices { get; set; }
        public string Region { get; set; }
        public int SafetyDepositLarge { get; set; }
        public int SafetyDepositMedium { get; set; }
        public int SafetyDepositSmall { get; set; }
        public string BranchCentreType { get; set; }
        public string ModelNumber { get; set; }
        public string CustomBranchFeatures { get; set; }
        public object CustomBranchFeatures_French { get; set; }
        public int UniqueId { get; set; }
        public int Id { get; set; }
        public string Transit { get; set; }
        public string SecondaryId { get; set; }
        public string StreetNumber { get; set; }
        public string Unit { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ProvinceState { get; set; }
        public string PostalCode { get; set; }
        public Location Location { get; set; }
        public List<Property> Properties { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public List<string> Languages { get; set; }
        //Branch base properties
        public int MapID { get; set; }
        public string getFormattedBranchID()
        {
            return Transit;
        }
        public string Address
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(Unit)) sb.Append(Unit + " ");
                if (!string.IsNullOrEmpty(StreetNumber)) sb.Append(StreetNumber + " ");
                if (!string.IsNullOrEmpty(Street)) sb.Append(Street);
                return sb.ToString().TrimEnd();
            }

        }
        public string AddressLine2
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(City)) sb.Append(City + ", ");
                if (!string.IsNullOrEmpty(ProvinceState)) sb.Append(ProvinceState + ", ");
                if (!string.IsNullOrEmpty(PostalCode)) sb.Append(PostalCode);
                return sb.ToString().TrimEnd();
            }
        }
        public string PhoneNumber
        {
            get
            {
                return Phone;
            }
        }
        public string Distance { get; set; }
    }

    public class RootObject
    {
        public List<BMOBranch> d { get; set; }
    }
}

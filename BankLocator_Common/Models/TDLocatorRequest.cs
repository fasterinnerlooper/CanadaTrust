using System;
using System.Net;
using System.Windows;

namespace BankLocator.Models
{
    public class TDLocatorRequest
    {
        private string requestURI = "http://td.via.infonow.net/locator/AdvancedSearchAction.do";
        private string dummyFullAddress;
        public string FullAddress
        {
            get { return dummyFullAddress; }
            set
            {
                dummyFullAddress = Uri.EscapeDataString(value);
            }
        }
        private string fullAddress = "";

        public TDLocatorRequest()
        {
        }

        public Uri compileUri() {
            if(string.IsNullOrEmpty(dummyFullAddress)) {
                throw new Exception("The address value has not been set");
            }
            string querystring = "dummyFullAddress=" + dummyFullAddress + "&dummyBranchNumber=&abmSearch=Abm&abmSearch=&branchSearch=Branch&branchSearch=&searchCustom__openSat=&searchCustom__openSun=&searchCustom__coinCounter=&searchCustom__openAfterSix=&searchCustom__openSunPresent=FALSE&searchCustom__openSatPresent=TRUE&searchCustom__openSunPresent=FALSE&searchCustom__wheelType=&fullAddress=" + dummyFullAddress + "&originalSearchValue=" + dummyFullAddress + "&originalBranchNumber=&searchType=fullAddress&country=Canada&mapAndList=mapAndList&searchCustom__transitNumber=&searchCustom__searchBy=";
            return new Uri(requestURI + "?" + querystring);
        }
    }
}

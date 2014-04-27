using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CanadaTrustv1.Models
{
    public class TDLocatorRequest
    {
        private string requestURI = "http://td.via.infonow.net/locator/AdvancedSearchAction.do";
        private string fullAddress = null;
        public string FullAddress
        {
            get
            {
                return fullAddress;
            }
            set
            {
                fullAddress = HttpUtility.UrlEncode(value);
            }
        }

        public TDLocatorRequest()
        {
        }

        public Uri compileUri()
        {
            string querystring = "dummyFullAddress=" + fullAddress + "&branchSearch=Branch&branchSearch=&abmSearch=Abm&abmSearch=&searchCustom__openSat=&searchCustom__openSun=&searchCustom__coinCounter=&searchCustom__openAfterSix=&searchCustom__openSunPresent=FALSE&searchCustom__openSatPresent=TRUE&searchCustom__openSunPresent=FALSE&searchCustom__wheelType=&fullAddress=" + fullAddress + "&originalSearchValue=&country=Canada&mapAndList=mapAndList&searchCustom__transitNumber=&searchCustom__searchBy=&.x=29&.y=6";
            return new Uri(requestURI + "?" + querystring);
        }
    }
}

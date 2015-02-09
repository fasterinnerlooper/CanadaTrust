using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BankLocator_Common.Helpers;

namespace BankLocator_Common.Locators
{
    public class TDLocator
    {
        private Uri requestUri;

        public TDLocator(Uri uri)
        {
            requestUri = uri;
        }

        public void SetLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        private WebRequestHelper httpWebRequestHelper = null;
        public WebRequestHelper HttpWebRequestHelper
        {
            get
            {
                if (HttpWebRequestHelper == null)
                {
                    throw new Exception("HttpWebRequest object has not been initialized");
                }
                return HttpWebRequestHelper;
            }
            set
            {
                httpWebRequestHelper = value;
            }
        }

        public void BeginWebRequest(AsyncCallback callback)
        {
            var webRequestHelper1 = new WebRequestHelper("http://td.via.infonow.net/locator");
            webRequestHelper1.BeginWebRequest(delegate { }); //We want to start the web request, but we just want the cookie
            httpWebRequestHelper = new WebRequestHelper(requestUri);
            httpWebRequestHelper.CookieContainer = webRequestHelper1.CookieContainer;
            httpWebRequestHelper.BeginWebRequest(callback);
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

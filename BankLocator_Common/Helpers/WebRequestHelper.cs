using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BankLocator_Common.Helpers
{
    public class WebRequestHelper
    {
        public WebRequestHelper(string uri) : this(new Uri(uri)) {}

        public WebRequestHelper(Uri uri)
        {
            RequestUri = uri;
            CookieContainer = new CookieContainer();
        }

        public Uri RequestUri { get; private set; }
        public CookieContainer CookieContainer { get; set; }

        public void BeginWebRequest(AsyncCallback callback)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(this.RequestUri.AbsoluteUri);
            webRequest.CookieContainer = this.CookieContainer;
            webRequest.BeginGetResponse(callback, webRequest);
        }
    }
}

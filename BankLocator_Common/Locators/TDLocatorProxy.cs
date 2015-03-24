using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BankLocator.Models;
using BankLocator_Common.Helpers;
using HtmlAgilityPack;

namespace BankLocator_Common.Locators
{
    public class TDLocatorProxy : ILocator
    {
        private TDLocator locator;
        private TDLocatorRequest locatorRequest;
        private object dispatcher;

        public TDLocatorProxy(Uri uri)
        {
            locator = new TDLocator();
            locatorRequest = new TDLocatorRequest();
            locator.BeginWebRequest(WebRequestCallback);
            locator.setUri(uri);
        }

        public void CompileUri(Dictionary<string, string> address) {

            locatorRequest.FullAddress = String.Format("{0} {1}, {2}, {3}", address["HouseNumber"],
                                                       address["Street"], address["City"], address["Province"]);
            Uri compiledUri = locatorRequest.compileUri();
            locator.setUri(compiledUri);
        }
        
        private void WebRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

            using (Stream stream = response.GetResponseStream())
            {
                var doc = new HtmlDocument();
                doc.Load(stream);
                HtmlNode error = doc.DocumentNode.SelectSingleNode("//div[@class='copyerror']");
                if (error != null)
                {
                    //Are we in the US? Check the first.
                    //this.dispatcher.BeginInvoke(() =>
                    //{
                    //    MessageBox.Show("The service is unavailable, please try again later");
                    //});
                    //return;
                }
                HtmlNode mapUrl = doc.DocumentNode.SelectSingleNode("//img[@usemap='#pins']");
                string mapUrLstring = mapUrl.Attributes["src"].Value;

                MatchCollection matches = RegexHelper.urlStringRegex.Matches(mapUrLstring);
                //CreateBranchesCollection(doc, matches);
            }
        }

        public Task BeginHttpClientRequest()
        {
            return new Task(() => { this.BeginHttpClientRequest(null); });
        }

        public void BeginHttpClientRequest(AsyncCallback callback)
        {
            locator.BeginWebRequest(callback);
        }

        public List<Branch> Branches
        {
            get
            {
                if (this.Branches == null)
                {
                    throw new NullReferenceException("The branches collection has not been intitalised");
                }
                else {
                    return this.Branches;
                }
            }

            set {
                this.Branches = value;
            }
        }
            
        public Task CreateBranches()
        {
            throw new NotImplementedException();
        }

        public System.Net.Http.HttpClient HttpClient
        {
            get { throw new NotImplementedException(); }
        }

        public System.Net.Http.StringContent HttpContent
        {
            get { throw new NotImplementedException(); }
        }

        public System.Net.Http.HttpResponseMessage HttpResponseMessage
        {
            get { throw new NotImplementedException(); }
        }

        public void InitializeHttpContent()
        {
            httpContent = new StringContent(String.Format(requestBody, Latitude, Longitude), Encoding.UTF8, "application/json");
        }

        public double Latitude
        {
            get
            {
                return this.locator.Latitude;
            }
            set
            {
                this.locator.Latitude = value;
            }
        }

        public double Longitude
        {
            get
            {
                return this.locator.Longitude;
            }
            set
            {
                this.locator.Longitude = value;
            }
        }

        public void SetLocation(double latitude, double longitude)
        {
            this.locator.SetLocation(latitude, longitude);
        }

        public Models.RootObject translateJson(string json)
        {
            throw new NotImplementedException();
        }

        List<Models.BMOBranch> ILocator.Branches
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void setDispatcher(object Dispatcher)
        {
            this.dispatcher = Dispatcher;
        }
    }
}

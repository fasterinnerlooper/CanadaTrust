using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BankLocator_Common.Models;
using System.Net;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace BankLocator_Common.Locators
{
    public class BMOLocator
    {
        private string requestString = "http://locator.bmo.com/LocatorService.asmx/SearchLocations";
        private string requestBody = @"{{""searchSpec"":{{""Type"":""BMOBranch"",""Location"":{{""Latitude"":{0},""Longitude"":{1},""Altitude"":null,""AltitudeMode"":null,""_reserved"":null}},""DisplayLanguage"":""English"",""Subdivision"":null}}}}";

        public RootObject translateJson(string json)
        {
            return JsonConvert.DeserializeObject<RootObject>(json);
        }

        public void SetLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        private StringContent httpContent = null;
        public StringContent HttpContent
        {
            get
            {
                if(httpContent==null)
                {
                    throw new Exception("HttpContent object has not been initialized");
                }
                return httpContent;
            }
            private set
            {
                httpContent = value;
            }
        }
        public void InitializeHttpContent()
        {
            httpContent = new StringContent(String.Format(requestBody, Latitude, Longitude), Encoding.UTF8, "application/json");
         }

        public async Task BeginHttpClientRequest()
        {
            httpClient = new HttpClient();
            httpResponseMessage = await httpClient.PostAsync(requestString, httpContent);
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        private HttpResponseMessage httpResponseMessage;
        public HttpResponseMessage HttpResponseMessage { get
            {
                if (httpResponseMessage == null) {
                    throw new Exception("HttpResponseMessage has not been initialized");
                }
                return httpResponseMessage;
            }
            private set
            {
                httpResponseMessage = value;
            }
        }
        private HttpClient httpClient = null;
        public HttpClient HttpClient { get {
            if (httpClient == null)
            {
                throw new Exception("Initialization of the HttpWebRequest object has not been run");
            }
            return httpClient;
        }
            private set
            {
                httpClient = value;
            }
        }

        public async Task CreateBranches()
        {
            string jsonText = await httpResponseMessage.Content.ReadAsStringAsync();
            var rootObject = JsonConvert.DeserializeObject<RootObject>(jsonText);
            var localBranches = rootObject.d;
            Branches = rootObject.d;
        }

        public List<BMOBranch> Branches { get; set; }
    }
}

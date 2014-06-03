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
using HtmlAgilityPack;
using CanadaTrustv1.Models;

namespace CanadaTrustv1.Factory
{
    public class TDBranchFactory
    {
        private static Uri locationURI = new Uri("http://td.via.infonow.net/locator/NewSearch.do");

        public static Branch createBranch(int branchNumber)
        {
            HtmlWeb web = new HtmlWeb();
            web.LoadAsync(string.Format(locationURI.ToString(), branchNumber));
            web.LoadCompleted += (s, e) =>
            {
                if (e.Error == null)
                {
                    HtmlDocument doc = e.Document;
                    HtmlNode address = doc.DocumentNode.SelectSingleNode("//table[@class='element']/td[@td='table']/td");
                    address.ToString();
                }
            };
            return new Branch()
            {
                BranchID = 205,
                Address = "1411 1st Street NE, Calgary, AB, T2E T2T",
                Name = "TD North Hill Centre",
                Hours = "Mon-Fri 10-10",
                Location = new System.Device.Location.GeoCoordinate { Latitude = 85, Longitude = 141 },
                PhoneNumber = "(403) 123-4567"
            };
        }
    }
}

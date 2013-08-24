using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using CanadaTrustv1.BingMapsGeocodeService;
using System.Collections.ObjectModel;
using System.Device.Location;
using CanadaTrustv1.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using Microsoft.Phone.Tasks;

namespace CanadaTrustv1
{
    public partial class MapPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher coordinateWatcher;
        public ObservableCollection<Branch> Branches;
        TDLocatorRequest locatorRequest = new TDLocatorRequest();
        string key = "AguTswrw5_cJGU7-8BVfFOYmGnZMXHvwz44VZLMEinF9oKLxeYlO9I9jdzoR_bk8";
        Uri locationLookupURI = new Uri("http://td.via.infonow.net/locator/NewSearch.do");
        string currentAddress;
        GeoCoordinate lastLocation = new GeoCoordinate();
        Regex URLStringRegex, mapIDNumberRegex, BranchNumberRegex, PhoneNumberRegex, AddressRegex, HoursRegex, DistanceRegex;
        Pushpin centreLocation;

        public MapPage()
        {
            InitializeComponent();
            coordinateWatcher = new GeoCoordinateWatcher();
            coordinateWatcher.MovementThreshold = 50;

            coordinateWatcher.PositionChanged += (s, e) =>
            {
                if (lastLocation != coordinateWatcher.Position.Location)
                {
                    mapLoading.Visibility = System.Windows.Visibility.Visible;
                    setLocation(coordinateWatcher.Position.Location.Latitude,
                                coordinateWatcher.Position.Location.Longitude,
                                10,
                                true);
                    lastLocation = coordinateWatcher.Position.Location;
                }
            };
        }

        public string Key { get { return key; } }

        private void setLocation(double latitude, double longitude, double zoomLevel, bool showLocation)
        {
            System.Device.Location.GeoCoordinate location = new System.Device.Location.GeoCoordinate();
            location.Latitude = latitude;
            location.Longitude = longitude;
            ReverseGeocodeRequest reverseGeocodeRequest = new ReverseGeocodeRequest();
            reverseGeocodeRequest.Credentials = new Credentials();
            reverseGeocodeRequest.Credentials.ApplicationId = key;
            reverseGeocodeRequest.Location = location;
            GeocodeServiceClient geocodeService = null;
            try
            {
                geocodeService = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
                geocodeService.ReverseGeocodeAsync(reverseGeocodeRequest);
                geocodeService.ReverseGeocodeCompleted += geocodeService_ReverseGeocodeCompleted;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void geocodeService_ReverseGeocodeCompleted(object sender, ReverseGeocodeCompletedEventArgs e)
        {
            ReverseGeocodeCompletedEventArgs response = (ReverseGeocodeCompletedEventArgs)e;
            GeocodeResponse geocodeResponse = response.Result;
            foreach (GeocodeResult result in geocodeResponse.Results)
            {
                if (result == null || result.Address.AddressLine == "")
                {
                    continue;
                }
                currentAddress = result.Address.FormattedAddress;
                locatorRequest.FullAddress = currentAddress;
                Uri compiledUri = locatorRequest.compileUri();
                callWebsite(compiledUri);
                break;
            }
        }
        private void callWebsite(Uri uri)
        {
            bingMap.Children.Clear();
            HtmlWeb web = new HtmlWeb();
            web.LoadAsync(uri.ToString());
            web.LoadCompleted += (s, e) =>
            {
                if (e.Error == null)
                {
                    HtmlDocument doc = e.Document;
                    HtmlNode error = doc.DocumentNode.SelectSingleNode("//div[@class='copyerror']");
                    if (error != null)
                    {
                        MessageBox.Show("The service is unavailable, please try again later");
                        return;
                    }
                    HtmlNode mapURL = doc.DocumentNode.SelectSingleNode("//img[@usemap='#pins']");
                    string mapURLstring = mapURL.Attributes["src"].Value;

                    setupRegex();

                    Branches = new ObservableCollection<Branch>();

                    MatchCollection matches = URLStringRegex.Matches(mapURLstring);
                    createBranchesCollection(doc, matches);
                    setupCentrePushpin();
                    addPushpinsToMap();
                    setupMapViewport();
                    mapLoading.Visibility = System.Windows.Visibility.Collapsed;
                }
            };
        }

        private void addPushpinsToMap()
        {
            foreach (Branch branch in Branches)
            {
                Pushpin pushpin = new Pushpin()
                {
                    Location = branch.Location,
                    Content = branch.Address,
                    Tag = branch.BranchID
                };
                pushpin.Tap += new EventHandler<GestureEventArgs>(pushpin_Tap);
                App.Branches.Add(branch);
                bingMap.Children.Add(pushpin);
            }
        }

        private void createBranchesCollection(HtmlDocument doc, MatchCollection matches)
        {
            for (int i = 2; i <= 6; i++)
            {
                string BranchNumber = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[2]", doc, BranchNumberRegex);
                string HoursUnformatted = MatchHelper.MatchAndReturnHtml("//tr[@class='table'][" + i + "]/td[4]", doc, HoursRegex);
                string HoursFormatted = HoursUnformatted.Replace("<br>", "\n");
                Branch branch = new Branch()
                {
                    MapID = Int32.Parse(MatchHelper.Match("//tr[@class='table'][" + i + "]/td[1]/strong", doc, mapIDNumberRegex)),
                    BranchID = BranchNumber == "" ? 0 : Int32.Parse(BranchNumber),
                    PhoneNumber = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[5]", doc, PhoneNumberRegex),
                    Address = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[2]/strong", doc, AddressRegex),
                    Hours = HoursFormatted,
                    Distance = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[3]", doc, DistanceRegex),
                    Location = new GeoCoordinate
                    {
                        Latitude = double.Parse(matches[i - 1].Groups[1].Value),
                        Longitude = double.Parse(matches[i - 1].Groups[2].Value)
                    }
                };
                Branches.Add(branch);
            }
        }

        private void setupMapViewport()
        {
            double north = Branches[1].Location.Latitude, west = Branches[1].Location.Longitude;
            double south = Branches[1].Location.Latitude, east = Branches[1].Location.Longitude;
            foreach (Branch branch in Branches)
            {
                north = branch.Location.Latitude > north ? branch.Location.Latitude : north;
                south = branch.Location.Latitude < south ? branch.Location.Latitude : south;
                east = branch.Location.Longitude > east ? branch.Location.Longitude : east;
                west = branch.Location.Longitude < west ? branch.Location.Longitude : west;
            }
            bingMap.SetView(new LocationRect(north, west, south, east));
            //TODO: Centre Map using Map's centre and our current position
        }

        private void setupCentrePushpin()
        {
            System.Device.Location.GeoCoordinate locator = new System.Device.Location.GeoCoordinate();
            locator.Latitude = coordinateWatcher.Position.Location.Latitude;
            locator.Longitude = coordinateWatcher.Position.Location.Longitude;
            centreLocation = new Pushpin();
            centreLocation.Location = locator;
            centreLocation.Foreground = new SolidColorBrush(Colors.Black);
            centreLocation.Background = new SolidColorBrush(Colors.Green);
            bingMap.Children.Add(centreLocation);
        }

        private void setupRegex()
        {
            string URLStringPattern = @"pp=([0-9\\.]*),([0-9-\\.]*);[0-9]+;(.)";
            URLStringRegex = new Regex(URLStringPattern, RegexOptions.IgnoreCase);
            string mapIDNumberPattern = @"([0-9])\.";
            mapIDNumberRegex = new Regex(mapIDNumberPattern, RegexOptions.IgnoreCase);
            string BranchNumberPattern = @".*Branch # ([0-9][0-9][0-9][0-9]).*";
            BranchNumberRegex = new Regex(BranchNumberPattern, RegexOptions.IgnoreCase);
            string PhoneNumberPattern = @".*(\([0-9][0-9][0-9]\)[ ]?[0-9][0-9][0-9]\-[0-9][0-9][0-9][0-9]).*";
            PhoneNumberRegex = new Regex(PhoneNumberPattern, RegexOptions.IgnoreCase);
            string AddressPattern = @"(.*)";
            AddressRegex = new Regex(AddressPattern, RegexOptions.IgnoreCase);
            string HoursPattern = @"(Mon.*PM)";
            HoursRegex = new Regex(HoursPattern, RegexOptions.IgnoreCase);
            string DistancePattern = @"([0-9]*\.[0-9]* km)";
            DistanceRegex = new Regex(DistancePattern, RegexOptions.IgnoreCase);
        }

        private void pushpin_Tap(object sender, RoutedEventArgs e)
        {
            Pushpin pushpin = sender as Pushpin;
            App.currentBranch = (int)pushpin.Tag;
            NavigationService.Navigate(new Uri("/MapDetailsPage.xaml", UriKind.Relative));
            mapLoadingTextBlock.Text = "Loading";
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            mapLoading.Visibility = System.Windows.Visibility.Collapsed;
            AdRotatorControl.Invalidate();
            base.OnNavigatedTo(e);
            coordinateWatcher.Start();
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            AdRotatorControl.Dispose();
            base.OnNavigatingFrom(e);
            coordinateWatcher.Stop();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            mapLoadingTextBlock.Text = "Updating";
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            AdRotatorControl.Invalidate();
        }

        private void ApplicationBarIconButton_MainClick(object sender, EventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://www.td.com/w");
            webBrowserTask.Show();
        }

        private void ApplicationBarMenuItem_AboutClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }
    }
}
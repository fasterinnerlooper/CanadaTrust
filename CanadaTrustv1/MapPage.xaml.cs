using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading;

namespace CanadaTrustv1
{
    public partial class MapPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher coordinateWatcher;
        public ObservableCollection<Branch> Branches;
        TDLocatorRequest locatorRequest = new TDLocatorRequest();
        string key = "AuVxcO7q6MuOaSUWkkOpV19yBG0CSv-SaCN7xxfKvgURFNbW36Jyz9rDlgmf72dP ";
        Uri locationLookupURI = new Uri("http://td.via.infonow.net/locator/NewSearch.do");
        string currentAddress;
        GeoCoordinate lastLocation = new GeoCoordinate();
        Regex URLStringRegex, mapIDNumberRegex, BranchNumberRegex, PhoneNumberRegex, AddressRegex, Address2Regex, HoursRegex, DistanceRegex;
        Pushpin centreLocation;
        LocationRect viewportSize = null;
        HtmlDocument doc;
        Dictionary<string, int> iDLookup = new Dictionary<string, int>();

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
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == false)
            {
                MessageBox.Show("There was a problem connecting to the remote location service. Please check your internet connection and try again.", "Location Services problem", MessageBoxButton.OK);
                return;
            }
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
            GeocodeResponse geocodeResponse;
            try
            {
                geocodeResponse = e.Result;
                foreach (GeocodeResult result in geocodeResponse.Results)
                {
                    if (result == null || result.Address.AddressLine == "")
                    {
                        continue;
                    }
                    if (result.Address.CountryRegion == "Canada")
                    {
                        currentAddress = result.Address.FormattedAddress;
                        locatorRequest.FullAddress = currentAddress;
                        Uri compiledUri = locatorRequest.compileUri();
                        iDLookup = new Dictionary<string, int>();
                        viewportSize = null;
                        setupCentrePushpin();
                        callWebsite(compiledUri);
                        break;
                    }
                    MessageBox.Show("This app is not available in your location/region.\nPlease try again later.", "Outside of Canada", MessageBoxButton.OK);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There was a problem connecting to the remote location service. Please check your internet connection and try again.", "Location Services problem", MessageBoxButton.OK);
                return;
            }
        }
        private void fakeWebRequestCallback(IAsyncResult result) { }
        private void webRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

            using (Stream stream = response.GetResponseStream())
            {
                doc = new HtmlDocument();
                doc.Load(stream);
                HtmlNode error = doc.DocumentNode.SelectSingleNode("//div[@class='copyerror']");
                if (error != null)
                {
                    //Are we in the US? Check the first.
                    MessageBox.Show("The service is unavailable, please try again later");
                    return;
                }
                HtmlNode mapURL = doc.DocumentNode.SelectSingleNode("//img[@usemap='#pins']");
                string mapURLstring = mapURL.Attributes["src"].Value;

                setupRegex();

                Branches = new ObservableCollection<Branch>();

                MatchCollection matches = URLStringRegex.Matches(mapURLstring);
                createBranchesCollection(doc, matches);
            }
        }
        private void callWebsite(Uri uri)
        {
            bingMap.Children.Clear();
            HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
            CookieContainer cookieContainer = new CookieContainer();
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("http://td.info.vianow.net/locator");
            webRequest.CookieContainer = cookieContainer;
            webRequest.BeginGetResponse(new AsyncCallback(fakeWebRequestCallback), webRequest);
            HttpWebRequest webRequest2 = (HttpWebRequest)HttpWebRequest.Create(uri.ToString());
            webRequest2.CookieContainer = cookieContainer;
            webRequest2.BeginGetResponse(new AsyncCallback(webRequestCallback), webRequest2);
        }

        private void addPushpinsToMap(Branch branch)
        {
            Pushpin pushpin = new Pushpin()
            {
                Location = branch.Location,
                Content = branch.Address,
                Tag = branch.BranchID
            };
            pushpin.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(pushpin_Tap);
            App.Branches.Add(branch);
            bingMap.Children.Add(pushpin);
        }

        private void createBranchesCollection(HtmlDocument doc, MatchCollection matches)
        {
            Collection<Branch> intBranches = new Collection<Branch>();
            for (int i = 2; i <= 6; i++)
            {
                String address = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[2]/strong", doc, AddressRegex);
                string address2 = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[2]", doc, Address2Regex);
                string BranchNumber = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[2]", doc, BranchNumberRegex);
                string HoursUnformatted = MatchHelper.MatchAndReturnHtml("//tr[@class='table'][" + i + "]/td[4]", doc, HoursRegex);
                string HoursFormatted = HoursUnformatted.Replace("<br>", "\n");
                iDLookup.Add(address, i);
                GeocodeServiceClient geocodeService = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
                GeocodeRequest request = new GeocodeRequest();
                request.Credentials = new Credentials() { ApplicationId = key };
                request.Query = address + " " + address2;
                geocodeService.GeocodeAsync(request);
                geocodeService.GeocodeCompleted += (s, e) =>
                {
                    GeocodeCompletedEventArgs result = e as GeocodeCompletedEventArgs;
                    //try
                    //{
                    int j = iDLookup[address];
                        if (result.Result.Results.Count > 0)
                        {
                            Branch branch = new Branch()
                            {
                                MapID = Int32.Parse(MatchHelper.Match("//tr[@class='table'][" + j + "]/td[1]/strong", doc, mapIDNumberRegex)),
                                BranchID = BranchNumber == "" ? 0 : Int32.Parse(BranchNumber),
                                PhoneNumber = MatchHelper.Match("//tr[@class='table'][" + j + "]/td[5]", doc, PhoneNumberRegex),
                                Address = address,
                                AddressLine2 = address2,
                                Hours = HoursFormatted,
                                Distance = MatchHelper.Match("//tr[@class='table'][" + j + "]/td[3]", doc, DistanceRegex),
                                Location = new GeoCoordinate()
                                {
                                    Latitude = result.Result.Results[0].Locations[0].Latitude,
                                    Longitude = result.Result.Results[0].Locations[0].Longitude
                                }

                            };
                            bingMap.Dispatcher.BeginInvoke(new Action(delegate()
                            {
                                Pushpin pushpin = new Pushpin()
                                {
                                    Location = branch.Location,
                                    Content = branch.Address,
                                    Tag = branch.BranchID
                                };
                                pushpin.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(pushpin_Tap);
                                bingMap.Children.Add(pushpin); 
                                mapLoading.Visibility = System.Windows.Visibility.Collapsed;
                                //setupMapViewport(branch);
                                //intBranches.Add(branch);
                            }));
                            App.Branches.Add(branch);
                            setupMapViewport(branch);
                            intBranches.Add(branch); 
                        }
                    //}
                    //catch (Exception ex)
                    //{
                    //    MessageBox.Show(ex.Message);
                    //}
                };
                Branches = new ObservableCollection<Branch>(intBranches);
            }
        }

        private void setupMapViewport(Branch branch)
        {
            if (viewportSize == null)
            {
                viewportSize = new LocationRect(branch.Location.Latitude,branch.Location.Longitude,branch.Location.Latitude,branch.Location.Longitude);
                return;
            }
            double north = viewportSize.North;
            double south = viewportSize.South;
            double east = viewportSize.East;
            double west = viewportSize.West;
            viewportSize.North = branch.Location.Latitude > north ? branch.Location.Latitude : north;
            viewportSize.South = branch.Location.Latitude < south ? branch.Location.Latitude : south;
            viewportSize.East = branch.Location.Longitude > east ? branch.Location.Longitude : east;
            viewportSize.West = branch.Location.Longitude < west ? branch.Location.Longitude : west;
            bingMap.Dispatcher.BeginInvoke(new Action(delegate()
            {
                bingMap.SetView(viewportSize);
            }));
            //TODO: Centre Map using Map's centre and our current position
        }

        private void setupCentrePushpin()
        {
            System.Device.Location.GeoCoordinate locator = new System.Device.Location.GeoCoordinate();
            locator.Latitude = coordinateWatcher.Position.Location.Latitude;
            locator.Longitude = coordinateWatcher.Position.Location.Longitude;
            bingMap.Dispatcher.BeginInvoke(new Action(delegate()
            {
                centreLocation = new Pushpin();
                centreLocation.Location = locator;
                centreLocation.Foreground = new SolidColorBrush(Colors.Black);
                centreLocation.Background = new SolidColorBrush(Colors.Green);
                bingMap.Children.Add(centreLocation);
            }));
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
            string Address2Pattern = @"(\w*, \w\w \w\d\w \d\w\d)";
            Address2Regex = new Regex(Address2Pattern, RegexOptions.IgnoreCase);
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
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                MessageBoxResult consent = MessageBox.Show("This app requires use of Location Services data. Please tap 'OK' to give your consent.", "Location Services consent", MessageBoxButton.OKCancel);

                if (consent == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    MessageBox.Show("Currently, this app requires location services to function. If you would like to enable location services, you can do so from the settings menu.", "Location Services Required.", MessageBoxButton.OK);
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                    return;
                }
            }
            else if (IsolatedStorageSettings.ApplicationSettings["LocationConsent"] as Boolean? == false)
            {
                MessageBox.Show("Currently, this app requires location services to function. If you would like to enable location services, you can do so from the settings menu.", "Location Services required", MessageBoxButton.OK);
                return;
            }
            mapLoading.Visibility = System.Windows.Visibility.Collapsed;
            base.OnNavigatedTo(e);
            coordinateWatcher.Start();
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
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

        private void ApplicationBarMenuItem_SettingsClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_RateClick(object sender, EventArgs e)
        {
            var rateTask = new MarketplaceReviewTask();
            rateTask.Show();
        }
    }
}
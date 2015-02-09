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
using Microsoft.Phone.Maps.Services;
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
using System.Threading.Tasks;
using BankLocator.Models;
using BankLocator_Common.Helpers;
using BankLocator_Common.Locators;
using Windows.Devices.Geolocation;

namespace CanadaTrustv1
{

    public partial class MapPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher coordinateWatcher;
        ReverseGeocodeQuery reverseGeocodeQuery = new ReverseGeocodeQuery();
        TDLocatorRequest locatorRequest = new TDLocatorRequest();
        string currentAddress;
        GeoCoordinate lastLocation = new GeoCoordinate();
        Regex URLStringRegex,
              mapIDNumberRegex,
              BranchNumberRegex,
              PhoneNumberRegex,
              AddressRegex,
              Address2Regex,
              HoursRegex,
              DistanceRegex;
        Pushpin centreLocation;
        LocationRect viewportSize = null;
        HtmlDocument doc;

        public MapPage()
        {
            InitializeComponent();
            StartPositionWatching();
        }

        public void StartPositionWatching()
        {
            coordinateWatcher = new GeoCoordinateWatcher();
            coordinateWatcher.MovementThreshold = 50;

            coordinateWatcher.PositionChanged += (s, e) =>
            {
                if (lastLocation != coordinateWatcher.Position.Location)
                {
                    mapLoading.Visibility = System.Windows.Visibility.Visible;
                    setLocation();
                }
            };
            coordinateWatcher.Start();
        }

        private async void setLocation()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            Geoposition geoposition = await geolocator.GetGeopositionAsync(TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1));
            if (!reverseGeocodeQuery.IsBusy)
            {
                reverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == false)
                {
                    MessageBox.Show("There was a problem connecting to the remote location service. Please check your internet connection and try again.", "Location Services problem", MessageBoxButton.OK);
                    return;
                }
                try
                {
                    reverseGeocodeQuery.QueryCompleted += geocodeService_ReverseGeocodeCompleted;
                    reverseGeocodeQuery.QueryAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void geocodeService_ReverseGeocodeCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            var query = sender as ReverseGeocodeQuery;
            try
            {
                if (e.Result.Count() > 0)
                {
                    foreach (MapLocation result in e.Result)
                    {
                        var address = result.Information.Address;
                        if (string.IsNullOrEmpty(address.HouseNumber) &&
                            string.IsNullOrEmpty(address.Street) &&
                            string.IsNullOrEmpty(address.StateCode))
                        {
                            setLocation();
                        }
                        if (result.Information.Address.Country == "Canada")
                        {
                            locatorRequest.FullAddress = String.Format("{0} {1}, {2}, {3}", address.HouseNumber,
                                address.Street, address.City, address.State);
                            Uri compiledUri = locatorRequest.compileUri();
                            viewportSize = null;
                            setupCentrePushpin(query.GeoCoordinate.Latitude,query.GeoCoordinate.Longitude);
                            callWebsite(compiledUri);
                            break;
                        }
                        MessageBox.Show("This app is not available in your location/region.\nPlease try again later.", "Outside of Canada", MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There was a problem connecting to the remote location service. Please check your internet connection and try again.", "Location Services problem", MessageBoxButton.OK);
                return;
            }
        }
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
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("The service is unavailable, please try again later");
                    });
                    return;
                }
                HtmlNode mapURL = doc.DocumentNode.SelectSingleNode("//img[@usemap='#pins']");
                string mapURLstring = mapURL.Attributes["src"].Value;

                setupRegex();

                MatchCollection matches = URLStringRegex.Matches(mapURLstring);
                Dispatcher.BeginInvoke(() =>
                {
                    createBranchesCollection(doc, matches);
                });
            }
        }
        private void callWebsite(Uri uri)
        {
            bingMap.Children.Clear();
            TDLocator tdLocator = new TDLocator(uri);
            tdLocator.BeginWebRequest(webRequestCallback);
        }

        private void addPushpinsToMap(BranchImpl branch)
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

        private async void createBranchesCollection(HtmlDocument doc, MatchCollection matches)
        {
            for (var i = 2; i <= 6; i++)
            {
                var BranchNumber = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[2]//.", doc,
                    BranchNumberRegex);
                BranchImpl branch = new BranchImpl()
                {
                    MapID =
                        Int32.Parse(MatchHelper.Match("//tr[@class='table'][" + i + "]/td[1]/strong", doc,
                            mapIDNumberRegex)),
                    BranchID = BranchNumber == "" ? 0 : Int32.Parse(BranchNumber),
                    PhoneNumber = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[5]", doc, PhoneNumberRegex),
                    Address = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[2]//strong", doc, AddressRegex),
                    AddressLine2 = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[2]//.", doc, Address2Regex),
                    Hours = MatchHelper.MatchAndReturnHtml("//tr[@class='table'][" + i + "]/td[4]//.", doc,
                    HoursRegex).Replace("<br>", "\n"),
                    Distance = MatchHelper.Match("//tr[@class='table'][" + i + "]/td[3]", doc, DistanceRegex),
                    Location = new GeoCoordinate()
                    {
                        Latitude = Double.Parse(matches[i - 2].Groups[1].Value),
                        Longitude = Double.Parse(matches[i - 2].Groups[2].Value)
                    }

                };
                bingMap.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    var pushpin = new Pushpin()
                    {
                        Location = branch.Location,
                        Content = branch.Address,
                        Tag = branch.BranchID
                    };
                    pushpin.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(pushpin_Tap);
                    bingMap.Children.Add(pushpin);
                    mapLoading.Visibility = System.Windows.Visibility.Collapsed;
                }));
                setupMapViewport(branch);
                App.Branches.Add(branch);
            }
        }

        private void setupMapViewport(BranchImpl branch)
        {
            if (viewportSize == null)
            {
                viewportSize = new LocationRect(
                    branch.Location.Latitude,
                    branch.Location.Longitude,
                    branch.Location.Latitude,
                    branch.Location.Longitude);
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

        private void setupCentrePushpin(double latitude, double longitude)
        {
            System.Device.Location.GeoCoordinate locator = new System.Device.Location.GeoCoordinate();
            locator.Latitude = latitude;
            locator.Longitude = longitude;
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
            //string URLStringPattern = @"pp=([0-9\\.]*),([0-9-\\.]*);[0-9]+;(.)";
            string URLStringPattern = @"pp=(\d{2}\.\d*),(-\d{3}\.\d*);\d*;(\d+)";
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
            int TimesRun = IsolatedStorageSettings.ApplicationSettings.Contains("TimesRun") ? (int) IsolatedStorageSettings.ApplicationSettings["TimesRun"] : 0;
            if (TimesRun == 3 || TimesRun - 3 % 5 == 0)
            {
                MessageBoxResult consent = MessageBox.Show("Please consider rating and reviewing this app. Plese tap 'OK' to be taken to the marketplace screen", "Rate and review", MessageBoxButton.OKCancel);
                if (consent == MessageBoxResult.OK)
                {
                    var rateTask = new MarketplaceReviewTask();
                    rateTask.Show();
                }
            }
            IsolatedStorageSettings.ApplicationSettings["TimesRun"] = ++TimesRun;
            mapLoading.Visibility = System.Windows.Visibility.Collapsed;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            mapLoadingTextBlock.Text = "Updating";
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

        private void ApplicationBarMenuItem_FeedbackClick(object sender, EventArgs e)
        {
            WebBrowserTask wbTask = new WebBrowserTask();
            wbTask.Uri = new Uri("http://sjetha.uservoice.com");
            wbTask.Show();
        }
    }
}
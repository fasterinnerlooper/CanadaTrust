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
using CanadaTrustv1.ViewModel;
using BankLocator_Common.Models;
using Microsoft.Phone.Shell;

namespace CanadaTrustv1
{

    public partial class MapPage : PhoneApplicationPage
    {
        Pushpin centreLocation;
        MapPageViewModel viewModel;
        public bool ShowATMs = true;

        public MapPage()
        {
            InitializeComponent();
            viewModel = new MapPageViewModel(this);
            viewModel.StartPositionWatching();
        }

        public void Show(string message = null, string title = null, MessageBoxButton? button = null)
        {
            if (button != null)
            {
                MessageBox.Show(message, title, (MessageBoxButton)button);
            }
            else
            {
                MessageBox.Show(message);
            }
        }

        public void DrawBranchesOnMap(List<BMOBranch> branches)
        {
            mapLoading.Visibility = System.Windows.Visibility.Visible;
            bingMap.Children.Clear();
            if (centreLocation != null)
            {
                bingMap.Children.Add(centreLocation);
            }
            viewModel.ViewportSize = null;
            foreach (var branch in branches)
            {
                this.addPushpinsToMap(branch);
                this.setupMapViewport(branch);
            }
            var fewerButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            var moreButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            fewerButton.IsEnabled = viewModel.CanDisplayFewer();
            moreButton.IsEnabled = viewModel.CanDisplayMore();
            mapLoading.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void addPushpinsToMap(BMOBranch branch)
        {
            Pushpin pushpin = new Pushpin()
            {
                Location = new GeoCoordinate() { Latitude = branch.Location.Latitude, Longitude = branch.Location.Longitude },
                Content = branch.Address,
                Tag = branch.Id
            };
            pushpin.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(pushpin_Tap);
            bingMap.Children.Add(pushpin);
        }

        private void setupMapViewport(BMOBranch branch)
        {
            if (viewModel.ViewportSize == null)
            {
                var viewport = new LocationRect(
                    branch.Location.Latitude,
                    branch.Location.Longitude,
                    branch.Location.Latitude,
                    branch.Location.Longitude);
                viewModel.ViewportSize = viewport;
                return;
            }
            double north = viewModel.ViewportSize.North;
            double south = viewModel.ViewportSize.South;
            double east = viewModel.ViewportSize.East;
            double west = viewModel.ViewportSize.West;
            var newviewport = new LocationRect()
            {
                North = branch.Location.Latitude > north ? branch.Location.Latitude : north,
                South = branch.Location.Latitude < south ? branch.Location.Latitude : south,
                East = branch.Location.Longitude > east ? branch.Location.Longitude : east,
                West = branch.Location.Longitude < west ? branch.Location.Longitude : west
            };
            viewModel.ViewportSize = newviewport;
            bingMap.Dispatcher.BeginInvoke(new Action(delegate()
            {
                bingMap.SetView(viewModel.ViewportSize);
            }));
            //TODO: Centre Map using Map's centre and our current position
        }

        public void setupCentrePushpin(double latitude, double longitude)
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

        private void pushpin_Tap(object sender, RoutedEventArgs e)
        {
            Pushpin pushpin = sender as Pushpin;
            var branchID = Convert.ToInt32(pushpin.Tag);
            App.Branch = viewModel.GetBranch(branchID);
            NavigationService.Navigate(new Uri("/MapDetailsPage.xaml", UriKind.Relative));
            mapLoadingTextBlock.Text = "Loading";
        }

        #region Navigation Elements
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
            int TimesRun = IsolatedStorageSettings.ApplicationSettings.Contains("TimesRun") ? (int)IsolatedStorageSettings.ApplicationSettings["TimesRun"] : 0;
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

        private void ApplicationBarIconButton_MoreBranchesClick(object sender, EventArgs e)
        {
            viewModel.BranchDisplaySize += 5;
        }

        private void ApplicationBarIconButton_FewerBranchesClick(object sender, EventArgs e)
        {
            viewModel.BranchDisplaySize -= 5;
        }

        private void ApplicationBarMenuItem_ShowOnlyClick(object sender, EventArgs e)
        {
            var menuItem = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            if (this.ShowATMs)
            {
                menuItem.Text = "Show Branches and ATMs";
                this.ShowATMs = false;
            }
            else
            {
                menuItem.Text = "Show Branches Only";
                this.ShowATMs = true;
            }
            viewModel.BranchDisplaySize = viewModel.BranchDisplaySize;
        }

        private void Recentre_Click(object sender, EventArgs e)
        {
            if (viewModel.ViewportSize != null)
            {
                bingMap.SetView(viewModel.ViewportSize);
            }
        }
    }
#endregion
}
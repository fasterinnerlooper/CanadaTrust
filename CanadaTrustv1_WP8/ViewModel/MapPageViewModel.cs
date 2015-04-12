using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BankLocator_Common.Locators;
using BankLocator_Common.Models;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Maps.Services;
using Windows.Devices.Geolocation;
using Windows.UI.Core;

namespace CanadaTrustv1.ViewModel
{
    class MapPageViewModel
    {
        private MapPage view;
        private Geolocator geoLocator;
        private ReverseGeocodeQuery reverseGeocodeQuery = new ReverseGeocodeQuery();
        private Geoposition lastLocation;
        private LocationRect viewportSize = null;
        private BMOLocator bmoLocator;
        private int branchDisplaySize = 5;

        public MapPageViewModel(MapPage view)
        {
            this.view = view;
            this.bmoLocator = new BMOLocator();
        }

        public int BranchDisplaySize
        {
            get
            {
                return this.branchDisplaySize;
            }
            set {
                this.branchDisplaySize = value;
                if (view.ShowATMs)
                {
                    view.DrawBranchesOnMap(bmoLocator.Branches.Take(this.branchDisplaySize).ToList<BMOBranch>());
                }
                else
                {
                    view.DrawBranchesOnMap(bmoLocator.Branches.Where(x=>x.IsBranch==true).Take(this.branchDisplaySize).ToList<BMOBranch>());
                }
            }
        }

        public bool CanDisplayMore() {
            return bmoLocator.Branches.Count > this.branchDisplaySize;
        }

        public bool CanDisplayFewer()
        {
            return this.branchDisplaySize > 5;
        }

        public LocationRect ViewportSize
        {
            get
            {
                return this.viewportSize;
            }
            set
            {
                this.viewportSize = value;
            }
        }

        public void StartPositionWatching()
        {
            geoLocator = new Geolocator();
            geoLocator.DesiredAccuracy = PositionAccuracy.High;
            geoLocator.MovementThreshold = 50;
            geoLocator.PositionChanged += geoLocator_PositionChanged;
        }

        private async void geoLocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Geoposition geoposition = await geoLocator.GetGeopositionAsync(TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1));
            if (lastLocation != geoposition)
            {
                this.view.Dispatcher.BeginInvoke(() => 
                {
                    view.mapLoading.Visibility = System.Windows.Visibility.Visible;
                    this.setLocation(geoposition);
                });
            }
        }
        private void setLocation(Geoposition geoposition)
        {
            if (!reverseGeocodeQuery.IsBusy)
            {
                reverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == false)
                {
                    view.Show("There was a problem connecting to the remote location service. Please check your internet connection and try again.", "Location Services problem", MessageBoxButton.OK);
                    return;
                }
                try
                {
                    reverseGeocodeQuery.QueryCompleted += geocodeService_ReverseGeocodeCompleted;
                    reverseGeocodeQuery.QueryAsync();
                }
                catch (Exception ex)
                {
                    view.Show(ex.Message);
                }
            }
        }

        public BMOBranch GetBranch(int BranchID)
        {
            return bmoLocator.Branches.First<BMOBranch>(x => x.Id == BranchID);
        }

        private async void geocodeService_ReverseGeocodeCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            var query = sender as ReverseGeocodeQuery;
            try
            {
                if (e.Result.Count() > 0)
                {
                    foreach (MapLocation result in e.Result)
                    {
                        var address = result.Information.Address;
                        if (!String.IsNullOrEmpty(result.Information.Address.Country) &&
                            result.Information.Address.Country == "Canada")
                        {
                            viewportSize = null;
                            bmoLocator.SetLocation(query.GeoCoordinate.Latitude,query.GeoCoordinate.Longitude);
                            bmoLocator.InitializeHttpContent();
                            await bmoLocator.BeginHttpClientRequest();
                            await bmoLocator.CreateBranches();
                            view.setupCentrePushpin(query.GeoCoordinate.Latitude,query.GeoCoordinate.Longitude);
                            view.DrawBranchesOnMap(bmoLocator.Branches.Take(this.branchDisplaySize).ToList<BMOBranch>());
                            break;
                        }
                        view.Show("This app is not available in your location/region.\nPlease try again later.", "Outside of Canada", MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception)
            {
                view.Show("There was a problem connecting to the remote location service. Please check your internet connection and try again.", "Location Services problem", MessageBoxButton.OK);
                return;
            }
        }
    }
}
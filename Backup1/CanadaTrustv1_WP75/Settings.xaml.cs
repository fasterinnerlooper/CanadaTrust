using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace CanadaTrustv1
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
            LocationServices.IsChecked = IsolatedStorageSettings.ApplicationSettings["LocationConsent"] as Boolean?;
        }

        private void LocationServices_Checked(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = toggleSwitch.IsChecked;
        }
    }
}
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

namespace CanadaTrustv1
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            mainBrowser.LoadCompleted +=new System.Windows.Navigation.LoadCompletedEventHandler(mainBrowser_LoadCompleted);
        }

        private void mainBrowser_LoadCompleted(object sender, EventArgs e)
        {
            browserProgressBar.IsIndeterminate = false;
        }

        private void ApplicationBarIconButton_MainClick(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Tapping 'Cancel' will take you back to your last position", "Return to login page?", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                mainBrowser.Source = new Uri("http://www.td.com/w");
            }
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_MapClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative));
        }

        private void ApplicationBarMenuItem_AboutClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }
    }
}
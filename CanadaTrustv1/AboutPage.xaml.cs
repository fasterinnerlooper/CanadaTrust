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
using Microsoft.Phone.Tasks;

namespace CanadaTrustv1
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void ApplicationBarIconButton_MapClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative));
        }

        private void ApplicationBarMenuItem_AboutClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void ApplicationBarMenuItem_Settings(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void HyperlinkButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask composerTask = new EmailComposeTask()
            {
                To = "shafiq.jetha@outlook.com",
                Subject = "Feedback on the Canda Trust Windows Phone app"
            };
            composerTask.Show();
        }

        private void ApplicationBarIconButton_RateClick(object sender, EventArgs e)
        {
            var rateTask = new MarketplaceReviewTask();
            rateTask.Show();
        }
    }
}
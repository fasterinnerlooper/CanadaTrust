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
using System.Text;
using System.IO;
using System.Diagnostics;

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

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HttpWebRequest request = HttpWebRequest.Create(new Uri("https://www.paypal.com/cgi-bin/webscr")) as HttpWebRequest;
            request.Method="POST";
            string postData = "cmd=donations&business=shafiqjetha@live.ca&lc=CA&item_name=Shafiq Jetha&currency_code=CAD&bn=PP-DonationsBF:btn_donate_SM.gif:NonHosted";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType="application/x-www-form-urlencoded";
            request.ContentLength=byteArray.Length;
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
        }

        private void GetRequestStreamCallback(IAsyncResult callbackResult)
        {
            HttpWebRequest request = callbackResult.AsyncState as HttpWebRequest;
            Stream postStream = request.EndGetRequestStream(callbackResult);
            request.Method="POST";
            string postData = "cmd=donations&business=shafiqjetha@live.ca&lc=CA&item_name=Shafiq Jetha&currency_code=CAD&bn=PP-DonationsBF:btn_donate_SM.gif:NonHosted";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType="application/x-www-form-urlencoded";
            request.ContentLength=byteArray.Length;
            postStream.Write(byteArray,0,byteArray.Length);
            postStream.Close();
            request.BeginGetResponse(new AsyncCallback(GetResponseStreamCallback), request);
        }

        private void GetResponseStreamCallback(IAsyncResult callbackResult) {
            HttpWebRequest request = callbackResult.AsyncState as HttpWebRequest;
            HttpWebResponse response = request.EndGetResponse(callbackResult) as HttpWebResponse;
            using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                string result = reader.ReadToEnd();
                Debug.WriteLine(result);
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = new Uri(result);
                webBrowserTask.Show();
            }
        }
    }
}
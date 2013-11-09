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
using CanadaTrustv1.Models;
using Microsoft.Phone.Tasks;

namespace CanadaTrustv1
{
    public partial class MapDetailsPage : PhoneApplicationPage
    {
        public Branch Branch;
        public MapDetailsPage()
        {
            InitializeComponent();
            Branch = App.Branches.First(x => x.BranchID == App.currentBranch);
            if (Branch.BranchID == 0)
            {
                BranchNumber.Text = "Green Machine ABM Only";
                HoursLabel.Visibility = System.Windows.Visibility.Collapsed;
                PhoneNumberLabel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                BranchNumber.Text = Branch.getFormattedBranchID();
                HoursLabel.Visibility = System.Windows.Visibility.Visible;
                PhoneNumberLabel.Visibility = System.Windows.Visibility.Visible;
            }
            PhoneNumber.Text = Branch.PhoneNumber;
            Address.Text = Branch.Address;
            Distance.Text = Branch.Distance;
            Hours.Text = Branch.Hours;
        }

        private void Address_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            LabeledMapLocation mapLocation = new LabeledMapLocation() {
                Label = Branch.Address + " " + Branch.AddressLine2,
                Location = Branch.Location
            };
            BingMapsDirectionsTask directionTask = new BingMapsDirectionsTask();
            directionTask.End = mapLocation;
            directionTask.Show();

        }

        private void PhoneNumber_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TextBlock number = sender as TextBlock;
            PhoneCallTask phoneTask = new PhoneCallTask();
            phoneTask.PhoneNumber = number.Text;
            phoneTask.Show();
        }
    }
}
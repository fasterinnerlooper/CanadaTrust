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
using BankLocator.Models;
using BankLocator_Common.Models;
using System.Device.Location;

namespace CanadaTrustv1
{
    public partial class MapDetailsPage : PhoneApplicationPage
    {
        public BMOBranch Branch;
        public MapDetailsPage()
        {
            InitializeComponent();
            Branch = App.Branch;
            if (Branch.IsBranch)
            {
                BranchNumber.Text = Branch.getFormattedBranchID();
            }
            else
            {
                BranchNumber.Text = "ATM Only";
            }
            PhoneNumber.Text = Branch.Phone;
            Address.Text = Branch.Address;
            Distance.Text = Branch.Distance;
            var HoursDesc = Branch.OpenHoursDescription;
            if (Branch.AbmCount > 0)
            {
                HoursDesc += "\n";
                HoursDesc += Branch.AbmHours.First().Trim();
            }
            Hours.Text = Branch.OpenHoursDescription;
        }

        private void Address_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            LabeledMapLocation mapLocation = new LabeledMapLocation() {
                Label = Branch.Address + " " + Branch.AddressLine2,
                Location = new GeoCoordinate(Branch.Location.Latitude, Branch.Location.Longitude)
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
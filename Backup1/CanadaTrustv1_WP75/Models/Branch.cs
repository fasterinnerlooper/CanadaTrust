using System;
using System.ComponentModel;
using System.Device.Location;

namespace BankLocator.Models
{
    public class Branch : INotifyPropertyChanged
    {

        public int MapID { get; set; }
        public int BranchID { get; set; }
        public GeoCoordinate Location { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string AddressLine2 { get; set; }
        public string PhoneNumber { get; set; }
        public string Hours { get; set; }
        public string Distance { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        public string getFormattedBranchID()
        {
            return BranchID.ToString().PadLeft(4, '0');
        }
    }
}

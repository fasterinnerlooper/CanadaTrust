using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;
using BankLocator.Models;

namespace CanadaTrustv1.Models
{
    public class BranchImpl : Branch
    {
        public BranchImpl() : base()
        {

        }

        public GeoCoordinate Location { get; set; }
    }
}

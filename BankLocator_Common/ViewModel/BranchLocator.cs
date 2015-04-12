using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankLocator_Common.Locators;
using BankLocator_Common.Models;

namespace BankLocator_Common.ViewModel
{
    public class BranchLocator
    {
        private BMOLocator bmoLocator;

        public BranchLocator()
        {
            this.bmoLocator = new BMOLocator();
        }

        public async Task<List<BMOBranch>> GetBranches(Dictionary<string, double> coords)
        {
            this.bmoLocator.SetLocation(coords["Latitude"], coords["Longitude"]);
            this.bmoLocator.InitializeHttpContent();
            await this.bmoLocator.BeginHttpClientRequest();
            await this.bmoLocator.CreateBranches();
            return this.bmoLocator.Branches;
        }
    }
}

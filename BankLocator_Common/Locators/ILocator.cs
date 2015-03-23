using System;
namespace BankLocator_Common.Locators
{
    interface ILocator
    {
        System.Threading.Tasks.Task BeginHttpClientRequest();
        System.Collections.Generic.List<BankLocator_Common.Models.BMOBranch> Branches { get; set; }
        System.Threading.Tasks.Task CreateBranches();
        System.Net.Http.HttpClient HttpClient { get; }
        System.Net.Http.StringContent HttpContent { get; }
        System.Net.Http.HttpResponseMessage HttpResponseMessage { get; }
        void InitializeHttpContent();
        double Latitude { get; set; }
        double Longitude { get; set; }
        void SetLocation(double latitude, double longitude);
        BankLocator_Common.Models.RootObject translateJson(string json);
    }
}

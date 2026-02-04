using Sitecore.DataExchange;
using Sitecore.DataExchange.Models;

namespace Brightcove.DataExchangeFramework.Settings
{
    public class BrightcoveEndpointSettings : IPlugin
    {
        public Endpoint BrightcoveEndpoint { get; set; }
        public Endpoint SitecoreEndpoint { get; set; }
    }
}
using Sitecore.Data.Items;
using Sitecore.DataExchange;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.DataExchangeFramework.Settings
{
    public class WebApiSettings : IPlugin
    {
        public string AccountId { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";

        public string ValidationMessage { get; set; } = "";

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(AccountId))
            {
                ValidationMessage = "No account ID is specified on the endpoint. ";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ClientId))
            {
                ValidationMessage = "No client ID is specified on the endpoint. ";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                ValidationMessage = "No client secret is specified on the endpoint. ";
                return false;
            }

            return true;
        }
    }
}

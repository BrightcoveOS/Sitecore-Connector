using Sitecore.Data.Items;
using Sitecore.DataExchange;
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

        public Item AccountItem { get; set; } = null;
    }
}

using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Converters.Endpoints;
using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.DataExchangeFramework.Converters
{
    public class WebApiEndpointConverter : BaseEndpointConverter
    {
        public const string TemplateAccount = "Account";

        public WebApiEndpointConverter(IItemModelRepository repository) : base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, Endpoint endpoint)
        {
            Guid accountItemId = this.GetGuidValue(source, TemplateAccount);
            ItemModel accountItem = ItemModelRepository.Get(accountItemId);

            WebApiSettings accountSettings = new WebApiSettings();

            if(accountItem != null)
            {
                accountSettings.AccountId = this.GetStringValue(accountItem, "AccountId") ?? "";
                accountSettings.ClientId = this.GetStringValue(accountItem, "ClientId") ?? "";
                accountSettings.ClientSecret = this.GetStringValue(accountItem, "ClientSecret") ?? "";
            }

            endpoint.AddPlugin(accountSettings);
        }
    }
}

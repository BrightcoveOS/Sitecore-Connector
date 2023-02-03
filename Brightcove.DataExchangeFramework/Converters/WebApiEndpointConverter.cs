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
            Item accountItem = Sitecore.Context.ContentDatabase.GetItem(new ID(accountItemId));

            WebApiSettings accountSettings = new WebApiSettings();

            if(accountItem != null)
            {
                accountSettings.AccountId = accountItem["AccountId"];
                accountSettings.ClientId = accountItem["ClientId"];
                accountSettings.ClientSecret = accountItem["ClientSecret"];
                accountSettings.AccountItem = accountItem;
            }

            endpoint.AddPlugin(accountSettings);
        }
    }
}

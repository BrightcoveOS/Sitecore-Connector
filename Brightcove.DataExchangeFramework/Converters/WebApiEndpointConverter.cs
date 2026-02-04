using Brightcove.DataExchangeFramework.Helpers;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Converters.Endpoints;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;

namespace Brightcove.DataExchangeFramework.Converters
{
    public class WebApiEndpointConverter : BaseEndpointConverter
    {
        public WebApiEndpointConverter(IItemModelRepository repository) 
            : base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, Endpoint endpoint)
        {
            Guid accountItemId = GetGuidValue(source, FieldName.Account);
            ItemModel accountItem = ItemModelRepository.Get(accountItemId);

            WebApiSettings accountSettings = new WebApiSettings();

            if (accountItem != null)
            {
                accountSettings.AccountId = GetStringValue(accountItem, FieldName.BrightcoveAccount.AccountId) ?? "";
                accountSettings.ClientId = GetStringValue(accountItem, FieldName.BrightcoveAccount.ClientId) ?? "";
                accountSettings.ClientSecret = GetStringValue(accountItem, FieldName.BrightcoveAccount.ClientSecret) ?? "";
            }

            endpoint.AddPlugin(accountSettings);
        }
    }
}

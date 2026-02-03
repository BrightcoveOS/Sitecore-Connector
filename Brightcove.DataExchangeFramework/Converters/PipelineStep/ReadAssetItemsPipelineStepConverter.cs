using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Providers.Sc.Converters.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;
using Brightcove.DataExchangeFramework.Helpers;

namespace Brightcove.DataExchangeFramework.Converters
{
    [SupportedIds("{51EF874F-CCA2-402D-8D5F-289E635D68E3}")]
    public class ReadAssetItemsPipelineStepConverter : ReadSitecoreItemsStepConverter
    {
        public ReadAssetItemsPipelineStepConverter(IItemModelRepository repository) :
            base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            base.AddPlugins(source, pipelineStep);

            Guid endpointId = GetGuidValue(source, FieldName.Endpoint.BrightcoveEndpoint);
            ResolveAssetItemSettings resolveAssetItemSettings = new ResolveAssetItemSettings();

            if (endpointId != null)
            {
                ItemModel endpointItem = ItemModelRepository.Get(endpointId);

                if(endpointItem != null)
                {
                    resolveAssetItemSettings.AcccountItemId = GetStringValue(endpointItem, FieldName.Account) ?? "";
                    resolveAssetItemSettings.RelativePath = GetStringValue(source, FieldName.RelativePath) ?? "";

                    Database database = Sitecore.Configuration.Factory.GetDatabase(ItemModelRepository.DatabaseName);
                    resolveAssetItemSettings.AccountItem = database.GetItem(resolveAssetItemSettings.AcccountItemId);
                    resolveAssetItemSettings.ParentItem = database.GetItem(resolveAssetItemSettings.AccountItem?.Paths?.Path + "/" + resolveAssetItemSettings.RelativePath);
                }
            }

            pipelineStep.AddPlugin(resolveAssetItemSettings);
        }
    }
}

using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;
using Brightcove.DataExchangeFramework.Helpers;

namespace Brightcove.DataExchangeFramework.Converters
{
    public class SyncPipelineStepConverter : BasePipelineStepConverter
    {
        public SyncPipelineStepConverter(IItemModelRepository repository) : 
            base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            Guid endpointId = GetGuidValue(source, FieldName.EndpointFrom);
            BrightcoveSyncSettings settings = new BrightcoveSyncSettings();

            if (endpointId != null)
            {
                ItemModel endpointItem = ItemModelRepository.Get(endpointId);

                if(endpointItem != null)
                {
                    string accountId = GetStringValue(endpointItem, FieldName.Account) ?? "";
                    settings.AccountItem = ItemModelRepository.Get(accountId);
                }
            }

            pipelineStep.AddPlugin(settings);
        }
    }
}

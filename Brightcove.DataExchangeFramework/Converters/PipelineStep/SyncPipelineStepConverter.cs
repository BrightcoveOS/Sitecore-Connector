using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Providers.Sc.Converters.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.DataExchangeFramework.Converters
{
    public class SyncPipelineStepConverter : BasePipelineStepConverter
    {
        public SyncPipelineStepConverter(IItemModelRepository repository) : base(repository)
        {

        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            Guid endpointId = this.GetGuidValue(source, "EndpointFrom");
            BrightcoveSyncSettings settings = new BrightcoveSyncSettings();

            if (endpointId != null)
            {
                ItemModel endpointItem = ItemModelRepository.Get(endpointId);

                if(endpointItem != null)
                {
                    string accountId = this.GetStringValue(endpointItem, "Account") ?? "";
                    settings.AccountItem = ItemModelRepository.Get(accountId);
                }
            }

            pipelineStep.AddPlugin(settings);
        }
    }
}

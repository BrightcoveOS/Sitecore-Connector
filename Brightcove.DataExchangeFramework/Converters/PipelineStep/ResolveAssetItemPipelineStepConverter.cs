using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Providers.Sc.Converters.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;

namespace Brightcove.DataExchangeFramework.Converters
{
    [SupportedIds("{31250AD6-4D31-485E-A42C-8D4ADE27B318}")]
    public class ResolveAssetItemPipelineStepConverter : ResolveSitecoreItemStepConverter
    {
        public ResolveAssetItemPipelineStepConverter(IItemModelRepository repository) : base(repository) { }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            base.AddPlugins(source, pipelineStep);

            Guid endpointId = this.GetGuidValue(source, "BrightcoveEndpoint");
            ResolveAssetItemSettings resolveAssetItemSettings = new ResolveAssetItemSettings();

            if (endpointId != null)
            {
                Item endpointItem = Sitecore.Context.ContentDatabase.GetItem(new ID(endpointId));

                if (endpointItem != null)
                {
                    resolveAssetItemSettings.AcccountItemId = endpointItem["Account"];
                    resolveAssetItemSettings.RelativePath = this.GetStringValue(source, "RelativePath") ?? "";
                }
            }

            pipelineStep.AddPlugin<ResolveAssetItemSettings>(resolveAssetItemSettings);
        }
    }
}
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
                ItemModel endpointItem = ItemModelRepository.Get(endpointId);

                if (endpointItem != null)
                {
                    resolveAssetItemSettings.AcccountItemId = this.GetStringValue(endpointItem, "Account") ?? "";
                    resolveAssetItemSettings.RelativePath = this.GetStringValue(source, "RelativePath") ?? "";

                    Database database = Sitecore.Configuration.Factory.GetDatabase(ItemModelRepository.DatabaseName);
                    resolveAssetItemSettings.AccountItem = database.GetItem(resolveAssetItemSettings.AcccountItemId);
                    resolveAssetItemSettings.ParentItem = database.GetItem(resolveAssetItemSettings.AccountItem?.Paths?.Path + "/" + resolveAssetItemSettings.RelativePath);
                }
            }

            //We need to store the resolve asset item plugin in the global Sitecore.DataExchangeContext so it
            //can be used in the VideoIdsPropertyValueReader
            if (Sitecore.DataExchange.Context.GetPlugin<ResolveAssetItemSettings>() == null)
            {
                Sitecore.DataExchange.Context.Plugins.Add(resolveAssetItemSettings);
            }

            pipelineStep.AddPlugin<ResolveAssetItemSettings>(resolveAssetItemSettings);
        }
    }
}
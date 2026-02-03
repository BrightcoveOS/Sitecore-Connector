using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using Brightcove.DataExchangeFramework.Helpers;

namespace Brightcove.DataExchangeFramework.Converters
{
    public class ResolveOrUpdateAssetModelPipelineStepConverter : PipelineStepWithEndpointFromConverter
    {
        public ResolveOrUpdateAssetModelPipelineStepConverter(IItemModelRepository repository) : 
            base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            base.AddPlugins(source, pipelineStep);

            var resolveAssetModelSettings = new ResolveAssetModelSettings()
            {
                AssetItemLocation = GetGuidValue(source, FieldName.TemplateAssetItemLocation),
                AssetModelLocation = GetGuidValue(source, FieldName.TemplateAssetModelLocation)
            };

            pipelineStep.AddPlugin(resolveAssetModelSettings);
        }
    }
}

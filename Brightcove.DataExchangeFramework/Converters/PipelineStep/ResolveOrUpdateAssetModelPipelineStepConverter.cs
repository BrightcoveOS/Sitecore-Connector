using Brightcove.DataExchangeFramework.Settings;
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
    public class ResolveOrUpdateAssetModelPipelineStepConverter : PipelineStepWithEndpointFromConverter
    {
        public const string TemplateAssetItemLocation = "AssetItemLocation";
        public const string TemplateAssetModelLocation = "AssetModelLocation";

        public ResolveOrUpdateAssetModelPipelineStepConverter(IItemModelRepository repository) : base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            base.AddPlugins(source, pipelineStep);

            var resolveAssetModelSettings = new ResolveAssetModelSettings()
            {
                AssetItemLocation = this.GetGuidValue(source, TemplateAssetItemLocation),
                AssetModelLocation = this.GetGuidValue(source, TemplateAssetModelLocation)
            };

            pipelineStep.AddPlugin(resolveAssetModelSettings);
        }
    }
}

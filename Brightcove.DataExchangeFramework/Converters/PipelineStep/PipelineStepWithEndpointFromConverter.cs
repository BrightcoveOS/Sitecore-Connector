using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using Brightcove.DataExchangeFramework.Helpers;

namespace Brightcove.DataExchangeFramework.Converters
{
    public class PipelineStepWithEndpointFromConverter : BasePipelineStepConverter
    {
        public PipelineStepWithEndpointFromConverter(IItemModelRepository repository): 
            base(repository) 
        { 
        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            AddEndpointSettings(source, pipelineStep);
        }

        private void AddEndpointSettings(ItemModel source, PipelineStep pipelineStep)
        {
            EndpointSettings newPlugin = new EndpointSettings();
            Endpoint model = ConvertReferenceToModel<Endpoint>(source, FieldName.EndpointFrom);
            if (model != null)
                newPlugin.EndpointFrom = model;
            pipelineStep.AddPlugin(newPlugin);
        }
    }
}

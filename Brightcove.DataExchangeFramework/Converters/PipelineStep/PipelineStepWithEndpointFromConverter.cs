using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework.Converters
{
    public class PipelineStepWithEndpointFromConverter : BasePipelineStepConverter
    {
        public const string FieldNameEndpointFrom = "EndpointFrom";

        public PipelineStepWithEndpointFromConverter(IItemModelRepository repository)
          : base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            this.AddEndpointSettings(source, pipelineStep);
        }

        private void AddEndpointSettings(ItemModel source, PipelineStep pipelineStep)
        {
            EndpointSettings newPlugin = new EndpointSettings();
            Endpoint model = this.ConvertReferenceToModel<Endpoint>(source, "EndpointFrom");
            if (model != null)
                newPlugin.EndpointFrom = model;
            pipelineStep.AddPlugin<EndpointSettings>(newPlugin);
        }
    }
}

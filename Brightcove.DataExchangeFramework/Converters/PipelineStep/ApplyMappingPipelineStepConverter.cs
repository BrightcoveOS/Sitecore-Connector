using Sitecore.DataExchange.ApplyMapping;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Repositories;

namespace Brightcove.DataExchangeFramework.Converters
{
    [SupportedIds("{AB4AF4DF-D282-4CD1-8268-FA12A9E457A3}")]
    public class ApplyMappingPipelineStepConverter : ApplyMappingStepConverter
    {
        public ApplyMappingPipelineStepConverter(IItemModelRepository repository) 
            : base(repository) 
        { 
        }
    }
}
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Providers.Sc.Converters.PipelineSteps;
using Sitecore.DataExchange.Repositories;

namespace Brightcove.DataExchangeFramework.Converters
{
    [SupportedIds("{F4AF37B2-F92D-4E49-A017-3D7489E23910}")]
    public class UpdateAssetItemPipelineStepConverter : UpdateSitecoreItemStepConverter
    {
        public UpdateAssetItemPipelineStepConverter(IItemModelRepository repository) 
            : base(repository)
        {
        }
    }
}
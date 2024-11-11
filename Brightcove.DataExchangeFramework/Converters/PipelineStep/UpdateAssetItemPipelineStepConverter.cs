using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.ApplyMapping;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Providers.Sc.Converters.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;

namespace Brightcove.DataExchangeFramework.Converters
{
    [SupportedIds("{F4AF37B2-F92D-4E49-A017-3D7489E23910}")]
    public class UpdateAssetItemPipelineStepConverter : UpdateSitecoreItemStepConverter
    {
        public UpdateAssetItemPipelineStepConverter(IItemModelRepository repository) : base(repository) { }
    }
}
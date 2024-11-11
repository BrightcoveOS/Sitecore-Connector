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
    [SupportedIds("{AB4AF4DF-D282-4CD1-8268-FA12A9E457A3}")]
    public class ApplyMappingPipelineStepConverter : ApplyMappingStepConverter
    {
        public ApplyMappingPipelineStepConverter(IItemModelRepository repository) : base(repository) { }
    }
}
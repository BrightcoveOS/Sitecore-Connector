using Sitecore.DataExchange;
using Sitecore.DataExchange.DataAccess;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;

namespace Brightcove.DataExchangeFramework.Settings
{
    public class MappingSettings : IPlugin
    {
        public IEnumerable<IMappingSet> ModelMappingSets { get; set; }

        public IEnumerable<IMappingSet> VariantMappingSets { get; set; }

        public Guid SourceObjectLocation { get; set; }

        public Guid TargetObjectLocation { get; set; }

        public bool SyncDeletionsFromBrightcove { get; set; }
    }
}
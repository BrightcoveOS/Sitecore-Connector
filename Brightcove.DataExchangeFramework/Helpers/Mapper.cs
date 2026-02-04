using Sitecore.DataExchange.DataAccess;
using System;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Helpers
{
    public static class Mapper
    {
        public static MappingContext ApplyMapping(IMappingSet mappingSet, object source, object target)
        {
            if (mappingSet == null)
            {
                throw new ArgumentNullException(nameof(mappingSet));
            }

            if (mappingSet.Mappings == null)
            {
                throw new ArgumentNullException(nameof(mappingSet.Mappings));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            MappingContext mappingContext = new MappingContext()
            {
                Source = source,
                Target = target
            };

            mappingSet.Run(mappingContext);

            //pipelineContext.GetSynchronizationSettings().IsTargetDirty = this.IsTargetDirty(mappingContext, mappingSettings, pipelineContext, logger);
            //if (!this.ShouldRunMappingsAppliedActions(mappingContext, mappingSettings, pipelineContext, logger) return;
            //this.RunMappingsAppliedActions(mappingContext, mappingSettings, pipelineContext, logger);

            return mappingContext;
        }

        public static string GetFailedMappings(MappingContext mappingContext)
        {
            return $"Failed mapping(s) {mappingContext.RunFail.Count}: '{string.Join(",", mappingContext.RunFail.Select(m => m.Identifier).ToArray())}'";
        }

        public static bool HasErrors(MappingContext mappingContext)
        {
            return mappingContext.RunFail.Any();
        }
    }
}

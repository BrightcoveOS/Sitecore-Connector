using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Linq;
using Sitecore.DataExchange.Extensions;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class GetLabelsPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            DateTime lastSyncStartTime = GetPluginOrFail<BrightcoveSyncSettings>(pipelineContext.GetCurrentPipelineBatch()).LastSyncStartTime;

            var labels = Service.GetLabels();
            LogDebug("Identified " + labels.Count() + "label model(s) that have been modified since last sync " + lastSyncStartTime);

            var dataSettings = new IterableDataSettings(labels);
            pipelineContext.AddPlugin(dataSettings);
        }
    }
}

using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class GetFoldersPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            DateTime lastSyncStartTime = GetPluginOrFail<BrightcoveSyncSettings>(pipelineContext.GetCurrentPipelineBatch()).LastSyncStartTime;

            var folders = Service.GetFolders().Where(f => f.UpdatedDate > lastSyncStartTime);
            LogInfo("Identified " + folders.Count() + " folder model(s) that have been modified since last sync " + lastSyncStartTime);

            var dataSettings = new IterableDataSettings(folders);
            pipelineContext.AddPlugin(dataSettings);
        }
    }
}

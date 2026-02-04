using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using System;

namespace Brightcove.DataExchangeFramework.Processors
{
    class ResetSyncPipelineStepProcessor : BasePipelineStepProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var settings = pipelineStep.GetPlugin<BrightcoveSyncSettings>();

                settings.AccountItem["LastSyncStartTime"] = "";
                settings.AccountItem["LastSyncFinishTime"] = "";
                settings.StartTime = DateTime.UtcNow;

                pipelineContext.GetCurrentPipelineBatch().AddPlugin(settings);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to reset the sync because an unexpected error has occured", ex);
                pipelineContext.CriticalError = true;
            }
        }
    }
}

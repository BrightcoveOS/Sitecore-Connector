using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using System;

namespace Brightcove.DataExchangeFramework.Processors
{
    class StartSyncPipelineStepProcessor : Sitecore.DataExchange.Processors.PipelineSteps.BasePipelineStepProcessor
    {
        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var settings = pipelineStep.GetPlugin<BrightcoveSyncSettings>();

                if (settings.ErrorFlag)
                {
                    logger.Error("Aborting the sync process because an error has been detected. Please resolve the error and then run the pipeline again.");
                    pipelineContext.CriticalError = true;
                    return;
                }

                DateTime.TryParse((string)settings.AccountItem?["LastSyncStartTime"] ?? "", out settings.LastSyncStartTime);
                DateTime.TryParse((string)settings.AccountItem?["LastSyncFinishTime"] ?? "", out settings.LastSyncFinishTime);
                settings.StartTime = DateTime.UtcNow;

                pipelineContext.GetCurrentPipelineBatch().AddPlugin(settings);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to start the sync because an unexpected error has occured", ex);
                pipelineContext.CriticalError = true;
            }
        }
    }
}

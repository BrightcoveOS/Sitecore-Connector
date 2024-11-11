using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.SecurityModel;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Processors
{
    class VerifySyncPipelineStepProcessor : Sitecore.DataExchange.Processors.PipelineSteps.BasePipelineStepProcessor
    {
        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var settings = pipelineStep.GetPlugin<BrightcoveSyncSettings>();

                DateTime.TryParse(((string)settings.AccountItem?["LastSyncStartTime"] ?? ""), out settings.LastSyncStartTime);
                DateTime.TryParse(((string)settings.AccountItem?["LastSyncFinishTime"] ?? ""), out settings.LastSyncFinishTime);

                if(settings.LastSyncStartTime == DateTime.MinValue || settings.LastSyncFinishTime == DateTime.MinValue)
                {
                    logger.Error($"Aborting the pipeline because the last sync start/finish time has not been recorded. Please run the pull pipeline before starting this pipeline.");
                    pipelineContext.CriticalError = true;
                    return;
                }

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

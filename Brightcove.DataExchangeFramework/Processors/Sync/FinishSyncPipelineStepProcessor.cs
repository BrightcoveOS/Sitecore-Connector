using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using System;

namespace Brightcove.DataExchangeFramework.Processors
{
    class FinishSyncPipelineStepProcessor : BasePipelineStepProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                BrightcoveSyncSettings settings = pipelineContext.GetCurrentPipelineBatch().GetPlugin<BrightcoveSyncSettings>();

                if (settings == null || settings.ErrorFlag)
                {
                    LogError($"Failed to finish the sync because an error has occured. Please correct the error(s) shown in the logs above and then run the sync again.");
                    pipelineContext.CriticalError = true;
                    return;
                }

                settings.AccountItem["LastSyncStartTime"] = settings.StartTime.ToString();
                settings.AccountItem["LastSyncFinishTime"] = DateTime.UtcNow.ToString();

                ItemModelRepository.Update(settings.AccountItem.GetItemId(), settings.AccountItem);
            }
            catch (Exception ex)
            {
                LogError($"Failed to finish the sync because an unexpected error has occured", ex);
                pipelineContext.CriticalError = true;
            }
        }
    }
}

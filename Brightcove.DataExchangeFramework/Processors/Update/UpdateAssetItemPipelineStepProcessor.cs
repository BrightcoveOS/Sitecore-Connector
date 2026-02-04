using Brightcove.DataExchangeFramework.Helpers;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Providers.Sc.Processors.PipelineSteps;
using Sitecore.Services.Core.Diagnostics;
using System;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class UpdateAssetItemPipelineStepProcessor : UpdateSitecoreItemStepProcessor
    {
        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                base.ProcessPipelineStep(pipelineStep, pipelineContext, logger);
            }
            catch(Exception ex)
            {
                logger.Error($"Failed to update the sitecore item because an unexpected error occured", ex);
                BrightcoveSyncSettingsHelper.SetErrorFlag(pipelineContext);
                pipelineContext.Finished = true;
                pipelineContext.CriticalError = false;
            }
        }
    }
}

using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.SecurityModel;
using Sitecore.Services.Core.Diagnostics;

namespace Brightcove.DataExchangeFramework.Processors
{
    class GetExperiencesPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        BrightcoveService service;

        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            base.ProcessPipelineStep(pipelineStep, pipelineContext, logger);

            service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);

            var data = service.GetExperiences().Items;
            var dataSettings = new IterableDataSettings(data);

            pipelineContext.AddPlugin(dataSettings);

            SetFolderSettings(WebApiSettings.AccountItem.Name, "Experiences");
        }
    }
}

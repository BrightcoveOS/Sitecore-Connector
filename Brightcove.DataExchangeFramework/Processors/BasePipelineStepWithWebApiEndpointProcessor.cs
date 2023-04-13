using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.SecurityModel;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class BasePipelineStepWithWebApiEndpointProcessor : BasePipelineStepProcessor
    {
        protected WebApiSettings WebApiSettings { get; set; }
        protected Endpoint EndpointFrom { get; set; }

        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            if (pipelineStep == null)
            {
                throw new ArgumentNullException(nameof(pipelineStep));
            }
            if (pipelineContext == null)
            {
                throw new ArgumentNullException(nameof(pipelineContext));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            EndpointSettings endpointSettings = pipelineStep.GetEndpointSettings();

            if (endpointSettings == null)
            {
                LogFatal("Pipeline step processing will abort because the pipeline step is missing endpoint settings.");
                return;
            }

            EndpointFrom = endpointSettings.EndpointFrom;

            if (EndpointFrom == null)
            {
                LogFatal("Pipeline step processing will abort because the pipeline step is missing an endpoint to read from.");
                return;
            }

            WebApiSettings = GetPluginOrFail<WebApiSettings>(EndpointFrom);

            if(!WebApiSettings.Validate())
            {
                LogFatal("Pipeline step processing will abort because the Brightcove web API settings are invalid: "+WebApiSettings.ValidationMessage);
                return;
            }

            try
            {
                ProcessPipelineStepInternal(pipelineStep, pipelineContext, logger);
            }
            catch (Exception ex)
            {
                LogFatal("An unexpected error occured running the internal pipeline step", ex);
                pipelineContext.CriticalError = false;
            }
        }
    }
}

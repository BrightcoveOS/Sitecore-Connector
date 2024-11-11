using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Helpers;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class BasePipelineStepProcessor : Sitecore.DataExchange.Processors.PipelineSteps.BasePipelineStepProcessor
    {
        protected IItemModelRepository itemModelRepository { get; set; }

        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
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

                itemModelRepository = Sitecore.DataExchange.Context.ItemModelRepository;

                ProcessPipelineStepInternal(pipelineStep, pipelineContext, logger);
            }
            catch(Exception ex)
            {
                LogFatal("An unexpected error occured running the internal pipeline step", ex);
                BrightcoveSyncSettingsHelper.SetErrorFlag(pipelineContext);
                pipelineContext.Finished = true;
                pipelineContext.CriticalError = false;
            }
        }

        protected virtual void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            return;
        }

        protected T GetPluginOrFail<T>(IHasPlugins source) where T: IPlugin
        {
            if(source == null)
            {
                throw new Exception($"Could not load plugin '{typeof(T)}' because the source is null. Please make sure the pipeline step has been configured properly.");
            }

            T plugin = source.GetPlugin<T>();

            if (plugin == null)
            {
                throw new Exception($"The required plugin '{typeof(T)}' is missing. Please make sure the correct converter has been specified.");
            }

            return plugin;
        }

        protected T GetPluginOrFail<T>() where T : IPlugin
        {
            return GetPluginOrFail<T>(PipelineStep);
        }

        protected void LogError(string message)
        {
            Logger.Error(GetErrorMessage(message));
        }

        protected void LogError(string message, Exception ex)
        {
            Logger.Error($"{GetErrorMessage(message)}:\n{GetExceptionMessage(ex)}");
        }

        protected void LogFatal(string message)
        {
            Logger.Fatal(GetErrorMessage(message));
        }

        protected void LogFatal(string message, Exception ex)
        {
            Logger.Fatal($"{GetErrorMessage(message)}:\n{GetExceptionMessage(ex)}");
        }

        protected void LogDebug(string message)
        {
            Logger.Debug(GetErrorMessage(message));
        }

        protected void LogWarn(string message)
        {
            Logger.Warn(GetErrorMessage(message));
        }

        protected void LogInfo(string message)
        {
            Logger.Info(GetErrorMessage(message));
        }

        private string GetErrorMessage(string message)
        {
            return $"{message} (pipeline step: {PipelineStep.Name})";
        }

        private string GetExceptionMessage(Exception ex)
        {
            return ex.ToString();
        }
    }
}

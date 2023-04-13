using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.SecurityModel;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Processors
{
    class GetPlayersPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        BrightcoveService service;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);

                var data = service.GetPlayers().Items;

                foreach (var player in data)
                {
                    player.LastSyncTime = DateTime.UtcNow;
                }

                var dataSettings = new IterableDataSettings(data);

                LogDebug("Read " + data.Count() + " player model(s) from web API");

                pipelineContext.AddPlugin(dataSettings);
            }
            catch (Exception ex)
            {
                LogError($"Failed to get the brightcove models because an unexpected error has occured", ex);
            }
        }
    }
}

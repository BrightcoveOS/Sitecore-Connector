using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.DataExchangeFramework.Processors
{
    class GetPlayListsPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        BrightcoveService service;

        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            base.ProcessPipelineStep(pipelineStep, pipelineContext, logger);
            service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);

            var data = this.GetIterableData(WebApiSettings, pipelineStep);
            var dataSettings = new IterableDataSettings(data);

            pipelineContext.AddPlugin(dataSettings);
        }

        protected virtual IEnumerable<PlayList> GetIterableData(WebApiSettings settings, PipelineStep pipelineStep)
        {
            int totalPlaylistsCount = service.PlayListsCount();
            int limit = 100;

            for(int offset = 0; offset < totalPlaylistsCount; offset += limit)
            {
                foreach(PlayList playList in service.GetPlayLists(offset, limit))
                {
                    Logger.Debug($"Found the brightcove asset {playList.Id} (pipeline step: {pipelineStep.Name})");

                    playList.LastSyncTime = DateTime.UtcNow;

                    yield return playList;
                }
            }
        }
    }
}

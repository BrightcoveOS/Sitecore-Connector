using Brightcove.Core.Models;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Processors
{
    class GetPlayListsPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        DateTime lastSyncStartTime;
        int totalCount = 0;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            lastSyncStartTime = GetPluginOrFail<BrightcoveSyncSettings>(pipelineContext.GetCurrentPipelineBatch()).LastSyncStartTime;
            totalCount = Service.PlayListsCount();

            var data = GetIterableData(WebApiSettings, pipelineStep);
            var dataSettings = new IterableDataSettings(data);

            pipelineContext.AddPlugin(dataSettings);
        }

        protected virtual IEnumerable<PlayList> GetIterableData(WebApiSettings settings, PipelineStep pipelineStep)
        {
            IEnumerable<PlayList> playLists;
            int limit = 100;

            for (int offset = 0; offset < totalCount; offset += limit)
            {
                playLists = Service.GetPlayLists(offset, limit).Where(p => p.LastModifiedDate > lastSyncStartTime);
                LogInfo("Identified " + playLists.Count() + " playlist model(s) that have been modified since last sync " + lastSyncStartTime);

                foreach (PlayList playList in playLists)
                {
                    yield return playList;
                }
            }
        }
    }
}

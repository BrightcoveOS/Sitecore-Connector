using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
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
        DateTime lastSyncStartTime;
        int totalCount = 0;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            lastSyncStartTime = GetPluginOrFail<BrightcoveSyncSettings>(pipelineContext.GetCurrentPipelineBatch()).LastSyncStartTime;
            totalCount = service.PlayListsCount();

            var data = this.GetIterableData(WebApiSettings, pipelineStep);
            var dataSettings = new IterableDataSettings(data);

            pipelineContext.AddPlugin(dataSettings);
        }

        protected virtual IEnumerable<PlayList> GetIterableData(WebApiSettings settings, PipelineStep pipelineStep)
        {
            IEnumerable<PlayList> playLists;
            int limit = 1000;

            for (int offset = 0; offset < totalCount; offset += limit)
            {
                playLists = service.GetPlayLists(offset, limit).Where(p => p.LastModifiedDate > lastSyncStartTime);
                LogInfo("Identified " + playLists.Count() + " playlist model(s) that have been modified since last sync " + lastSyncStartTime);

                foreach (PlayList playList in playLists)
                {
                    yield return playList;
                }
            }
        }
    }
}

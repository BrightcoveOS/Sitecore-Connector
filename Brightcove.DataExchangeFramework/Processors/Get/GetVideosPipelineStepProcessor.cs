using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
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
    public class GetVideosPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        int totalCount = 0;
        string query = "";

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            BrightcoveSyncSettings syncSettings = GetPluginOrFail<BrightcoveSyncSettings>(pipelineContext.GetCurrentPipelineBatch());

            if (syncSettings.LastSyncStartTime != DateTime.MinValue)
            {
                query = $"+updated_at:[{syncSettings.LastSyncStartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}Z TO *]";
            }

            totalCount = service.VideosCount(query);
            LogInfo("Identified " + totalCount + " video model(s) that have been modified since last sync "+ syncSettings.LastSyncStartTime);

            var data = GetIterableData(pipelineStep);
            var dataSettings = new IterableDataSettings(data);

            pipelineContext.AddPlugin(dataSettings);
        }

        protected virtual IEnumerable<Video> GetIterableData(PipelineStep pipelineStep)
        {
            int limit = 1000;

            for (int offset = 0; offset < totalCount; offset += limit)
            {
                foreach (Video video in service.GetVideos(offset, limit, "created_at", query))
                {
                    yield return video;
                }
            }
        }
    }
}

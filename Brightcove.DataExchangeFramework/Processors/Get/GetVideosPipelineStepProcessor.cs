using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Converters.PipelineSteps;
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
        BrightcoveService service;
        int totalCount = 0;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);

                totalCount = service.VideosCount();
                LogDebug("Read " + totalCount + " video model(s) from web API");

                var data = GetIterableData(pipelineStep);
                var dataSettings = new IterableDataSettings(data);

                pipelineContext.AddPlugin(dataSettings);
            }
            catch (Exception ex)
            {
                LogError($"Failed to get the brightcove models because an unexpected error has occured", ex);
            }
        }

        protected virtual IEnumerable<Video> GetIterableData(PipelineStep pipelineStep)
        {
            int limit = 100;

            for (int offset = 0; offset < totalCount; offset += limit)
            {
                foreach (Video video in service.GetVideos(offset, limit))
                {
                    video.LastSyncTime = DateTime.UtcNow;
                    yield return video;
                }
            }
        }
    }
}

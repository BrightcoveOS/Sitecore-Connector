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

        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            base.ProcessPipelineStep(pipelineStep, pipelineContext, logger);

            //We may need to refresh the search index before syncing
            RefreshSearchIndex(pipelineStep);

            service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);

            var data = GetIterableData(pipelineStep);
            var dataSettings = new IterableDataSettings(data);

            pipelineContext.AddPlugin(dataSettings);
        }

        protected virtual IEnumerable<Video> GetIterableData(PipelineStep pipelineStep)
        {
            int totalVideosCount = service.VideosCount();
            int limit = 100;

            for (int offset = 0; offset < totalVideosCount; offset += limit)
            {
                foreach (Video video in service.GetVideos(offset, limit))
                {
                    Logger.Debug($"Found the brightcove asset {video.Id} (pipeline step: {pipelineStep.Name})");

                    video.LastSyncTime = DateTime.UtcNow;

                    yield return video;
                }
            }
        }

        private void RefreshSearchIndex(PipelineStep pipelineStep)
        {            
            Item labelsFolder = Sitecore.Data.Database.GetDatabase("master").GetItem(WebApiSettings.AccountItem.Paths.FullPath + "/Labels");
            Item foldersFolder = Sitecore.Data.Database.GetDatabase("master").GetItem(WebApiSettings.AccountItem.Paths.FullPath + "/Folders");

            Logger.Debug($"Started refreshing the search index before syncing videos... (pipeline step: {pipelineStep.Name})");

            var jobs = Sitecore.ContentSearch.Maintenance.IndexCustodian.RefreshTree((SitecoreIndexableItem)labelsFolder);
            jobs.Concat(Sitecore.ContentSearch.Maintenance.IndexCustodian.RefreshTree((SitecoreIndexableItem)foldersFolder));

            foreach (var job in jobs)
            {
                job.Wait();
            }

            Logger.Debug($"Finished refreshing the search index (pipeline step: {pipelineStep.Name})");
        }
    }
}

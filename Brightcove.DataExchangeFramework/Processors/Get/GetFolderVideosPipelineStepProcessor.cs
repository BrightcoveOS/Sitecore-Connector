using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Brightcove.Core.Models;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework.Processors
{
    // Gets all the videos in the folder in Brightcove Video Cloud and saves it to BrightcoveFolderVideosSettings in the pipelineContext
    public class GetFolderVideosPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        Folder folderModel;
        ItemModel folderItem;
        int totalCount = 0;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                folderModel = (Folder)pipelineContext.GetObjectFromPipelineContext(ItemIDs.PipelineContextStorageLocationSource);
                folderItem = (ItemModel)pipelineContext.GetObjectFromPipelineContext(ItemIDs.PipelineContextStorageLocationTarget);
                totalCount = folderModel.VideoCount ?? 0;

                BrightcoveFolderVideosSettings folderVideosSettings = new BrightcoveFolderVideosSettings
                {
                    folderModel = folderModel,
                    folderItem = folderItem,
                    videos = GetIterableData(pipelineStep).ToList()
                };

                pipelineContext.AddPlugin(folderVideosSettings);
            }
            catch (Exception e)
            {
                logger.Debug("Error getting folder videos");
                logger.Debug(e.Message);
            }
        }

        protected virtual IEnumerable<Video> GetIterableData(PipelineStep pipelineStep)
        {
            int limit = 100;

            for (int offset = 0; offset < totalCount; offset += limit)
            {
                foreach (Video video in service.GetFolderVideos(folderModel.Id, offset, limit, "created_at"))
                {
                    yield return video;
                }
            }
        }
    }
}

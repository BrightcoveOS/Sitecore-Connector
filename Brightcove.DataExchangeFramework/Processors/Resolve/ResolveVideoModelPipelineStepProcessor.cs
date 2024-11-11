using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class ResolveVideoModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            ResolveAssetModelSettings resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
            ItemModel item = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
            Video video = new Video();
            string videoId = "";

            //If the sitecore item is a video variant item then resolve the video (parent item)
            if (item.GetTemplateId() == new Guid("{a7eaf4fd-bcf3-4511-9e8c-2ed0b165f1d6}"))
            {
                var database = Sitecore.Data.Database.GetDatabase(itemModelRepository.DatabaseName);
                Item parentItem = database?.GetItem(new ID(item.GetItemId()))?.Parent ?? null;
                videoId = parentItem["ID"];
            }
            else
            {
                videoId = (string)item["ID"];
            }

            if (service.TryGetVideo(videoId, out video))
            {
                LogDebug($"Resolved the brightcove item '{item.GetItemId()}' to the brightcove model '{video.Id}'");
                pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, video);
            }
            else
            {
                //The item was probably deleted or the ID has been modified incorrectly so we delete the item
                LogWarn($"Deleting the brightcove item '{item.GetItemId()}' because the corresponding brightcove model '{videoId}' could not be found");
                itemModelRepository.Delete(item.GetItemId());
                pipelineContext.Finished = true;
            }
        }
    }
}

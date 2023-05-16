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
        BrightcoveService service;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
                service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);
                ItemModel item = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                string videoId = (string)item["ID"];
                Video video;

                if (service.TryGetVideo(videoId, out video))
                {
                    LogDebug($"Resolved the brightcove item '{item.GetItemId()}' to the brightcove model '{video.Id}'");

                    //The brightcove API says the asset is deleted so we should probably delete the item
                    if (video.ItemState == Core.Models.ItemState.DELETED)
                    {
                        LogInfo($"Deleting the brightcove item '{item.GetItemId()}' because the brightcove cloud has marked it for deletion");
                        Sitecore.Context.ContentDatabase.GetItem(new ID(item.GetItemId())).Delete();
                    }
                    else
                    {
                        pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, video);
                    }
                }
                else
                {
                    //The item was probably deleted or the ID has been modified incorrectly so we delete the item
                    LogWarn($"Deleting the brightcove item '{item.GetItemId()}' because the corresponding brightcove model '{videoId}' could not be found");
                    Sitecore.Context.ContentDatabase.GetItem(new ID(item.GetItemId())).Delete();
                    pipelineContext.Finished = true;
                }
            }
            catch(Exception ex)
            {
                LogError($"Failed to resolve the brightcove item because an unexpected error has occured", ex);
                pipelineContext.Finished = true;
            }
        }
    }
}

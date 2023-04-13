using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
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

        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            base.ProcessPipelineStep(pipelineStep, pipelineContext, logger);

            if(pipelineContext.CriticalError)
            {
                return;
            }

            var resolveAssetModelSettings = pipelineStep.GetPlugin<ResolveAssetModelSettings>();
            if (resolveAssetModelSettings == null)
            {
                logger.Error(
                    "No resolve asset model settings are specified for the pipeline step. "+
                    "(pipeline step: {0})",
                    pipelineStep.Name);

                pipelineContext.CriticalError = true;
                return;
            }

            try
            {
                service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);
                ItemModel item = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                string videoId = (string)item["ID"];
                Video video;

                if (service.TryGetVideo(videoId, out video))
                {
                    logger.Debug($"Successfully resolved the brightcove item '{item.GetItemId()}' to the brightcove model '{video.Id}' (pipeline step: {pipelineStep.Name})");

                    //The brightcove API says the asset is deleted so we should probably delete the item
                    if (video.ItemState == Core.Models.ItemState.DELETED)
                    {
                        logger.Info($"Deleting the brightcove item '{item.GetItemId()}' because the brightcove cloud has marked it for deletion {pipelineStep.Name})");
                        Sitecore.Context.ContentDatabase.GetItem(item.GetItemId().ToString()).Delete();
                    }
                    else
                    {
                        pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, video);
                    }
                }
                else
                {
                    //The item was probably deleted or the ID has been modified incorrectly so we delete the item
                    logger.Warn($"Deleting the brightcove item '{item.GetItemId()}' because the corresponding brightcove model '{videoId}' could not be found (pipeline step: {pipelineStep.Name})");
                    Sitecore.Context.ContentDatabase.GetItem(item.GetItemId().ToString()).Delete();
                }
            }
            catch(Exception ex)
            {
                logger.Error($"Failed to resolve the brightcove item because an unexpected error has occured (pipeline step: {pipelineStep.Name}, exception: {ex.Message})");
            }
        }
    }
}

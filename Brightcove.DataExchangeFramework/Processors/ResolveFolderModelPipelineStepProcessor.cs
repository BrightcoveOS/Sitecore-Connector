using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class ResolveFolderModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        BrightcoveService service;

        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            base.ProcessPipelineStep(pipelineStep, pipelineContext, logger);

            if (pipelineContext.CriticalError)
            {
                return;
            }

            var resolveAssetModelSettings = pipelineStep.GetPlugin<ResolveAssetModelSettings>();
            if (resolveAssetModelSettings == null)
            {
                logger.Error(
                    "No resolve asset model settings are specified for the pipeline step. " +
                    "(pipeline step: {0})",
                    pipelineStep.Name);

                pipelineContext.CriticalError = true;
                return;
            }

            try
            {
                service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);
                ItemModel item = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                string id = (string)item["ID"];
                Folder folder;

                if (string.IsNullOrWhiteSpace(id))
                {
                    logger.Debug($"Creating brightcove model for the brightcove item '{item.GetItemId()}' (pipeline step: {pipelineStep.Name})");
                    folder = CreateFolder(item);
                    pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, folder);
                }
                else if (service.TryGetFolder(id, out folder))
                {
                    pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, folder);
                    logger.Debug($"Resolved the brightcove item '{item.GetItemId()}' to the brightcove model '{id}' (pipeline step: {pipelineStep.Name})");
                }
                else
                {
                    //The item was probably deleted or the ID has been modified incorrectly so we delete the item
                    logger.Warn($"Deleting the brightcove item '{item.GetItemId()}' because the corresponding brightcove model '{id}' could not be found (pipeline step: {pipelineStep.Name})");
                    Sitecore.Context.ContentDatabase.GetItem(item.GetItemId().ToString()).Delete();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to resolve the brightcove item because an unexpected error has occured (pipeline step: {pipelineStep.Name}, exception: {ex.Message})");
            }
        }

        private Folder CreateFolder(ItemModel itemModel)
        {
            Folder folder = service.CreateFolder((string)itemModel["Name"]);
            Item item = Sitecore.Context.ContentDatabase.GetItem(new ID(itemModel.GetItemId()));

            item.Editing.BeginEdit();
            item["ID"] = folder.Id;
            item["LastSyncTime"] = DateTime.UtcNow.ToString();
            item.Editing.EndEdit();

            return folder;
        }
    }
}

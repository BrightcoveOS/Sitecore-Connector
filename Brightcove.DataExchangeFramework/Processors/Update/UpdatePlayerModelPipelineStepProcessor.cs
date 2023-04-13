using Brightcove.Core.Exceptions;
using Brightcove.Core.Extensions;
using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class UpdatePlayerModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
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

                Player player = (Player)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetModelLocation);
                ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                Item item = Sitecore.Context.ContentDatabase.GetItem(itemModel.GetItemId().ToString());

                //The item has been marked for deletion in Sitecore
                if ((string)itemModel["Delete"] == "1")
                {
                    logger.Info($"Deleting the brightcove model '{player.Id}' because it has been marked for deletion in Sitecore (pipeline step: {pipelineStep.Name})");
                    service.DeletePlayer(player.Id);

                    logger.Info($"Deleting the brightcove item '{item.ID}' because it has been marked for deleteion in Sitecore '{itemModel.GetItemId()}' (pipeline step: {pipelineStep.Name})");
                    item.Delete();

                    return;
                }

                DateTime lastSyncTime = DateTime.UtcNow;
                DateField lastModifiedTime = item.Fields["__Updated"];
                bool isNewPlayer = string.IsNullOrWhiteSpace(item["LastSyncTime"]);

                if (!isNewPlayer)
                {
                    lastSyncTime = DateTime.Parse(item["LastSyncTime"]);
                }

                //If the brightcove item has been modified since the last sync (or is new) then send the updates to brightcove
                //Unless the brightcove model has already been modified since the last sync (presumably outside of Sitecore)
                if (isNewPlayer || lastModifiedTime.DateTime > lastSyncTime)
                {
                    try
                    {
                        service.UpdatePlayer(player);

                        item.Editing.BeginEdit();
                        item["LastSyncTime"] = DateTime.UtcNow.ToString();
                        item.Editing.EndEdit();
                    }
                    //This is hacky fix to silent ignore invalid custom fields
                    //This should be removed when a more permant solution is found
                    catch (HttpStatusException ex)
                    {
                        if ((int)ex.Response.StatusCode != 422)
                            throw ex;

                        string message = ex.Response.Content.ReadAsString();

                        if (!message.Contains("custom_fields"))
                            throw ex;

                        //Rerun with the invalid custom fields removed so the rest of the updates are made
                        //player.CustomFields = null;
                        this.ProcessPipelineStep(pipelineStep, pipelineContext, logger);
                        return;
                    }

                    logger.Debug($"Successfully updated the brightcove model '{player.Id}' (pipeline step: {pipelineStep.Name})");
                }
                else
                {
                    logger.Debug($"Ignored the brightcove item '{item.ID}' because it has not been updated since last sync (pipeline step: {pipelineStep.Name})");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to update the brightcove model because an unexpected error occured (pipeline step: {pipelineStep.Name}, exception: {ex.Message})");
            }
        }
    }
}

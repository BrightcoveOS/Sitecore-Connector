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
    public class UpdateLabelModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
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

                Label label = (Label)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetModelLocation);
                ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                Item item = Sitecore.Context.ContentDatabase.GetItem(itemModel.GetItemId().ToString());

                //The item has been marked for deletion in Sitecore
                if ((string)itemModel["Delete"] == "1")
                {
                    logger.Info($"Deleting the brightcove model '{label.Path}' because it has been marked for deletion in Sitecore (pipeline step: {pipelineStep.Name})");
                    service.DeleteLabel(label.Path);

                    logger.Info($"Deleting the brightcove item '{item.ID}' because it has been marked for deleteion in Sitecore '{itemModel.GetItemId()}' (pipeline step: {pipelineStep.Name})");
                    item.Delete();

                    return;
                }

                bool isNewLabel = !string.IsNullOrWhiteSpace(item["NewLabel"]);

                if (isNewLabel)
                {
                    Label updatedLabel = service.UpdateLabel(label);

                    item.Editing.BeginEdit();
                    item["Label"] = updatedLabel.Path;
                    item["NewLabel"] = "";
                    item["LastSyncTime"] = DateTime.UtcNow.ToString();
                    item.Name = updatedLabel.SitecoreName;
                    item["__Display name"] = updatedLabel.Path;
                    item.Editing.EndEdit();

                    logger.Debug($"Successfully updated the brightcove model '{label.Path}' (pipeline step: {pipelineStep.Name})");
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

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
    public class ResolveLabelModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
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
                string labelField = (string)item["Label"];
                string newPathField = (string)item["NewPath"];
                Label label;

                if (string.IsNullOrWhiteSpace(labelField))
                {
                    if (string.IsNullOrWhiteSpace(newPathField) || !Label.TryParse(newPathField, out _))
                    {
                        logger.Debug($"The new label item '{item.GetItemId()}' does not have a valid path field set. (pipeline step: {pipelineStep.Name})");
                        return;
                    }

                    logger.Debug($"Creating brightcove model for the brightcove item '{item.GetItemId()}' (pipeline step: {pipelineStep.Name})");
                    label = CreateLabel(newPathField, item);
                    pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, label);
                }
                else if (service.TryGetLabel(labelField, out label))
                {
                    pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, label);
                    logger.Debug($"Resolved the brightcove item '{item.GetItemId()}' to the brightcove model '{labelField}' (pipeline step: {pipelineStep.Name})");
                }
                else
                {
                    //The item was probably deleted or the ID has been modified incorrectly so we delete the item
                    logger.Warn($"Deleting the brightcove item '{item.GetItemId()}' because the corresponding brightcove model '{labelField}' could not be found (pipeline step: {pipelineStep.Name})");
                    Sitecore.Context.ContentDatabase.GetItem(item.GetItemId().ToString()).Delete();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to resolve the brightcove item because an unexpected error has occured (pipeline step: {pipelineStep.Name}, exception: {ex.Message})");
            }
        }

        private Label CreateLabel(string labelPath, ItemModel itemModel)
        {
            Label label = service.CreateLabel(labelPath);
            Item item = Sitecore.Context.ContentDatabase.GetItem(new ID(itemModel.GetItemId()));

            item.Editing.BeginEdit();
            item["Label"] = label.Path;
            item["NewPath"] = "";
            item["LastSyncTime"] = DateTime.UtcNow.ToString();
            item.Name = label.SitecoreName;
            item["__Display name"] = label.Path;
            item.Editing.EndEdit();

            return label;
        }
    }
}

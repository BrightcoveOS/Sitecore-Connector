using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Extensions;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Globalization;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class ResolveLabelModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        BrightcoveService service;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
                service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);
                ItemModel item = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                string labelField = (string)item["Label"];
                string newPathField = (string)item["NewPath"];
                Label label;

                if (string.IsNullOrWhiteSpace(labelField))
                {
                    if (string.IsNullOrWhiteSpace(newPathField) || !Label.TryParse(newPathField, out _))
                    {
                        LogWarn($"The new label item '{item.GetItemId()}' does not have a valid path field set so it will be ignored");
                        return;
                    }

                    LogInfo($"Creating brightcove model for the brightcove item '{item.GetItemId()}'");
                    label = CreateLabel(newPathField, item);
                    pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, label);
                }
                else if (service.TryGetLabel(labelField, out label))
                {
                    pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, label);
                    LogDebug($"Resolved the brightcove item '{item.GetItemId()}' to the brightcove model '{labelField}'");
                }
                else
                {
                    //The item was probably deleted or the ID has been modified incorrectly so we delete the item
                    LogWarn($"Deleting the brightcove item '{item.GetItemId()}' because the corresponding brightcove model '{labelField}' could not be found");
                    Sitecore.Context.ContentDatabase.GetItem(new ID(item.GetItemId())).Delete();
                    pipelineContext.Finished = true;
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to resolve the brightcove item because an unexpected error has occured", ex);
                pipelineContext.Finished = true;
            }
        }

        private Label CreateLabel(string labelPath, ItemModel itemModel)
        {
            Label label = service.CreateLabel(labelPath);
            Item item = Sitecore.Context.ContentDatabase.GetItem(new ID(itemModel.GetItemId()), Language.Parse(itemModel.GetLanguage()));

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

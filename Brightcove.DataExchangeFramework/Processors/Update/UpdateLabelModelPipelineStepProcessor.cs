using Brightcove.Core.Exceptions;
using Brightcove.Core.Extensions;
using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Extensions;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Fields;
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
    public class UpdateLabelModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        BrightcoveService service;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
                service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);

                Label label = (Label)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetModelLocation);
                ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                Item item = Sitecore.Context.ContentDatabase.GetItem(itemModel.GetItemId().ToString(), Language.Parse(itemModel.GetLanguage()));

                //The item has been marked for deletion in Sitecore
                if ((string)itemModel["Delete"] == "1")
                {
                    LogInfo($"Deleting the brightcove model '{label.Path}' because it has been marked for deletion in Sitecore");
                    service.DeleteLabel(label.Path);

                    LogInfo($"Deleting the brightcove item '{item.ID}' because it has been marked for deleteion in Sitecore '{itemModel.GetItemId()}'");
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

                    LogInfo($"Updated the brightcove label model '{label.Path}'");
                }
                else
                {
                    LogDebug($"Ignored the brightcove item '{item.ID}' because it has not been updated since last sync");
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to update the brightcove model because an unexpected error occured", ex);
            }
        }
    }
}

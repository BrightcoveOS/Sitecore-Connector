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
    public class UpdatePlayerModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        BrightcoveService service;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
                service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);

                Player player = (Player)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetModelLocation);
                ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                Item item = Sitecore.Context.ContentDatabase.GetItem(itemModel.GetItemId().ToString(), Language.Parse(itemModel.GetLanguage()));

                //The item has been marked for deletion in Sitecore
                if ((string)itemModel["Delete"] == "1")
                {
                    LogInfo($"Deleting the brightcove model '{player.Id}' because it has been marked for deletion in Sitecore");
                    service.DeletePlayer(player.Id);

                    LogInfo($"Deleting the brightcove item '{item.ID}' because it has been marked for deleteion in Sitecore '{itemModel.GetItemId()}'");
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
                    //We dont currently map anything (and probably wont anytime soon) so nothing to update
                    /*if (isNewPlayer || player.Branches.Master.UpdatedAt < lastSyncTime)
                    {
                        service.UpdatePlayer(player);

                        item.Editing.BeginEdit();
                        item["LastSyncTime"] = DateTime.UtcNow.ToString();
                        item.Editing.EndEdit();

                        LogInfo($"Updated the brightcove player model '{player.Id}'");
                    }
                    else
                    {
                        LogWarn($"Ignored changes made to brightcove item '{item.ID}' because the brightcove asset '{player.Id}' has been modified since last sync. Please run the pull pipeline to get the latest changes");
                    }*/
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

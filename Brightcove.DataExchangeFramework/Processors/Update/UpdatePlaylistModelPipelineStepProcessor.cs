using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Extensions;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Globalization;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class UpdatePlaylistModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
                BrightcoveService service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);
                PlayList playlist = (PlayList)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetModelLocation);
                ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                Item item = Sitecore.Context.ContentDatabase.GetItem(itemModel.GetItemId().ToString(), Language.Parse(itemModel.GetLanguage()));

                //The item has been marked for deletion in Sitecore
                if ((string)itemModel["Delete"] == "1")
                {
                    LogInfo($"Deleting the brightcove model '{playlist.Id}' because it has been marked for deletion in Sitecore");
                    service.DeletePlaylist(playlist.Id);

                    LogInfo($"Deleting the brightcove item '{item.ID}' because it has been marked for deletion in Sitecore");
                    item.Delete();

                    return;
                }

                DateTime lastSyncTime = DateTime.UtcNow;
                DateField lastModifiedTime = item.Fields["__Updated"];
                bool isNewPlaylist = string.IsNullOrWhiteSpace(item["LastSyncTime"]);

                if (!isNewPlaylist)
                {
                    lastSyncTime = DateTime.Parse(item["LastSyncTime"]);
                }

                //If the brightcove item has been modified since the last sync (or is new) then send the updates to brightcove
                //Unless the brightcove asset has already been modified since the last sync (presumably outside of Sitecore)
                if (isNewPlaylist || lastModifiedTime.DateTime > lastSyncTime)
                {
                    if (isNewPlaylist || playlist.LastModifiedDate < lastSyncTime)
                    {
                        service.UpdatePlaylist(playlist);
                        LogInfo($"Updated the brightcove playlist model '{playlist.Id}'");

                        if(isNewPlaylist)
                        {
                            item.Editing.BeginEdit();
                            item["LastSyncTime"] = DateTime.UtcNow.ToString();
                            item.Editing.EndEdit();
                        }
                    }
                    else
                    {
                        LogWarn($"Ignored changes made to brightcove item '{item.ID}' because the brightcove asset '{playlist.Id}' has been modified since last sync. Please run the pull pipeline to get the latest changes");
                    }
                }
                else
                {
                    LogDebug($"Ignored the brightcove item '{item.ID}' because it has not been updated since last sync");
                }
            }
            catch(Exception ex)
            {
                LogError($"Failed to update the brightcove model because an unexpected error occured", ex);
            }
        }
    }
}

using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Extensions;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Processors.PipelineSteps;
using Sitecore.Globalization;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class ResolvePlaylistModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        BrightcoveService service;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
                service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);
                ItemModel item = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                string playlistId = (string)item["ID"];
                PlayList playlist;

                if (string.IsNullOrWhiteSpace(playlistId))
                {
                    LogInfo($"Creating brightcove model for the brightcove item '{item.GetItemId()}'");
                    playlist = CreatePlaylist(item);
                    pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, playlist);
                }
                else if (service.TryGetPlaylist(playlistId, out playlist))
                {
                    pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, playlist);
                    LogDebug($"Resolved the brightcove item '{item.GetItemId()}' to the brightcove model '{playlistId}'");
                }
                else
                {
                    //The item was probably deleted or the ID has been modified incorrectly so we delete the item
                    LogWarn($"Deleting the brightcove item '{item.GetItemId()}' because the corresponding brightcove model '{playlistId}' could not be found");
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

        private PlayList CreatePlaylist(ItemModel itemModel)
        {
            PlayList playlist = service.CreatePlaylist((string)itemModel["Name"]);
            Item item = Sitecore.Context.ContentDatabase.GetItem(new ID(itemModel.GetItemId()), Language.Parse(itemModel.GetLanguage()));

            item.Editing.BeginEdit();
            item["ID"] = playlist.Id;
            item.Editing.EndEdit();

            return playlist;
        }
    }
}

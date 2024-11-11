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
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
    {
            ResolveAssetModelSettings resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
            ItemModel item = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
            string playlistId = (string)item["ID"];

            PlayList playlist = new PlayList();
            pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, playlist);

            if (!string.IsNullOrWhiteSpace(playlistId) && service.TryGetPlaylist(playlistId, out playlist))
            {
                pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, playlist);
                LogDebug($"Resolved the brightcove item '{item.GetItemId()}' to the brightcove model '{playlistId}'");
            }
            else
            {
                //The item was probably deleted or the ID has been modified incorrectly so we delete the item
                LogWarn($"Deleting the brightcove item '{item.GetItemId()}' because the corresponding brightcove model '{playlistId}' could not be found");
                itemModelRepository.Delete(item.GetItemId());
                pipelineContext.Finished = true;
            }
        }
    }
}

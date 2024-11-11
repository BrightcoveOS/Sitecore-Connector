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
            var resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
            PlayList playlist = (PlayList)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetModelLocation);
            ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);

            if (string.IsNullOrWhiteSpace(playlist.Id))
            {
                LogInfo($"Creating brightcove model for the brightcove item '{itemModel.GetItemId()}'");
                CreatePlaylist(itemModel);

                return;
            }

            //The item has been marked for deletion in Sitecore
            if ((string)itemModel["Delete"] == "1")
            {
                LogInfo($"Deleting the brightcove model '{playlist.Id}' because it has been marked for deletion in Sitecore");
                service.DeletePlaylist(playlist.Id);

                LogInfo($"Deleting the brightcove item '{itemModel.GetItemId()}' because it has been marked for deletion in Sitecore");
                itemModelRepository.Delete(itemModel.GetItemId());

                return;
            }

            service.UpdatePlaylist(playlist);
            LogInfo($"Updated the brightcove playlist model '{playlist.Id}'");
        }

        private PlayList CreatePlaylist(ItemModel itemModel)
        {
            PlayList playlist = service.CreatePlaylist((string)itemModel["Name"]);

            itemModel["ID"] = playlist.Id;

            itemModelRepository.Update(itemModel.GetItemId(), itemModel);

            return playlist;
        }
    }
}

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
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            var resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
            Label label = (Label)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetModelLocation);
            ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);

            if (string.IsNullOrWhiteSpace(label.Path))
            {
                if (string.IsNullOrWhiteSpace(label.NewLabel) || !Label.TryParse(label.NewLabel, out _))
                {
                    LogWarn($"The new label item '{itemModel.GetItemId()}' does not have a valid path field set so it will be ignored");
                    return;
                }

                LogInfo($"Creating brightcove model for the brightcove item '{itemModel.GetItemId()}'");
                CreateLabel(label.NewLabel, itemModel);

                return;
            }

            //The item has been marked for deletion in Sitecore
            if ((string)itemModel["Delete"] == "1")
            {
                LogInfo($"Deleting the brightcove model '{label.Path}' because it has been marked for deletion in Sitecore");
                service.DeleteLabel(label.Path);

                LogInfo($"Deleting the brightcove item '{itemModel.GetItemId()}' because it has been marked for deleteion in Sitecore '{itemModel.GetItemId()}'");
                itemModelRepository.Delete(itemModel.GetItemId());

                return;
            }

            if (!string.IsNullOrWhiteSpace(label.NewLabel))
            {
                Label updatedLabel = service.UpdateLabel(label);
                LogInfo($"Updated the brightcove label model '{label.Path}'");

                itemModel["Label"] = updatedLabel.Path;
                itemModel["NewLabel"] = "";
                itemModel["ItemName"] = updatedLabel.SitecoreName;
                itemModel["__Display name"] = updatedLabel.Path;

                itemModelRepository.Update(itemModel.GetItemId(), itemModel);
            }
            else
            {
                LogDebug($"Ignored the brightcove item '{itemModel.GetItemId()}' because it has not been updated since last sync");
            }
        }

        private Label CreateLabel(string labelPath, ItemModel itemModel)
        {
            Label label = service.CreateLabel(labelPath);

            itemModel["Label"] = label.Path;
            itemModel["NewPath"] = "";
            itemModel["ItemName"] = label.SitecoreName;
            itemModel["__Display name"] = label.Path;

            itemModelRepository.Update(itemModel.GetItemId(), itemModel);

            return label;
        }
    }
}

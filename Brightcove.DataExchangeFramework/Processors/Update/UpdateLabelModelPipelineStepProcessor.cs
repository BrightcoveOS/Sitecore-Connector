using Brightcove.Core.Models;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;

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
                Service.DeleteLabel(label.Path);

                LogInfo($"Deleting the brightcove item '{itemModel.GetItemId()}' because it has been marked for deleteion in Sitecore '{itemModel.GetItemId()}'");
                ItemModelRepository.Delete(itemModel.GetItemId());

                return;
            }

            if (!string.IsNullOrWhiteSpace(label.NewLabel))
            {
                Label updatedLabel = Service.UpdateLabel(label);
                LogInfo($"Updated the brightcove label model '{label.Path}'");

                itemModel["Label"] = updatedLabel.Path;
                itemModel["NewLabel"] = "";
                itemModel["ItemName"] = updatedLabel.SitecoreName;
                itemModel["__Display name"] = updatedLabel.Path;

                ItemModelRepository.Update(itemModel.GetItemId(), itemModel);
            }
            else
            {
                LogDebug($"Ignored the brightcove item '{itemModel.GetItemId()}' because it has not been updated since last sync");
            }
        }

        private Label CreateLabel(string labelPath, ItemModel itemModel)
        {
            Label label = Service.CreateLabel(labelPath);

            itemModel["Label"] = label.Path;
            itemModel["NewPath"] = "";
            itemModel["ItemName"] = label.SitecoreName;
            itemModel["__Display name"] = label.Path;

            ItemModelRepository.Update(itemModel.GetItemId(), itemModel);

            return label;
        }
    }
}

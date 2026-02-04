using Brightcove.Core.Models;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class UpdateFolderModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            var resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
            Folder folder = (Folder)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetModelLocation);
            ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);

            if (string.IsNullOrWhiteSpace(folder.Id))
            {
                LogInfo($"Creating brightcove model for the new brightcove item '{itemModel.GetItemId()}'");
                CreateFolder(itemModel);

                return;
            }

            // The item has been marked for deletion in Sitecore
            if ((string)itemModel["Delete"] == "1")
            {
                LogInfo($"Deleting the brightcove model '{folder.Id}' because it has been marked for deletion in Sitecore");
                Service.DeleteFolder(folder.Id);

                LogInfo($"Deleting the brightcove item '{itemModel.GetItemId()}' because it has been marked for deletion in Sitecore");
                ItemModelRepository.Delete(itemModel.GetItemId());

                return;
            }

            string itemName = (string)itemModel["Name"];

            if (folder.Name != itemName)
            {
                // We can only update one field for folders (the name) so it is easier to manually map it
                folder.Name = itemName;
                Service.UpdateFolder(folder);
                LogInfo($"Updated the brightcove asset '{folder.Id}'");
            }
            else
            {
                LogDebug($"Ignored the brightcove item '{itemModel.GetItemId()}' because it has not been updated since last sync");
            }
        }


        private Folder CreateFolder(ItemModel itemModel)
        {
            Folder folder = Service.CreateFolder((string)itemModel["Name"]);

            itemModel["ID"] = folder.Id;

            ItemModelRepository.Update(itemModel.GetItemId(), itemModel);

            return folder;
        }
    }
}

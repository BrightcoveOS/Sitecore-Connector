using Brightcove.Core.Models;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class ResolveFolderModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            ResolveAssetModelSettings resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
            ItemModel item = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
            string id = (string)item["ID"];

            Folder folder = new Folder();
            pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, folder);

            if (!string.IsNullOrWhiteSpace(id) && Service.TryGetFolder(id, out folder))
            {
                pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, folder);
                LogDebug($"Resolved the brightcove item '{item.GetItemId()}' to the brightcove model '{id}'");
            }
            else
            {
                //The item was probably deleted or the ID has been modified incorrectly so we delete the item
                LogWarn($"Deleting the brightcove item '{item.GetItemId()}' because the corresponding brightcove model '{id}' could not be found");
                ItemModelRepository.Delete(item.GetItemId());
                pipelineContext.Finished = true;
            }
        }
    }
}

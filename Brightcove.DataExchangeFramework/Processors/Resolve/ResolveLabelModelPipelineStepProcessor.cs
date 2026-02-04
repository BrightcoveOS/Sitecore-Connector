using Brightcove.Core.Models;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class ResolveLabelModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            ResolveAssetModelSettings resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
            ItemModel item = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
            string labelField = (string)item["Label"];

            Label label = new Label();
            pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, label);

            if (!string.IsNullOrWhiteSpace(labelField) && Service.TryGetLabel(labelField, out label))
            {
                pipelineContext.SetObjectOnPipelineContext(resolveAssetModelSettings.AssetModelLocation, label);
                LogDebug($"Resolved the brightcove item '{item.GetItemId()}' to the brightcove model '{labelField}'");
            }
            else
            {
                //The item was probably deleted or the ID has been modified incorrectly so we delete the item
                LogWarn($"Deleting the brightcove item '{item.GetItemId()}' because the corresponding brightcove model '{labelField}' could not be found");
                ItemModelRepository.Delete(item.GetItemId());
                pipelineContext.Finished = true;
            }
        }
    }
}

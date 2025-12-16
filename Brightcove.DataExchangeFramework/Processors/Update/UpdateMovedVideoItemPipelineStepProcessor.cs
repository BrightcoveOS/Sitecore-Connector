using Brightcove.Core.Exceptions;
using Brightcove.Core.Models;
using Brightcove.Core.Services;
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
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Brightcove.Core.Extensions;
using Brightcove.DataExchangeFramework.Helpers;
using Sitecore.DataExchange.Providers.Sc.Plugins;
using Sitecore.Data;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class UpdateMovedVideoItemPipelineStepProcessor : UpdateVideoItemPipelineStepProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            var mappingSettings = GetPluginOrFail<MappingSettings>();
            var endpointSettings = GetPluginOrFail<BrightcoveEndpointSettings>();
            var webApiSettings = GetPluginOrFail<WebApiSettings>(endpointSettings.BrightcoveEndpoint);
            BrightcoveService service = new BrightcoveService(webApiSettings.AccountId, webApiSettings.ClientId, webApiSettings.ClientSecret);

            ItemModel item = (ItemModel)GetObjectFromPipelineContext(mappingSettings.TargetObjectLocation, pipelineContext, logger);
            string videoId = item.GetFieldValueAsString("ID");

            if (!service.TryGetVideo(videoId, out Video model))
            {
                LogDebug($"Brightcove model '{videoId}' could not be found for Sitecore item '{item.GetItemId()}'");
                return;
            }

            string itemLanguage = pipelineContext.GetCurrentPipelineBatch().GetPlugin<SelectedLanguagesSettings>()?.Languages?.FirstOrDefault() ?? "en";

            ApplyMappings(mappingSettings.ModelMappingSets, model, item);

            if (!ItemUpdater.Update(itemModelRepository, item))
            {
                throw new Exception($"Failed to update the item '{item.GetItemId()}'");
            }
            else
            {
                LogDebug($"Updated the video item '{item.GetItemId()}'");
            }

            var videoVariants = model.Variants ?? new List<VideoVariant>();
            var resolvedVariantItems = ResolveVideoVariants(videoVariants, item, itemLanguage);

            ApplyVariantMappings(mappingSettings.VariantMappingSets, resolvedVariantItems, itemLanguage);

            foreach (ItemModel variantItem in resolvedVariantItems.Values)
            {
                if (!ItemUpdater.Update(itemModelRepository, variantItem))
                {
                    throw new Exception($"Failed to update the item '{variantItem.GetItemId()}'");
                }
                else
                {
                    LogDebug($"Updated the video variant item '{variantItem.GetItemId()}'");
                }
            }
        }
    }
}

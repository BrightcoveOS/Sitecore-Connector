using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Brightcove.DataExchangeFramework.Helpers;
using Sitecore.DataExchange.Providers.Sc.Plugins;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class UpdateVideoItemPipelineStepProcessor : BasePipelineStepProcessor
    {
        BrightcoveService service;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            var mappingSettings = GetPluginOrFail<MappingSettings>();
            var endpointSettings = GetPluginOrFail<BrightcoveEndpointSettings>();
            var webApiSettings = GetPluginOrFail<WebApiSettings>(endpointSettings.BrightcoveEndpoint);
            service = new BrightcoveService(webApiSettings.AccountId, webApiSettings.ClientId, webApiSettings.ClientSecret);

            Video model = (Video)GetObjectFromPipelineContext(mappingSettings.SourceObjectLocation, pipelineContext, logger);
            ItemModel item = (ItemModel)GetObjectFromPipelineContext(mappingSettings.TargetObjectLocation, pipelineContext, logger);
            string itemLanguage = pipelineContext.GetPlugin<SelectedLanguagesSettings>()?.Languages?.FirstOrDefault() ?? "en";

            ApplyMappings(mappingSettings.ModelMappingSets, model, item);

            if (!ItemUpdater.Update(ItemModelRepository, item))
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
                if (!ItemUpdater.Update(ItemModelRepository, variantItem))
                {
                    throw new Exception($"Failed to update the item '{variantItem.GetItemId()}'");
                }
                else
                {
                    LogDebug($"Updated the video variant item '{variantItem.GetItemId()}'");
                }
            }
        }

        protected IDictionary<VideoVariant, ItemModel> ResolveVideoVariants(IEnumerable<VideoVariant> videoVariants, ItemModel videoItem, string itemLanguage)
        {
            var resolvedVariantItems = new Dictionary<VideoVariant, ItemModel>();
            var videoChildren = ItemModelRepository.GetChildren(videoItem.GetItemId(), itemLanguage);
            ItemModel variantItem;

            foreach (var videoVariant in videoVariants)
            {
                variantItem = videoChildren.Where(c => c["Language"].ToString() == videoVariant.Language).FirstOrDefault();

                if (variantItem == null)
                {
                    variantItem = new ItemModel
                    {
                        { "ItemName", ItemUtil.ProposeValidItemName(videoVariant.Name) },
                        { "TemplateID", new Guid("{A7EAF4FD-BCF3-4511-9E8C-2ED0B165F1D6}") },
                        { "ParentID", videoItem.GetItemId() },
                        { "ItemLanguage", itemLanguage }
                    };
                }

                resolvedVariantItems.Add(videoVariant, variantItem);
            }

            return resolvedVariantItems;
        }

        protected void ApplyVariantMappings(IEnumerable<IMappingSet> mappingSets, IDictionary<VideoVariant, ItemModel> videoVariants, string itemLanguage)
        {
            foreach (VideoVariant videoVariant in videoVariants.Keys)
            {
                ApplyMappings(mappingSets, videoVariant, videoVariants[videoVariant]);
            }
        }

        protected void ApplyMappings(IEnumerable<IMappingSet> mappingSets, object model, ItemModel item)
        {
            foreach (IMappingSet mappingSet in mappingSets)
            {
                var mappingContext = Mapper.ApplyMapping(mappingSet, model, item);

                if (Mapper.HasErrors(mappingContext))
                {
                    throw new Exception($"Failed to apply mapping set(s): {Mapper.GetFailedMappings(mappingContext)}");
                }
            }
        }
    }
}

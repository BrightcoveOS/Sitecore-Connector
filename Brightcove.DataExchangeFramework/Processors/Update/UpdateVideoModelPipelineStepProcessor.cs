using Brightcove.Core.Exceptions;
using Brightcove.Core.Extensions;
using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Helpers;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class UpdateVideoModelPipelineStepProcessor : BasePipelineStepProcessor
    {
        BrightcoveService service;

        BrightcoveSyncSettings brightcoveSyncSettings;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            var mappingSettings = GetPluginOrFail<MappingSettings>();
            var endpointSettings = GetPluginOrFail<BrightcoveEndpointSettings>();
            var webApiSettings = GetPluginOrFail<WebApiSettings>(endpointSettings.BrightcoveEndpoint);
            brightcoveSyncSettings = GetPluginOrFail<BrightcoveSyncSettings>(pipelineContext.GetCurrentPipelineBatch());

            service = new BrightcoveService(webApiSettings.AccountId, webApiSettings.ClientId, webApiSettings.ClientSecret);
            Video video = (Video)pipelineContext.GetObjectFromPipelineContext(mappingSettings.TargetObjectLocation);
            ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(mappingSettings.SourceObjectLocation);

            if (itemModel.GetTemplateId() == new Guid("{a7eaf4fd-bcf3-4511-9e8c-2ed0b165f1d6}"))
            {
                HandleVariant(itemModelRepository, mappingSettings.VariantMappingSets, itemModel, video);
                return;
            }    

            if (DeleteVideo(video, itemModel))
            {
                return;
            }

            ApplyMappings(mappingSettings.ModelMappingSets, itemModel, video);

            LogInfo($"Updating the brightcove video model '{video.Id}'");
            UpdateVideo(video);
            UpdateFolder(video, itemModel);
        }

        private void ApplyMappings(IEnumerable<IMappingSet> mappingSets, ItemModel item, object model)
        {
            foreach (IMappingSet mappingSet in mappingSets)
            {
                var mappingContext = Mapper.ApplyMapping(mappingSet, item, model);

                if (Mapper.HasErrors(mappingContext))
                {
                    throw new Exception($"Failed to apply mapping to the model '{item.GetItemId()}': {Mapper.GetFailedMappings(mappingContext)}");
                }
                else
                {
                    LogDebug($"Applied mapping to the model '{item.GetItemId()}'");
                }
            }
        }

        public bool DeleteVideo(Video video, ItemModel item)
        {
            //The item has been marked for deletion in Sitecore
            if ((string)item["Delete"] == "1")
            {
                LogInfo($"Deleting the brightcove model '{video.Id}' because it has been marked for deletion in Sitecore");
                service.DeleteVideo(video.Id);

                LogInfo($"Deleting the brightcove item '{item.GetItemId()}' because it has been marked for deleteion in Sitecore");
                itemModelRepository.Delete(item.GetItemId());

                return true;
            }

            return false;
        }

        public void UpdateVideo(Video video)
        {
            try
            {
                service.UpdateVideo(video);
            }
            //This is hacky fix to ignore invalid custom fields
            //This should be removed when a more permant solution is found
            catch (HttpStatusException ex)
            {
                if ((int)ex.Response.StatusCode != 422)
                    throw ex;

                string message = ex.Response.Content.ReadAsString();

                if (!message.Contains("custom_fields"))
                    throw ex;

                LogWarn($"The video model {video.Id} contains invalid custom fields so the custom fields will not be updated. Please verify all of the custom fields have been defined properly.");

                //Rerun with the invalid custom fields removed so the rest of the updates are made
                video.CustomFields = null;
                UpdateVideo(video);
                return;
            }
        }

        public void UpdateFolder(Video video, ItemModel item)
        {
            string folderField = (string)item["BrightcoveFolder"];

            if(string.IsNullOrWhiteSpace(folderField))
            {
                if(!string.IsNullOrWhiteSpace(video.Folder))
                {
                    LogInfo($"Removing the video '{video.Id}' from the folder '{video.Folder}'");
                    service.RemoveFromFolder(video, video.Folder);
                }
            }
            else
            {
                string folderId = (string)(itemModelRepository.Get(new Guid(folderField))["ID"]);

                if(video.Folder != folderId)
                {
                    LogInfo($"Moving the video '{video.Id}' into the folder '{folderId}'");
                    service.MoveToFolder(video, folderId);
                }
            }
        }

        /* Video Variant Handling */

        public void HandleVariant(IItemModelRepository itemModelRepository, IEnumerable<IMappingSet> mappingSets, ItemModel variantItemModel, Video model)
        {
            var database = Sitecore.Data.Database.GetDatabase(itemModelRepository.DatabaseName);
            Item variantItem = database?.GetItem(new ID(variantItemModel.GetItemId()));

            VideoVariant variantModel = new VideoVariant()
            {
                Id = model.Id
            };

            ApplyMappings(mappingSets, variantItemModel, variantModel);

            if (CreateVideoVariant(variantModel, variantItem))
            {
                return;
            }

            if (!ResolveVideoVariant(variantModel, variantItemModel))
            {
                return;
            }

            if (DeleteVideoVariant(variantModel, variantItemModel))
            {
                return;
            }

            if (variantItem.Statistics.Updated > brightcoveSyncSettings.LastSyncStartTime)
            {
                UpdateVideoVariant(variantModel);
            }
        }

        public bool ResolveVideoVariant(VideoVariant videoVariant, ItemModel item)
        {
            if(!service.TryGetVideoVariant(videoVariant.Id, videoVariant.Language, out _))
            {
                LogWarn($"Deleting the item '{item.GetItemId()}' because it could not be resolved to the model '{videoVariant.Id}:{videoVariant.Language}'");
                itemModelRepository.Delete(item.GetItemId());
                return false;
            }

            return true;
        }

        public bool CreateVideoVariant(VideoVariant videoVariant, Item item)
        {
            if(item.Statistics.Created > brightcoveSyncSettings.LastSyncStartTime)
            {
                LogInfo($"Creating the video variant model '{videoVariant.Id}:{videoVariant.Language}'");
                service.CreateVideoVariant(videoVariant.Id, videoVariant.Name, videoVariant.Language);
                return true;
            }

            return false;
        }

        public void UpdateVideoVariant(VideoVariant videoVariant)
        {
            try
            {
                service.UpdateVideoVariant(videoVariant);
            }
            //This is hacky fix to ignore invalid custom fields
            //This should be removed when a more permant solution is found
            catch (HttpStatusException ex)
            {
                if ((int)ex.Response.StatusCode != 422)
                    throw ex;

                string message = ex.Response.Content.ReadAsString();

                if (!message.Contains("custom_fields"))
                    throw ex;

                LogWarn($"The video variant model {videoVariant.Id} contains invalid custom fields so the custom fields will not be updated. Please verify all of the custom fields have been defined properly.");

                //Rerun with the invalid custom fields removed so the rest of the updates are made
                videoVariant.CustomFields = null;
                UpdateVideoVariant(videoVariant);
                return;
            }
        }

        public bool DeleteVideoVariant(VideoVariant videoVariant, ItemModel itemModel)
        {
            //The item has been marked for deletion in Sitecore
            if ((string)itemModel["Delete"] == "1")
            {
                LogInfo($"Deleting the variant '{videoVariant.Id}:{videoVariant.Language}' because it has been marked for deletion in Sitecore");
                service.DeleteVideoVariant(videoVariant.Id, videoVariant.Language);

                LogInfo($"Deleting the item '{itemModel.GetItemId()}' because it has been marked for deletion in Sitecore");
                itemModelRepository.Delete(itemModel.GetItemId());

                return true;
            }

            return false;
        }
    }
}

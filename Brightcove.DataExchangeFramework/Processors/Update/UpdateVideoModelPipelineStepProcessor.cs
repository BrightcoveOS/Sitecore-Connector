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
using Sitecore.Globalization;
using Brightcove.DataExchangeFramework.Extensions;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class UpdateVideoModelPipelineStepProcessor : BasePipelineStepProcessor
    {
        BrightcoveService service;
        IItemModelRepository itemModelRepository;

        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var mappingSettings = GetPluginOrFail<MappingSettings>();
                var endpointSettings = GetPluginOrFail<BrightcoveEndpointSettings>();
                var webApiSettings = GetPluginOrFail<WebApiSettings>(endpointSettings.BrightcoveEndpoint);
                itemModelRepository = GetPluginOrFail<ItemModelRepositorySettings>(endpointSettings.SitecoreEndpoint).ItemModelRepository;

                service = new BrightcoveService(webApiSettings.AccountId, webApiSettings.ClientId, webApiSettings.ClientSecret);
                Video video = (Video)pipelineContext.GetObjectFromPipelineContext(mappingSettings.TargetObjectLocation);
                ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(mappingSettings.SourceObjectLocation);
                Item item = Sitecore.Context.ContentDatabase.GetItem(itemModel.GetItemId().ToString(), Language.Parse(itemModel.GetLanguage()));

                DateTime lastSyncTime = DateTime.UtcNow;
                DateField lastModifiedTime = item.Fields["__Updated"];
                bool isNewVideo = string.IsNullOrWhiteSpace(item["LastSyncTime"]);

                if (!isNewVideo)
                {
                    lastSyncTime = DateTime.Parse(item["LastSyncTime"]);
                }

                if (DeleteVideo(video, itemModel))
                {
                    return;
                }

                ApplyMappings(mappingSettings.ModelMappingSets, itemModel, video);

                //If the brightcove item has been modified since the last sync (or is new) then send the updates to brightcove
                //Unless the brightcove model has already been modified since the last sync (presumably outside of Sitecore)
                if (isNewVideo || lastModifiedTime.DateTime > lastSyncTime)
                {
                    if (isNewVideo || video.LastModifiedDate < lastSyncTime)
                    {
                        UpdateVideo(video);
                        UpdateFolder(video, itemModel);

                        if (isNewVideo)
                        {
                            item.Editing.BeginEdit();
                            item["LastSyncTime"] = DateTime.UtcNow.ToString();
                            item.Editing.EndEdit();
                        }

                        LogDebug($"Updated the brightcove video model '{video.Id}'");
                    }
                    else
                    {
                        LogWarn($"Ignored changes made to brightcove item '{item.ID}' because the brightcove model '{video.Id}' has been modified since last sync. Please run the pull pipeline to get the latest changes");
                    }
                }
                else
                {
                    LogDebug($"Ignored the brightcove item '{item.ID}' because it has not been updated since last sync");
                }

                UpdateVariants(itemModelRepository, mappingSettings.VariantMappingSets, itemModel, video);
            }
            catch(Exception ex)
            {
                LogError($"An unexpected error occured updating the model", ex);
            }
        }

        private void ApplyMappings(IEnumerable<IMappingSet> mappingSets, ItemModel item, object model)
        {
            foreach (IMappingSet mappingSet in mappingSets)
            {
                var mappingContext = Mapper.ApplyMapping(mappingSet, item, model);

                if (Mapper.HasErrors(mappingContext))
                {
                    LogError($"Failed to apply mapping to the model '{item.GetItemId()}': {Mapper.GetFailedMappings(mappingContext)}");
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
                    service.RemoveFromFolder(video, video.Folder);
                    LogInfo($"Removed the video '{video.Id}' from the folder '{video.Folder}'");
                }
            }
            else
            {
                string folderId = (string)(itemModelRepository.Get(new Guid(folderField))["ID"]);

                if(video.Folder != folderId)
                {
                    service.MoveToFolder(video, folderId);
                    LogInfo($"Moved the video '{video.Id}' into the folder '{folderId}'");
                }
            }
        }

        public void UpdateVariants(IItemModelRepository itemModelRepository, IEnumerable<IMappingSet> mappingSets, ItemModel item, Video model)
        {
            var variantItems = itemModelRepository.GetChildren(item.GetItemId(), item.GetLanguage());

            foreach(ItemModel variantItem in variantItems)
            {
                try
                {
                    VideoVariant variantModel = new VideoVariant()
                    {
                        Id = model.Id
                    };

                    ApplyMappings(mappingSets, variantItem, variantModel);

                    if(!ResolveVideoVariant(variantModel, variantItem))
                    {
                        return;
                    }    

                    if(DeleteVideoVariant(variantModel, variantItem))
                    {
                        return;
                    }

                    SyncStatus status = ItemUpdater.GetSyncStatus(variantItem);

                    if (status == SyncStatus.NewItem)
                    {
                        CreateVideoVariant(variantModel, variantItem);
                        LogInfo($"Created the variant model '{model.Id}:{variantModel.Language}'");

                        UpdateVideoVariant(variantModel);
                        LogInfo($"Updated the variant model '{model.Id}:{variantModel.Language}'");
                    }

                    if (status == SyncStatus.ItemNewer)
                    {
                        UpdateVideoVariant(variantModel);
                        LogInfo($"Updated the variant model '{model.Id}:{variantModel.Language}'");
                    }

                    if (status == SyncStatus.ModelNewer)
                    {
                        LogWarn($"Ignored the item '{variantItem.GetItemId()}' because it has been modified outside of Sitecore. Please run a sync to get the latest changes.");
                    }

                    if (status == SyncStatus.Unmodified)
                    {
                        LogDebug($"Ignored the item '{variantItem.GetItemId()}' because it has not been modified since last sync.");
                    }
                }
                catch(Exception ex)
                {
                    LogError($"An unexpected error occured updating the variant '{variantItem.GetItemId()}'", ex);
                }
            }
        }

        public bool ResolveVideoVariant(VideoVariant videoVariant, ItemModel item)
        {
            //If variant is new then continue
            if(string.IsNullOrWhiteSpace((string)item["LastSyncTime"]))
            {
                return true;
            }

            if(!service.TryGetVideoVariant(videoVariant.Id, videoVariant.Language, out _))
            {
                itemModelRepository.Delete(item.GetItemId());
                LogWarn($"Deleting the item '{item.GetItemId()}' because it could not be resolved to the model '{videoVariant.Id}:{videoVariant.Language}'");
                return false;
            }

            return true;
        }

        public void CreateVideoVariant(VideoVariant videoVariant, ItemModel item)
        {
            service.CreateVideoVariant(videoVariant.Id, videoVariant.Name, videoVariant.Language);

            item["LastSyncTime"] = DateTime.UtcNow.ToString();
            itemModelRepository.Update(item.GetItemId(), item);
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

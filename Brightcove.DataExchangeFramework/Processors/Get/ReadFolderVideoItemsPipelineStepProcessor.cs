using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Brightcove.Core.Models;
using Brightcove.DataExchangeFramework.Helpers;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Local.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Providers.Sc.Extensions;
using Sitecore.DataExchange.Providers.Sc.Plugins;
using Sitecore.DataExchange.Providers.Sc.Processors.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using Sitecore.Services.Infrastructure.Sitecore.Data;

namespace Brightcove.DataExchangeFramework.Processors
{
    [RequiredEndpointPlugins(new Type[] { typeof(ItemModelRepositorySettings) })]
    public class ReadFolderVideoItemsPipelineStepProcessor : ReadAssetItemsPipelineStepProcessor
    {
        Folder folderModel;
        ItemModel folderItem;
        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                folderModel = (Folder)pipelineContext.GetObjectFromPipelineContext(ItemIDs.PipelineContextStorageLocationSource);
                folderItem = (ItemModel)pipelineContext.GetObjectFromPipelineContext(ItemIDs.PipelineContextStorageLocationTarget);

                base.ProcessPipelineStep(pipelineStep, pipelineContext, logger);
            }
            catch(Exception ex)
            {
                logger.Error("Failed to read sitecore items because an unexpected error occured", ex);
                BrightcoveSyncSettingsHelper.SetErrorFlag(pipelineContext);
                pipelineContext.Finished = true;
                pipelineContext.CriticalError = false;
            }
        }

        public override IEnumerable<ItemModel> Search(string bucketPath, string indexName, List<Guid> templateGuids, string language, IItemModelRepository modelRepository)
        {
            var index = ContentSearchManager.GetIndex(indexName);

            using (var context = index.CreateSearchContext())
            {
                List<ItemModel> itemModels = new List<ItemModel>();

                try
                {
                    ID templateId = new ID(templateGuids[0]);

                    var query = context.GetQueryable<SearchResultItem>()
                        .Where(x => x.Path.Contains(bucketPath) && x.Path != bucketPath && x.Language == language && x.TemplateId == templateId);
                    
                    var searchResults = query.ToList();
                    
                    itemModels = searchResults.Select(r => modelRepository.Get(r.ItemId.ToGuid(), language))
                        .Where(model => model != null && model.GetFieldValueAsGuid("BrightcoveFolder") == folderItem.GetItemId())
                        .ToList();

                    List<String> itemModelsWithFolders = searchResults.Select(r => modelRepository.Get(r.ItemId.ToGuid(), language))
                        .Where(model => model != null && model.GetFieldValueAsString("BrightcoveFolder")?.Length > 0)
                        .Select(m => $"ID: {m.GetFieldValueAsString("ID")} FolderID: {m.GetFieldValueAsString("BrightcoveFolder")}")
                        .ToList();

                    Logger.Info(string.Join("\n", itemModelsWithFolders));

                    //List<String> itemsModelsInAFolder = itemModels
                    //    .Where(model => model.GetFieldValueAsString("BrightcoveFolder")?.Length > 0)
                    //    .Select(m => $"ID: {m.GetFieldValueAsString("ID")} Name: \"{m.GetFieldValueAsString("Name")}\" Folder: {m.GetFieldValueAsString("BrightcoveFolder")}").ToList();

                    //Logger.Info(string.Join("\n", itemsModelsInAFolder));

                    //List<ItemModel> itemModelsInFolder = itemModels.Where(model => model.GetFieldValueAsString("BrightcoveFolder") == folderModel.Id).ToList();

                    Logger.Info($"Number of Sitecore videos in folder {folderItem.GetItemId()} ({folderModel.Id}): {itemModels.Count()}");

                }
                catch (Exception ex)
                {
                    Logger.Info(ex.Message);
                }
               
                return itemModels;
            }
        }
    }
}

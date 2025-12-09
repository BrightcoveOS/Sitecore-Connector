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
using Sitecore.Search;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework.Processors
{
    [RequiredEndpointPlugins(new Type[] { typeof(ItemModelRepositorySettings) })]
    public class ReadFolderVideoItemsPipelineStepProcessor : ReadAssetItemsPipelineStepProcessor
    {
        BrightcoveFolderVideosSettings folderSettings;
        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                BrightcoveFolderVideosSettings folderSettings = pipelineContext.GetCurrentPipelineBatch().GetPlugin<BrightcoveFolderVideosSettings>();

                this.folderSettings = folderSettings;

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
                ID templateId = new ID(templateGuids[0]);
                ItemModel folderItem = folderSettings.folderItem;

                var query = context.GetQueryable<SearchResultItem>()
                    .Where(x => x.Path.Contains(bucketPath) && x.Path != bucketPath && x.Language == language && x.TemplateId == templateId);

                var searchResults = query.ToList();

                itemModels = searchResults.Select(r => modelRepository.Get(r.ItemId.ToGuid(), language))
                    .Where(model => model != null && model.GetFieldValueAsGuid("BrightcoveFolder") == folderItem.GetItemId())
                    .ToList();

                Logger.Info($"number of vids in the folder: {folderSettings.videos.Count}");

                return itemModels;
            }
        }
    }
}

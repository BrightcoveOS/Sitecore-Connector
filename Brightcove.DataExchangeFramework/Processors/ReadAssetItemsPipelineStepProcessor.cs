using Brightcove.DataExchangeFramework.Helpers;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Processors
{
    [RequiredEndpointPlugins(new Type[] { typeof(ItemModelRepositorySettings) })]
    public class ReadAssetItemsPipelineStepProcessor : ReadSitecoreItemsStepProcessor
    {
        DateTime lastSyncFinishTime;

        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var syncSettings = pipelineContext.GetCurrentPipelineBatch().GetPlugin<BrightcoveSyncSettings>();

                if(syncSettings != null)
                {
                    lastSyncFinishTime = syncSettings.LastSyncFinishTime;
                }

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

        public override IEnumerable<ItemModel> GetSitecoreItemModels(
          Endpoint endpoint,
          ReadSitecoreItemModelsSettings readSitecoreItemModelsSettings,
          PipelineStep pipelineStep,
          PipelineContext pipelineContext,
          ILogger logger)
        {
            IItemModelRepository itemModelRepository = endpoint.GetPlugin<ItemModelRepositorySettings>().ItemModelRepository;
            string language = pipelineContext.GetPlugin<SelectedLanguagesSettings>()?.Languages?.FirstOrDefault() ?? "en";

            if (readSitecoreItemModelsSettings.ItemRootId == Guid.Empty)
            {
                logger.Error($"No root item specified {pipelineStep.Name}");
                return new List<ItemModel>();
            }

            string bucketPath = GetAssetParentItemMediaPath(pipelineContext);
            string indexName = $"sitecore_{itemModelRepository.DatabaseName}_index";

            return Search(bucketPath, indexName, readSitecoreItemModelsSettings.TemplateIds.ToList(), language, itemModelRepository);
        }

        public virtual IEnumerable<ItemModel> Search(string bucketPath, string indexName, List<Guid> templateGuids, string language, IItemModelRepository modelRepository)
        {
            var index = ContentSearchManager.GetIndex(indexName);

            using (var context = index.CreateSearchContext())
            {
                var query = context.GetQueryable<SearchResultItem>().Where(x => x.Path.Contains(bucketPath) && x.Path != bucketPath && x.Language == language);

                //Add support for filtering by 2 GUIDs for videos and video varaints (dont need more than 2 for now)
                if (templateGuids.Count == 2 && templateGuids[0] != Guid.Empty && templateGuids[1] != Guid.Empty)
                {
                    ID templateId1 = new ID(templateGuids[0]);
                    ID templateId2 = new ID(templateGuids[1]);
                    query = query.Where(x => x.TemplateId == templateId1 || x.TemplateId == templateId2);
                }
                else if (templateGuids.Count >= 1 && templateGuids[0] != Guid.Empty)
                {
                    ID templateId = new ID(templateGuids[0]);
                    query = query.Where(x => x.TemplateId == templateId);
                }

                if (lastSyncFinishTime != DateTime.MinValue)
                {
                    DateTime localTime = lastSyncFinishTime.ToLocalTime();
                    query = query.Where(x => x.Updated > localTime);
                }

                var searchResults = query.ToList();
                IEnumerable<ItemModel> itemModels = searchResults.Select(r => modelRepository.Get(r.ItemId.ToGuid(), language)).Where(m => m != null);
                this.Logger.Info("Identified " + itemModels.Count() + " sitecore items that have been modified since last sync "+lastSyncFinishTime);

                return itemModels;
            }
        }

        private string GetAssetParentItemMediaPath(PipelineContext context)
        {
            var settings = context.CurrentPipelineStep.GetPlugin<ResolveAssetItemSettings>();
            return settings.ParentItem.Paths.MediaPath;
        }
    }
}

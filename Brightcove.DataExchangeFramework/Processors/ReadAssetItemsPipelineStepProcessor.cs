using Brightcove.DataExchangeFramework.Settings;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
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
using static Sitecore.ContentSearch.Linq.Extensions.ReflectionExtensions;

namespace Brightcove.DataExchangeFramework.Processors
{
    [RequiredEndpointPlugins(new Type[] { typeof(ItemModelRepositorySettings) })]
    public class ReadAssetItemsPipelineStepProcessor : ReadSitecoreItemsStepProcessor
    {
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

            return Search(bucketPath, indexName, readSitecoreItemModelsSettings.TemplateIds.FirstOrDefault(), language, itemModelRepository);
        }

        public virtual IEnumerable<ItemModel> Search(string bucketPath, string indexName, Guid templateGuid, string language, IItemModelRepository modelRepository)
        {
            var index = ContentSearchManager.GetIndex(indexName);

            using (var context = index.CreateSearchContext())
            {
                var query = context.GetQueryable<SearchResultItem>().Where(x => x.Path.Contains(bucketPath) && x.Path != bucketPath && x.Language == language);

                if(templateGuid != Guid.Empty)
                {
                    ID templateId = new ID(templateGuid);
                    query = query.Where(x => x.TemplateId == templateId);
                }

                var searchResults = query.ToList();
                IEnumerable<ItemModel> itemModels = searchResults.Select(r => modelRepository.Get(r.ItemId.ToGuid(), language)).Where(m => m != null);

                return itemModels;
            }
        }

        private string GetAssetParentItemMediaPath(PipelineContext context)
        {
            var settings = context.CurrentPipelineStep.GetPlugin<ResolveAssetItemSettings>();
            var accountItem = Sitecore.Context.ContentDatabase.GetItem(settings.AcccountItemId);

            return accountItem.Paths.MediaPath + "/" + settings.RelativePath;
        }
    }
}

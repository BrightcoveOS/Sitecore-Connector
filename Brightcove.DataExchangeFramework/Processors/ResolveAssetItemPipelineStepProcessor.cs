using System;
using System.Linq;
using Brightcove.DataExchangeFramework.SearchResults;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Buckets.Managers;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Local.Extensions;
using Sitecore.DataExchange.Providers.Sc.DataAccess.Readers;
using Sitecore.DataExchange.Providers.Sc.Plugins;
using Sitecore.DataExchange.Providers.Sc.Processors.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class ResolveAssetItemPipelineStepProcessor : ResolveSitecoreItemStepProcessor
    {
        protected override ItemModel DoSearch(object value, ResolveSitecoreItemSettings resolveItemSettings, IItemModelRepository repository, PipelineContext pipelineContext, ILogger logger)
        {
            var valueReader = resolveItemSettings.MatchingFieldValueAccessor?.ValueReader as SitecoreItemFieldReader;

            if (valueReader == null)
            {
                return null;
            }

            //We need to store the resolve asset item plugin in the global Sitecore.DataExchangeContext so it
            //can be used in the VideoIdsPropertyValueReader
            if (Sitecore.DataExchange.Context.GetPlugin<ResolveAssetItemSettings>() == null)
            {
                Sitecore.DataExchange.Context.Plugins.Add(pipelineContext.CurrentPipelineStep.GetPlugin<ResolveAssetItemSettings>());
            }

            string parentItemPath = GetAssetParentItemPath(pipelineContext);
            string parentItemMediaPath = GetAssetParentItemMediaPath(pipelineContext);

            Database database = Sitecore.Configuration.Factory.GetDatabase(repository.DatabaseName);
            Item parentItem = database?.GetItem(parentItemPath);

            if (parentItem == null)
            {
                return null;
            }

            string fieldName = valueReader.FieldName;
            string convertedValue = this.ConvertValueForSearch(value);

            if (BucketManager.IsBucket(parentItem))
            {
                IProviderSearchContext searchContext = ContentSearchManager.GetIndex($"sitecore_{repository.DatabaseName}_index").CreateSearchContext();

                //Since we must search the index becasue the target folder is a bucket the items must have a field called 'ID' that can be used to identify them
                AssetSearchResult searchResult = searchContext.GetQueryable<AssetSearchResult>().FirstOrDefault(x => x.Path.Contains(parentItemMediaPath) && x.ID == convertedValue);
                return searchResult?.GetItem()?.GetItemModel();
            }

            return parentItem.Children?.Where(c => c[fieldName] == convertedValue)?.FirstOrDefault()?.GetItemModel();
        }

        protected override Guid GetParentItemIdForNewItem(
          IItemModelRepository repository,
          ResolveSitecoreItemSettings settings,
          PipelineContext pipelineContext,
          ILogger logger)
        {
            return Sitecore.Context.ContentDatabase.GetItem(GetAssetParentItemPath(pipelineContext)).ID.Guid;
        }

        private string GetAssetParentItemPath(PipelineContext context)
        {
            var settings = context.CurrentPipelineStep.GetPlugin<ResolveAssetItemSettings>();
            var accountItem = Sitecore.Context.ContentDatabase.GetItem(settings.AcccountItemId);

            return accountItem.Paths.Path + "/" + settings.RelativePath;
        }

        private string GetAssetParentItemMediaPath(PipelineContext context)
        {
            var settings = context.CurrentPipelineStep.GetPlugin<ResolveAssetItemSettings>();
            var accountItem = Sitecore.Context.ContentDatabase.GetItem(settings.AcccountItemId);

            return accountItem.Paths.MediaPath + "/" + settings.RelativePath;
        }
    }
}
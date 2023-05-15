using System;
using System.Collections.Generic;
using System.Linq;
using Brightcove.DataExchangeFramework.SearchResults;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Buckets.Managers;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Local.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Providers.Sc.DataAccess.Readers;
using Sitecore.DataExchange.Providers.Sc.Plugins;
using Sitecore.DataExchange.Providers.Sc.Processors.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using Sitecore.DataExchange.Providers.Sc.Extensions;
using Sitecore.Collections;
using Sitecore.Globalization;

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

            string language = pipelineContext.GetPlugin<SelectedLanguagesSettings>()?.Languages?.FirstOrDefault() ?? "en";
            string parentItemPath = GetAssetParentItemPath(pipelineContext);
            string parentItemMediaPath = GetAssetParentItemMediaPath(pipelineContext);

            Database database = Sitecore.Configuration.Factory.GetDatabase(repository.DatabaseName);
            Item parentItem = database?.GetItem(parentItemPath, Language.Parse(language));

            if (parentItem == null)
            {
                return null;
            }

            string fieldName = valueReader.FieldName;
            string convertedValue = this.ConvertValueForSearch(value);
            ItemModel resolvedItem = null;

            if (BucketManager.IsBucket(parentItem))
            {
                IProviderSearchContext searchContext = ContentSearchManager.GetIndex($"sitecore_{repository.DatabaseName}_index").CreateSearchContext();

                //Since we must search the index becasue the target folder is a bucket the items must have a field called 'ID' that can be used to identify them
                AssetSearchResult searchResult = searchContext.GetQueryable<AssetSearchResult>().FirstOrDefault(x => x.Path.Contains(parentItemMediaPath) && x.ID == convertedValue && x.Language == language);
                resolvedItem = searchResult?.GetItem()?.GetItemModel();
            }
            else
            {
                resolvedItem = parentItem.Children?.Where(c => c[fieldName] == convertedValue)?.FirstOrDefault()?.GetItemModel();
            }

            //Make sure we update the item name if it has changed. (The name is initially set as part of the CreateNewItem method)
            if(resolvedItem != null)
            {
                string modelName = GetModelName(pipelineContext.CurrentPipelineStep, pipelineContext, logger, resolveItemSettings);

                if(!string.IsNullOrWhiteSpace(modelName) && modelName != (string)resolvedItem["ItemName"])
                {
                    resolvedItem["ItemName"] = modelName;
                }
            }

            return resolvedItem;
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

        protected override Guid GetParentItemIdForNewItem(IItemModelRepository repository, ResolveSitecoreItemSettings settings, PipelineContext pipelineContext, ILogger logger)
        {
            return Sitecore.Context.ContentDatabase.GetItem(GetAssetParentItemPath(pipelineContext)).ID.Guid;
        }

        public override object CreateNewObject(object identifierValue, PipelineStep pipelineStep, PipelineContext pipelineContext, ILogger logger)
        {
            if (identifierValue == null)
                throw new ArgumentException("The value cannot be null.", nameof(identifierValue));
            Endpoint endpoint = this.GetEndpoint(pipelineStep, pipelineContext, logger);
            if (endpoint == null)
                throw new ArgumentNullException("endpoint");
            if (pipelineStep == null)
                throw new ArgumentNullException(nameof(pipelineStep));
            if (pipelineContext == null)
                throw new ArgumentNullException(nameof(pipelineContext));
            IItemModelRepository repositoryFromEndpoint = this.GetItemModelRepositoryFromEndpoint(endpoint);
            if (repositoryFromEndpoint == null)
                return (object)null;
            ResolveSitecoreItemSettings sitecoreItemSettings = pipelineStep.GetResolveSitecoreItemSettings();
            if (sitecoreItemSettings == null)
                return (object)null;
            ItemModel newItem = this.CreateNewItem(this.GetIdentifierObject(pipelineStep, pipelineContext, logger), repositoryFromEndpoint, sitecoreItemSettings, pipelineContext, logger);
            this.SetRepositoryStatusSettings(RepositoryObjectStatus.DoesNotExist, pipelineContext);
            return (object)newItem;
        }

        private IItemModelRepository GetItemModelRepositoryFromEndpoint(Endpoint endpoint)
        {
            return endpoint.GetItemModelRepositorySettings()?.ItemModelRepository;
        }

        private ItemModel CreateNewItem(object identifierObject, IItemModelRepository repository, ResolveSitecoreItemSettings settings, PipelineContext pipelineContext, ILogger logger)
        {
            IValueReader valueReader = this.GetValueReader(settings.ItemNameValueAccessor);
            if (valueReader == null)
                return (ItemModel)null;

            DataAccessContext context = new DataAccessContext();
            string validItemName = this.ConvertValueToValidItemName(this.ReadValue(identifierObject, valueReader, context), pipelineContext, logger);
            if (validItemName == null)
                return (ItemModel)null;

            string language = pipelineContext.GetPlugin<SelectedLanguagesSettings>()?.Languages?.FirstOrDefault() ?? "en";

            Guid itemIdForNewItem = this.GetParentItemIdForNewItem(repository, settings, pipelineContext, logger);

            if (settings.DoNotCreateItemIfDoesNotExist)
            {
                ItemModel itemModel = new ItemModel();
                itemModel.Add("ItemName", (object)validItemName);
                itemModel.Add("TemplateID", (object)settings.TemplateForNewItem);
                itemModel.Add("ParentID", (object)itemIdForNewItem);
                itemModel.Add("ItemLanguage", (object)language);
                return itemModel;
            }

            Guid id = repository.Create(validItemName, settings.TemplateForNewItem, itemIdForNewItem, language);

            return repository.Get(id, language);
        }

        private string ConvertValueToValidItemName(
          object value,
          PipelineContext pipelineContext,
          ILogger logger)
        {
            if (value == null)
                return (string)null;
            string str = value.ToString();
            SitecoreItemUtilities plugin = Context.GetPlugin<SitecoreItemUtilities>();
            if (plugin == null)
            {
                this.Log(new Action<string>(logger.Error), pipelineContext, "No plugin is specified on the context to determine whether or not the specified value is a valid item name. The original value will be used.", new string[1]
                {
          "missing plugin: " + typeof (SitecoreItemUtilities).FullName
                });
                return str;
            }
            if (plugin.IsItemNameValid == null)
            {
                this.Log(new Action<string>(logger.Error), pipelineContext, "No delegate is specified on the plugin that can determine whether or not the specified value is a valid item name. The original value will be used.", new string[3]
                {
          "plugin: " + typeof (SitecoreItemUtilities).FullName,
          "delegate: IsItemNameValid",
          "original value: " + str
                });
                return str;
            }
            if (plugin.IsItemNameValid(str))
                return str;
            if (plugin.ProposeValidItemName != null)
                return plugin.ProposeValidItemName(str);
            logger.Error("No delegate is specified on the plugin that can propose a valid item name. The original value will be used. (plugin: {0}, delegate: {1}, original value: {2})", (object)typeof(SitecoreItemUtilities).FullName, (object)"ProposeValidItemName", (object)str);
            return str;
        }

        private IValueReader GetValueReader(IValueAccessor config) => config?.ValueReader;

        private object ReadValue(object source, IValueReader reader, DataAccessContext context)
        {
            if (reader == null)
                return (object)null;
            ReadResult readResult = reader.Read(source, context);
            return !readResult.WasValueRead ? (object)null : readResult.ReadValue;
        }

        protected override object ConvertValueToIdentifier(
          object identifierValue,
          PipelineStep pipelineStep,
          PipelineContext pipelineContext,
          ILogger logger)
        {
            return identifierValue;
        }

        private string GetModelName(PipelineStep pipelineStep, PipelineContext pipelineContext, ILogger logger, ResolveSitecoreItemSettings settings)
        {
            object identifierObject = this.GetIdentifierObject(pipelineStep, pipelineContext, logger);

            IValueReader valueReader = this.GetValueReader(settings.ItemNameValueAccessor);
            if (valueReader == null)
                return null;

            DataAccessContext context = new DataAccessContext();
            string validItemName = this.ConvertValueToValidItemName(this.ReadValue(identifierObject, valueReader, context), pipelineContext, logger);
            if (validItemName == null)
                return null;

            return validItemName;
        }
    }
}
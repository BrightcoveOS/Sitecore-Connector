using System;
using System.Collections.Generic;
using System.Linq;
using Brightcove.DataExchangeFramework.SearchResults;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Buckets.Managers;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Local.Extensions;
using Sitecore.Diagnostics;
using Sitecore.Services.Core.Model;
using Sitecore.DataExchange.Extensions;

namespace Brightcove.DataExchangeFramework.ValueReaders
{
    public class FolderPropertyValueReader : IValueReader
    {
        public FolderPropertyValueReader(string propertyName)
        {
            PropertyName = !string.IsNullOrWhiteSpace(propertyName) ? propertyName : throw new ArgumentOutOfRangeException(nameof(propertyName), (object)propertyName, "Property name must be specified.");
            ReflectionUtil = Sitecore.DataExchange.DataAccess.Reflection.ReflectionUtil.Instance;
        }

        public string PropertyName { get; private set; }

        public IReflectionUtil ReflectionUtil { get; set; }

        private ItemModel FindMatchingBrightcoveFolder(Item parentFoldersItem, string brightcoveFolderId)
        {
            if (BucketManager.IsBucket(parentFoldersItem))
            {
                ItemRepositorySettings repositorySettings = Sitecore.DataExchange.Context.GetPlugin<ItemRepositorySettings>();
                IProviderSearchContext searchContext = repositorySettings.searchContext;

                List<AssetSearchResult> searchResults = searchContext.GetQueryable<AssetSearchResult>().Where(x => x.ID == brightcoveFolderId).ToList();

                return searchResults.FirstOrDefault()?.GetItem()?.GetItemModel();
            }
            else
            {
                return parentFoldersItem.Children?.Where(c => c["ID"] == brightcoveFolderId)?.FirstOrDefault()?.GetItemModel();
            }
        }

        public virtual ReadResult Read(object source, DataAccessContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            
            string returnValue = "";
            bool wasValueRead;
            try
            {
                var reader = new ChainedPropertyValueReader(PropertyName);
                var readResult = reader.Read(source, context);

                wasValueRead = readResult.WasValueRead;
                object property = readResult.ReadValue;

                if (wasValueRead)
                {
                    string brightcoveFolderId = property as string;
                    if (!string.IsNullOrWhiteSpace(brightcoveFolderId))
                    {
                        Item parentFoldersItem = GetParentFoldersItem();
                        ItemModel sitecoreFolder = FindMatchingBrightcoveFolder(parentFoldersItem, brightcoveFolderId);

                        returnValue = sitecoreFolder?.GetItemId().ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, this);
                wasValueRead = false;
            }

            return new ReadResult(DateTime.UtcNow)
            {
                WasValueRead = wasValueRead,
                ReadValue = returnValue
            };
        }

        //Gets the Sitecore folder that holds all of the Brightcove folder items (Yes both are called folders)
        private Item GetParentFoldersItem()
        {
            return Sitecore.DataExchange.Context.GetPlugin<ResolveAssetItemSettings>().ParentItem;
        }
    }
}

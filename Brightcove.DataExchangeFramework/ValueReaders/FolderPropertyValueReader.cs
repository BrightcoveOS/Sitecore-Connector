using Brightcove.DataExchangeFramework.SearchResults;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.DataAccess.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Brightcove.DataExchangeFramework.ValueReaders
{
    public class FolderPropertyValueReader : IValueReader
    {
        public FolderPropertyValueReader(string propertyName)
        {
            this.PropertyName = !string.IsNullOrWhiteSpace(propertyName) ? propertyName : throw new ArgumentOutOfRangeException(nameof(propertyName), (object)propertyName, "Property name must be specified.");
            this.ReflectionUtil = (IReflectionUtil)global::Sitecore.DataExchange.DataAccess.Reflection.ReflectionUtil.Instance;
        }

        public string PropertyName { get; private set; }

        public IReflectionUtil ReflectionUtil { get; set; }

        public virtual ReadResult Read(object source, DataAccessContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            bool wasValueRead = false;
            object obj = source;

            var reader = new ChainedPropertyValueReader(PropertyName);
            var result = reader.Read(obj, context);

            wasValueRead = result.WasValueRead;
            obj = result.ReadValue;

            try
            {
                if (wasValueRead)
                {
                    string folderId = obj as string;
                    List<string> videoItemIds = new List<string>();

                    if (!string.IsNullOrWhiteSpace(folderId))
                    {
                        string accountPath = GetAssetParentItemMediaPath();

                        using (var index = ContentSearchManager.GetIndex("sitecore_master_index").CreateSearchContext())
                        {
                            //Note we need to limit our search to only videos in the current account but we need
                            //context information to know which account is currently being synced. We use the
                            //Sitecore.DataExchange.Context to pass this information to this value reader
                            string folderItemId = index.GetQueryable<AssetSearchResult>()
                                .Where(r => r.Path.Contains(accountPath) && r.ID == folderId)
                                .Select(r => r.ItemId.ToString())
                                .FirstOrDefault();

                            obj = folderItemId ?? "";
                        }
                    }
                    else
                    {
                        obj = "";
                    }
                }
            }
            catch
            {
                wasValueRead = false;
                obj = null;
            }

            if(!wasValueRead || obj == null)
            {
                wasValueRead = true;
                obj = "";
            }

            return new ReadResult(DateTime.UtcNow)
            {
                WasValueRead = wasValueRead,
                ReadValue = obj
            };
        }

        private string GetAssetParentItemMediaPath()
        {
            var settings = Sitecore.DataExchange.Context.GetPlugin<ResolveAssetItemSettings>();
            var accountItem = Sitecore.Context.ContentDatabase.GetItem(settings.AcccountItemId);

            return accountItem.Paths.MediaPath;
        }
    }
}

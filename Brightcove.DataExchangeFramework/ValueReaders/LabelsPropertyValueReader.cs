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
    public class LabelsPropertyValueReader : IValueReader
    {
        public LabelsPropertyValueReader(string propertyName)
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
                if (wasValueRead && obj != null)
                {
                    List<string> labels = obj as List<string>;
                    List<string> labelItemIds = new List<string>();

                    if (labels.Count > 0)
                    {
                        string accountPath = GetAssetParentItemMediaPath();
                        ID labelTemplate = new ID("{8B6A9EAD-4DE9-4C59-B8F5-F3FF40A64F12}");

                        using (var index = ContentSearchManager.GetIndex("sitecore_master_index").CreateSearchContext())
                        {
                            foreach (var label in labels)
                            {
                                var searchResult = index.GetQueryable<AssetSearchResult>()
                                    .Where(r => r.Path.Contains(accountPath) && r.TemplateId == labelTemplate && r.Label == label)
                                    .FirstOrDefault();

                                string labelItemId = searchResult.ItemId.ToString();

                                if(labelItemId != null)
                                {
                                    labelItemIds.Add(labelItemId);
                                }
                            }

                            obj = string.Join("|", labelItemIds);
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

            return new ReadResult(DateTime.UtcNow)
            {
                WasValueRead = wasValueRead,
                ReadValue = obj
            };
        }

        private string GetAssetParentItemMediaPath()
        {
            //Note we need to limit our search to only assets in the current account but we need
            //context information to know which account is currently being synced. We use the
            //Sitecore.DataExchange.Context to pass this information to this value reader
            var settings = Sitecore.DataExchange.Context.GetPlugin<ResolveAssetItemSettings>();
            var accountItem = Sitecore.Context.ContentDatabase.GetItem(settings.AcccountItemId);

            return accountItem.Paths.MediaPath;
        }
    }
}

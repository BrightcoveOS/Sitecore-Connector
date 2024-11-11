using Brightcove.DataExchangeFramework.SearchResults;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
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
            object property = null;
            string returnValue = "";

            try
            {
                var reader = new ChainedPropertyValueReader(PropertyName);
                var result = reader.Read(source, context);

                wasValueRead = result.WasValueRead;
                property = result.ReadValue;

                if (wasValueRead && property != null)
                {
                    List<string> labels = property as List<string>;
                    List<string> labelItemIds = new List<string>();

                    if (labels.Count > 0)
                    {
                        Item parentLabelsItem = GetParentLabelsItem();

                        foreach (var label in labels)
                        {
                            string labelItemId = parentLabelsItem.Children?.Where(c => c["Label"] == label)?.FirstOrDefault()?.ID?.ToString() ?? "";

                            if(!string.IsNullOrWhiteSpace(labelItemId))
                            {
                                labelItemIds.Add(labelItemId);
                            }
                        }

                        returnValue = string.Join("|", labelItemIds);
                    }
                }
            }
            catch
            {
                wasValueRead = false;
                returnValue = null;
            }

            return new ReadResult(DateTime.UtcNow)
            {
                WasValueRead = wasValueRead,
                ReadValue = returnValue
            };
        }

        private Item GetParentLabelsItem()
        {
            return Sitecore.DataExchange.Context.GetPlugin<ResolveAssetItemSettings>().ParentItem;
        }
    }
}

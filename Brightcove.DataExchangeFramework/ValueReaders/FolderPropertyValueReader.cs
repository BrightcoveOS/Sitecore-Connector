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
            object property = null;
            string returnValue = "";

            try
            {
                var reader = new ChainedPropertyValueReader(PropertyName);
                var readResult = reader.Read(source, context);

                wasValueRead = readResult.WasValueRead;
                property = readResult.ReadValue;

                if (wasValueRead)
                {
                    string brightcoveFolderId = property as string;

                    if (!string.IsNullOrWhiteSpace(brightcoveFolderId))
                    {
                        Item parentFoldersItem = GetParentFoldersItem();
                        string sitecoreFolderId = parentFoldersItem.Children?.Where(c => c["ID"] == brightcoveFolderId)?.FirstOrDefault()?.ID?.ToString() ?? "";
                        returnValue = sitecoreFolderId;
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

        //Gets the Sitecore folder that holds all of the Brightcove folder items (Yes both are called folders)
        private Item GetParentFoldersItem()
        {
            return Sitecore.DataExchange.Context.GetPlugin<ResolveAssetItemSettings>().ParentItem;
        }
    }
}

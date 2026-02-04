using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Helpers
{
    public static class ItemUpdater
    {
        public static bool Update(IItemModelRepository itemModelRepository, ItemModel itemModel)
        {
            if (itemModelRepository == null)
            {
                throw new ArgumentNullException(nameof(itemModelRepository));
            }

            if (itemModel == null)
            {
                throw new ArgumentNullException(nameof(itemModel));
            }

            // TODO: ItemModel may be broken...
            FixItemModel(itemModel);

            string language = itemModel.ContainsKey("ItemLanguage") ? itemModel["ItemLanguage"].ToString() : string.Empty;
            Guid id = itemModel.GetItemId();
            bool flag = false;

            if (id == Guid.Empty)
            {
                id = itemModelRepository.Create(itemModel);

                if (id != Guid.Empty)
                {
                    flag = true;
                    itemModel["ItemID"] = (object)id;
                }
            }
            else
            {
                flag = itemModelRepository.Update(id, itemModel, language);
            }

            return flag;
        }

        private static void FixItemModel(ItemModel itemModel)
        {
            if (itemModel == null)
                return;

            //Convert all non-null values to strings
            foreach (string key in itemModel.Keys.ToArray())
            {
                object obj = itemModel[key];

                if (obj != null)
                    itemModel[key] = obj.ToString();
            }
        }
    }
}

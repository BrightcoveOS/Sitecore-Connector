using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.DataExchangeFramework.Extensions
{
    public static class ItemModelExtensions
    {
        public static string GetLanguage(this ItemModel model)
        {
            object language = null;

            if(!model.TryGetValue(ItemModel.ItemLanguage, out language))
            {
                language = "en";
            }

            return (string)language;
        }
    }
}

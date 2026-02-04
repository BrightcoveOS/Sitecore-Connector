using Sitecore.Data.Items;
using Sitecore.DataExchange;

namespace Brightcove.DataExchangeFramework.Settings
{
    public class ResolveAssetItemSettings : IPlugin
    {
        public string AcccountItemId { get; set; } = "";

        public string RelativePath { get; set; } = "";

        public Item AccountItem { get; set; }

        public Item ParentItem { get; set; }
    }
}
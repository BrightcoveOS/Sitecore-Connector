using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using System.Runtime.Serialization;

namespace Brightcove.DataExchangeFramework.SearchResults
{
    public class AssetSearchResult : SearchResultItem
    {
        [DataMember]
        [IndexField("id_t")]
        public string ID { get; set; }

        [DataMember]
        [IndexField("label_t")]
        public string Label { get; set; }
    }
}

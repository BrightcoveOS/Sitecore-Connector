using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

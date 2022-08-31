using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Brightcove.Core.Models
{
    public class Folder
    {
        [JsonProperty("account_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountId { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("video_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? VideoCount { get; set; }

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreationDate { get; set; }

        [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? UpdatedDate { get; set; }

        [JsonIgnore()]
        public DateTime LastSyncTime { get; set; }

        public Folder ShallowCopy()
        {
            return (Folder)this.MemberwiseClone();
        }
    }
}

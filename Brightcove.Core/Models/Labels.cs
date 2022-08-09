using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Brightcove.Core.Models
{
    public class Labels
    {
        [JsonProperty("account_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountId { get; set; }

        [JsonProperty("labels", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Paths { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public int Version { get; set; }

        public Labels ShallowCopy()
        {
            return (Labels)this.MemberwiseClone();
        }
    }
}

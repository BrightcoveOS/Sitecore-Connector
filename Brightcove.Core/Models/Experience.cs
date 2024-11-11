using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Brightcove.Core.Models
{
    /// <summary>
    /// Represents a experience object from the Brightcove API
    /// For more information, see https://apis.support.brightcove.com/ipx/references/reference.html#operation/GetExperiences
    /// </summary>
    public class Experience : Asset
    {
        [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreationDate { get; set; }

        [JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
        public new DateTime? LastModifiedDate { get; set; }

        [JsonProperty("publishedUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        public Experience ShallowCopy()
        {
            return (Experience)this.MemberwiseClone();
        }
    }

    public class ExperienceList
    {
        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Experience> Items { get; set; }
    }
}

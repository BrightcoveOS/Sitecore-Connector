using Newtonsoft.Json;
using System;

namespace Brightcove.Core.Models
{
    public class VideoSchedule
    {
        [JsonProperty("starts_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartsAt { get; set; }

        [JsonProperty("ends_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndsAt { get; set; }
    }
}

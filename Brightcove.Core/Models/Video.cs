using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Brightcove.Core.Models
{
    /// <summary>
    /// Represents a video object from the Brightcove API
    /// For more information, see http://support.brightcove.com/en/video-cloud/docs/media-api-objects-reference#Video
    /// </summary>
    public class Video : Asset
    {
        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreationDate { get; set; }

        [JsonProperty("long_description", NullValueHandling = NullValueHandling.Ignore)]
        public string LongDescription { get; set; }

        [JsonProperty("published_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PublishedDate { get; set; }

        [JsonProperty("economics", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Economics? Economics { get; set; }

        [JsonProperty("link", NullValueHandling = NullValueHandling.Ignore)]
        public VideoLink Link { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Tags { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemState? ItemState { get; set; }

        [JsonProperty("custom_fields", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> CustomFields { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public int? Duration { get; set; }

        [JsonProperty("text_tracks", NullValueHandling = NullValueHandling.Ignore)]
        public Collection<VideoTextTrack> TextTracks { get; set; }

        [JsonProperty("sharing", NullValueHandling = NullValueHandling.Ignore)]
        public VideoSharing Sharing { get; set; }

        [JsonProperty("labels", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Labels { get; set; }

        [JsonProperty("folder_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Folder { get; set; }

        [JsonIgnore]
        public string IngestJobId { get; set; }

        [JsonIgnore]
        public bool IngestSuccessful { get; set; }

        [JsonProperty("schedule", NullValueHandling = NullValueHandling.Ignore)]
        public VideoSchedule Schedule { get; set; }

        [JsonProperty("variants", NullValueHandling = NullValueHandling.Ignore)]
        public List<VideoVariant> Variants { get; set; }

        public Video ShallowCopy()
        {
            return (Video)this.MemberwiseClone();
        }
    }
}

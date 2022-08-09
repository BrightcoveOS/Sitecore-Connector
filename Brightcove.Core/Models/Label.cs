using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Brightcove.Core.Models
{
    public class Label
    {
        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        public Label ShallowCopy()
        {
            return (Label)this.MemberwiseClone();
        }
    }
}

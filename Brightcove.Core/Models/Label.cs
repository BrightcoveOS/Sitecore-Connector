using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace Brightcove.Core.Models
{
    public class Label
    {
        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty("new_label", NullValueHandling = NullValueHandling.Ignore)]
        public string NewLabel { get; set; }

        [JsonIgnore()]
        public string SitecoreName { get; set; }

        [JsonIgnore()]
        public DateTime LastSyncTime { get; set; }

        public Label()
        {

        }

        public Label(string path)
        {
            Path = AddTrailingSlash(path);
            SitecoreName = Path.Replace("/", "_");
        }

        public Label ShallowCopy()
        {
            return (Label)this.MemberwiseClone();
        }

        public static bool TryParse(string path, out Label label)
        {
            if(path.Length <= 1)
            {
                label = null;
                return false;
            }

            if (path[0] != '/')
            {
                label = null;
                return false;
            }

            label = new Label(path);
            return true;
        }

        public string GetLeafLabel()
        {
            return Path.Split('/').Last();
        }

        private string AddTrailingSlash(string path)
        {
            if (path[path.Length - 1] != '/')
            {
                return path + '/';
            }

            return path;
        }
    }
}
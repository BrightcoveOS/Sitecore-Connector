using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Brightcove.Core.EmbedGenerator.Models;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Presentation;

namespace Brightcove.Web.Model
{
    [Serializable]
    public class EmbedRenderingParameters
    {
        public NameValueCollection Parameters { get; protected set; }

        public EmbedRenderingParameters()
        {
            this.Parameters = new NameValueCollection();
        }

        public EmbedRenderingParameters(Dictionary<string, string> dictionary) : this()
        {
            foreach (var pair in dictionary)
            {
                this.Parameters.Add(pair.Key, pair.Value);
            }
        }

        public EmbedRenderingParameters(NameValueCollection collection)
        {
            Assert.ArgumentNotNull(collection, "collection");

            this.Parameters = new NameValueCollection(collection);
        }

        public string AccountId
        {
            get
            {
                return GetString("accountId");
            }
        }

        public string PlayerId
        {
            get
            {
                return GetString("playerId");
            }
        }

        public string MediaId
        {
            get
            {
                return GetString("mediaId");
            }
        }

        public int Width
        {
            get
            {
                return GetInt("width", 960);
            }
        }

        public int Height
        {
            get
            {
                return GetInt("height", 540);
            }
        }

        public bool IsPlaylist
        {
            get
            {
                return GetBoolean("isPlaylist");
            }
        }

        public bool IsFixed
        {
            get
            {
                return GetBoolean("isFixedSize");
            }
        }

        public bool IsJavascriptEmbed
        {
            get
            {
                return GetBoolean("isJavascriptEmbed");
            }
        }

        public EmbedModel CreateEmbedModel()
        {
            EmbedModel embedModel = new EmbedModel();

            embedModel.AccountId = AccountId;
            embedModel.PlayerId = PlayerId;
            embedModel.MediaId = MediaId;
            embedModel.Height = Height;
            embedModel.Width = Width;

            if(IsPlaylist)
            {
                embedModel.MediaType = MediaType.Playlist;
            }
            else
            {
                embedModel.MediaType = MediaType.Video;
            }

            if(IsFixed)
            {
                embedModel.MediaSizing = MediaSizing.Fixed;
            }
            else
            {
                embedModel.MediaSizing = MediaSizing.Responsive;
            }

            if(IsJavascriptEmbed)
            {
                embedModel.EmbedType = EmbedType.JavaScript;
            }
            else
            {
                embedModel.EmbedType = EmbedType.Iframe;
            }

            return embedModel;
        }

        public override string ToString()
        {
            return string.Join("&", this.Parameters.AllKeys.Select(i => i + "=" + this.Parameters[i]));
        }

        private string GetString(string key)
        {
            return this.Parameters[key] ?? "";
        }

        private int GetInt(string key, int defaultValue)
        {
            int result = 0;

            if (int.TryParse(Parameters[key], out result))
            {
                return result;
            }

            return defaultValue;
        }

        private bool GetBoolean(string key)
        {
            if(Parameters[key] == "1")
            {
                return true;
            }

            return false;
        }
    }
}
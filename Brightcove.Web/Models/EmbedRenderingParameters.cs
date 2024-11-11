using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Brightcove.Constants;
using Brightcove.Core.EmbedGenerator.Models;
using Brightcove.MediaFramework.Brightcove;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Presentation;

namespace Brightcove.Web.Models
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

        public EmbedRenderingParameters(EmbedModel model) : this()
        {
            AccountId = model.AccountId;
            PlayerId = model.PlayerId;
            MediaId = model.MediaId;
            Width = model.Width;
            Height = model.Height;
            IsPlaylist = model.MediaType == MediaType.Playlist;
            Autoplay = model.Autoplay;
            Muted = model.Muted;
            Language = model.Language;
            Title = model.Title;

            if(model.MediaSizing == MediaSizing.Fixed)
            {
                Sizing = PlayerParameters.SizingFixed;
            }
            else
            {
                Sizing = PlayerParameters.SizingResponsive;
            }

            if(model.EmbedType == EmbedType.JavaScript)
            {
                Embed = PlayerParameters.EmbedJavascript;
            }
            else
            {
                Embed = PlayerParameters.EmbedIframe;
            }
        }

        public string AccountId
        {
            get
            {
                return GetString("accountId");
            }
            set
            {
                this.Parameters["accountId"] = value;
            }
        }

        public string PlayerId
        {
            get
            {
                return GetString("brightcovePlayerId");
            }
            set
            {
                this.Parameters["brightcovePlayerId"] = value;
            }
        }

        public string MediaId
        {
            get
            {
                return GetString("brightcoveMediaId");
            }
            set
            {
                this.Parameters["brightcoveMediaId"] = value;
            }
        }

        public int Width
        {
            get
            {
                return GetInt("width", 960);
            }
            set
            {
                this.Parameters["width"] = value.ToString();
            }
        }

        public int Height
        {
            get
            {
                return GetInt("height", 540);
            }
            set
            {
                this.Parameters["height"] = value.ToString();
            }
        }

        public bool IsPlaylist
        {
            get
            {
                return GetBoolean("isPlaylist");
            }
            set
            {
                this.Parameters["isPlaylist"] = (value ? "1" : "0");
            }
        }

        public string Sizing
        {
            get
            {
                return GetString("sizing");
            }
            set
            {
                this.Parameters["sizing"] = value;
            }
        }

        public string Embed
        {
            get
            {
                return GetString("embed");
            }
            set
            {
                this.Parameters["embed"] = value;
            }
        }

        public bool Autoplay
        {
            get
            {
                return GetBoolean("autoplay");
            }
            set
            {
                this.Parameters["autoplay"] = (value ? "1" : "0");
            }
        }

        public bool Muted
        {
            get
            {
                return GetBoolean("muted");
            }
            set
            {
                this.Parameters["muted"] = (value ? "1" : "0");
            }
        }

        public string Language
        {
            get
            {
                return GetString("lang");
            }
            set
            {
                this.Parameters["lang"] = value;
            }
        }

        public string Title
        {
            get
            {
                return GetString("title");
            }
            set
            {
                this.Parameters["title"] = value;
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
            embedModel.Muted = Muted;
            embedModel.Autoplay = Autoplay;
            embedModel.Language = Language;
            embedModel.Title = Title;

            if (IsPlaylist)
            {
                embedModel.MediaType = MediaType.Playlist;
            }
            else
            {
                embedModel.MediaType = MediaType.Video;
            }

            if(Sizing == PlayerParameters.SizingFixed)
            {
                embedModel.MediaSizing = MediaSizing.Fixed;
            }
            else
            {
                embedModel.MediaSizing = MediaSizing.Responsive;
            }

            if(Embed == PlayerParameters.EmbedJavascript)
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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
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

            if(model.MediaSizing == MediaSizing.Fixed)
            {
                Sizing = Constants.SizingFixed;
            }
            else
            {
                Sizing = Constants.SizingResponsive;
            }

            if(model.EmbedType == EmbedType.JavaScript)
            {
                Embed = Constants.EmbedJavascript;
            }
            else
            {
                Embed = Constants.EmbedIframe;
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

            if(Sizing == Constants.SizingFixed)
            {
                embedModel.MediaSizing = MediaSizing.Fixed;
            }
            else
            {
                embedModel.MediaSizing = MediaSizing.Responsive;
            }

            if(Embed == Constants.EmbedJavascript)
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
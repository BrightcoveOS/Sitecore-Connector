using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

using Sitecore.MediaFramework.Pipelines.MediaGenerateMarkup;
using Sitecore.MediaFramework.Players;
using Sitecore.StringExtensions;
using Sitecore.Xml;
using Sitecore.Configuration;
using System.Reflection;
using Sitecore.Text;
using Sitecore.Globalization;
using Sitecore;
using Sitecore.Rules;
using Sitecore.MediaFramework.Rules.Analytics;
using Newtonsoft.Json;

// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The Brightcove player markup provider.
// </summary>                                  
// ------------------------------------------------------------ --------------------------------------------------------

namespace Brightcove.MediaFramework.Brightcove.Players
{
    /// <summary>
    /// The Brightcove player markup provider.
    /// </summary>
    public class BrightcovePlayerMarkupGenerator : PlayerMarkupGeneratorBase
    {
        protected readonly Dictionary<string, string> DefaultParameters;

        public string ScriptUrl = "https://players.brightcove.net/{account_id}/{player_id}_default/index.min.js";
        private const string AccountToken = "{account_id}";
        private const string PlayerToken = "{player_id}";
        private const string ScriptTagTemplate = "<script type='text/javascript' src='{0}'></script>";

        public BrightcovePlayerMarkupGenerator()
        {
            this.DefaultParameters = new Dictionary<string, string>();
        }

        public void AddParameter(XmlNode configNode)
        {
            Assert.ArgumentNotNull((object)configNode, "configNode");
            string attribute1 = XmlUtil.GetAttribute("name", configNode);
            string attribute2 = XmlUtil.GetAttribute("value", configNode);
            if (string.IsNullOrEmpty(attribute1) || string.IsNullOrEmpty(attribute2))
                return;
            this.DefaultParameters[attribute1] = attribute2;
        }

        public override PlayerMarkupResult Generate(MediaGenerateMarkupArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            var result = new PlayerMarkupResult();

            switch (args.MarkupType)
            {
                case MarkupType.Frame:
                    result.Html = args.Properties.Collection.Get(BrightcovePlayerParameters.EmbedStyle) == Brightcove.Constants.EmbedJavascript ?
                      result.Html = GenerateJavascriptEmbed(args) :
                      result.Html = GenerateIframeEmbed(args);

                    //args.Result.Html = args.Generator.GetFrame(args);
                    break;
                case MarkupType.FrameUrl:
                    result.Html = GenerateFrameUrl(args);
                    break;
                case MarkupType.Link:
                    if (string.IsNullOrEmpty(args.LinkTitle))
                    {
                        args.LinkTitle = Translate.Text("Media Link");
                    }

                    result.Html = GenerateLinkHtml(args);
                    break;
                case MarkupType.Html:
                    if (!args.Properties.ForceRender && Context.PageMode.IsExperienceEditorEditing)
                    {
                        result.Html = GetPreviewImage(args);
                    }
                    else
                    {
                        result = GenerateHtml(args);
                    }
                    break;
            }

            return result;
        }

        public override string GenerateFrameUrl(MediaGenerateMarkupArgs args)
        {
            var itemId = args.MediaItem.ID.ToGuid().ToString("N");
            var playerId = args.PlayerItem.ID.ToGuid().ToString("N");
            var url = new UrlString($"{Sitecore.MediaFramework.Constants.PlayerIframeUrl}?itemId={itemId}&playerId={playerId}");

            return url.ToString();
        }

        public virtual string GenerateBrightcoveUrl(MediaGenerateMarkupArgs args, bool isJs)
        {
            var resource = isJs ? "index.min.js" : $"index.html?videoId={args.MediaItem[BrightcovePlayerParameters.MediaId]}";
            var url = new UrlString($"https://players.brightcove.net/{args.AccountItem[BrightcovePlayerParameters.AccountId]}/{args.PlayerItem[BrightcovePlayerParameters.PlayerId]}_default/{resource}");

            foreach (string arg in args.Properties.Collection)
            {
                url[arg] = args.Properties.Collection[arg];
            }

            return url.ToString();
        }

        protected virtual string GenerateJavascriptEmbed(MediaGenerateMarkupArgs args)
        {
            string responsive = String.Empty;
            string responsiveStyle = String.Empty;
            string responsiveClosingTags = String.Empty;
            string autoplay = String.Empty;
            string muted = String.Empty;
            string assetId = "";
            string showPlaylists = "";
            string languageEmbed = "";

            // Add autoplay
            if (args.Properties.Collection[BrightcovePlayerParameters.EmbedStyle] != null)
            {
                var calcPadding = ((float)args.Properties.Height / args.Properties.Width) * 100;
                responsive = $"<div style='position: relative; display: block; max-width: {args.Properties.Width}px;'><div style='padding-top: {calcPadding}%;'>";
                responsiveStyle = "style='position: absolute; top: 0px; right: 0px; bottom: 0px; left: 0px; width: 100%; height: 100%;'";
                responsiveClosingTags = "</div></div>";
            }
            // Add autoplay
            if (args.Properties.Collection[BrightcovePlayerParameters.Autoplay] != null)
            {
                autoplay = "autoplay='autoplay'";
            }
            // Add muted
            if (args.Properties.Collection[BrightcovePlayerParameters.Muted] != null)
            {
                muted = "muted='muted'";
            }

            if (args.MediaItem.TemplateID.Equals(TemplateIDs.Video))
            {
                assetId = $"data-video-id='{args.MediaItem[FieldIDs.MediaElement.Id]}'";
            }
            else
            {
                assetId = $"data-playlist-id='{args.MediaItem[FieldIDs.MediaElement.Id]}'";
            }

            if (!args.MediaItem.TemplateID.Equals(TemplateIDs.Video) && args.PlayerItem[FieldIDs.Player.ShowPlaylist] == "1")
            {
                showPlaylists = "<ol class='vjs-playlist'></ol>";
            }

            if(!string.IsNullOrWhiteSpace(args.Language))
            {
                languageEmbed = $"lang='{args.Language}'";
            }

            //data-item-id='{args.Properties.ItemId}'
            return $@"{responsive}
                <video {languageEmbed} {assetId}
                  data-account='{args.AccountItem[BrightcovePlayerParameters.AccountId]}' 
	                data-player='{args.PlayerItem[BrightcovePlayerParameters.PlayerId]}' 
	                data-embed='default' 
	                data-application-id 
	                class='video-js' 
	                controls='true' {autoplay} {muted} 
	                {responsiveStyle}></video>
                    {showPlaylists}
                  <script src='{this.GenerateBrightcoveUrl(args, true)}'></script>
                {responsiveClosingTags}";
        }

        protected virtual string GenerateIframeEmbed(MediaGenerateMarkupArgs args)
        {
            string width = $"width='{args.Properties.Width}'";
            string height = $"height='{args.Properties.Height}'";
            string responsive = String.Empty;
            string responsiveStyle = String.Empty;
            string responsiveClosingTags = String.Empty;
            string queryStr = String.Empty;

            // Add autoplay
            if (args.Properties.Collection[BrightcovePlayerParameters.EmbedStyle] == Brightcove.Constants.SizingResponsive)
            {
                var calcPadding = ((float)args.Properties.Height / args.Properties.Width) * 100;
                responsive = $"<div style='position: relative; display: block; max-width: {args.Properties.Width}px;'><div style='padding-top: {calcPadding}%;'>";
                responsiveStyle = "style='position: absolute; top: 0px; right: 0px; bottom: 0px; left: 0px; width: 100%; height: 100%;'";
                responsiveClosingTags = "</div></div>";
                width = height = String.Empty;
            }

            if (args.Properties.Collection[BrightcovePlayerParameters.Autoplay] != null)
                queryStr += "&autoplay=true";
            if (args.Properties.Collection[BrightcovePlayerParameters.Muted] != null)
                queryStr += "&muted=true";

            return $@"{responsive}
                <iframe scrolling='no' class='player-frame' {width} {height} frameborder='0' allowfullscreen='' webkitallowfullscreen='' mozallowfullscreen=''
                  src='{this.GenerateBrightcoveUrl(args, false)}{queryStr}' {responsiveStyle}></iframe>
              {responsiveClosingTags}";
        }

        /// <summary>
        /// Generate a player markup.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string GetPreviewImage(MediaGenerateMarkupArgs args)
        {
            string imageSrc = args.MediaItem[FieldIDs.MediaElement.ThumbnailUrl];

            if (imageSrc.Length == 0)
            {
                imageSrc = string.Format(Sitecore.MediaFramework.Constants.DefaultPreview, args.Properties.Height, args.Properties.Width);
            }

            return string.Format("<img src='{0}' width='{2}' height='{3}' style=\"display:block;cursor: pointer;\" alt='{1}'/>",
              imageSrc, args.MediaItem.Name, args.Properties.Width, args.Properties.Height);
        }

        /*public override Item GetDefaultPlayer(MediaGenerateMarkupArgs args)
        {
            ID fieldId = args.MediaItem.TemplateID == TemplateIDs.Video ? FieldIDs.AccountSettings.DefaultVideoPlayer : FieldIDs.AccountSettings.DefaultPlaylistPlayer;
            //ReferenceField referenceField = (ReferenceField)AccountManager.GetSettingsField(args.AccountItem, fieldId);
            //if (referenceField == null)
            //return (Item)null;
            return null;// referenceField.TargetItem;
        }*/

        public override string GetMediaId(Item item)
        {
            return item[FieldIDs.MediaElement.Id];
        }

        protected virtual Dictionary<string, string> GetPlayerParameters(MediaGenerateMarkupArgs args, ref string scriptUrl)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>((IDictionary<string, string>)this.DefaultParameters);

            // Add width and height attribute for player
            dictionary["style"] = "width:" +
                                  args.Properties.Width.ToString((IFormatProvider)CultureInfo.InvariantCulture) + "px" +
                                  ";height:" +
                                  args.Properties.Height.ToString((IFormatProvider)CultureInfo.InvariantCulture) + "px";

            // Add class attribute for player
            if (args.PlayerItem != null)
            {
                if (!args.PlayerItem[FieldIDs.Player.Class].IsNullOrEmpty())
                    dictionary["class"] = "video-js " + args.PlayerItem[FieldIDs.Player.Class];
                else
                    dictionary["class"] = "video-js";
            }

            // Set autoplay for player
            if (args.PlayerItem != null && args.PlayerItem[FieldIDs.Player.AutoStart] == "1")
            {
                dictionary["autoplay"] = args.PlayerItem[FieldIDs.Player.AutoStart];
            }

            // Set player id for player
            if (args.PlayerItem != null && !args.PlayerItem["ID"].IsNullOrEmpty())
            {
                dictionary["data-player"] = args.PlayerItem["ID"].ToString();
                scriptUrl = scriptUrl.Replace(PlayerToken, args.PlayerItem["ID"].ToString());
            }

            // Set account id for player
            if (args.AccountItem != null && !args.AccountItem[FieldIDs.Account.AccountId].IsNullOrEmpty())
            {
                dictionary["data-account"] = args.AccountItem[FieldIDs.Account.AccountId];
                scriptUrl = scriptUrl.Replace(AccountToken, args.AccountItem[FieldIDs.Account.AccountId]);
            }

            // Set video/playlist id attribute for player
            if (args.MediaItem != null && !args.MediaItem[FieldIDs.MediaElement.Id].IsNullOrEmpty())
            {
                if (args.MediaItem.TemplateID.Equals(TemplateIDs.Video))
                {
                    dictionary["data-video-id"] = args.MediaItem[FieldIDs.MediaElement.Id];
                }
                else
                {
                    dictionary["data-playlist-id"] = args.MediaItem[FieldIDs.MediaElement.Id];
                }
            }

            if(!string.IsNullOrWhiteSpace(args.Language))
            {
                dictionary["lang"] = args.Language;
            }

            //add cms version
            /*string sitecoreVersion = About.Version;
            string connectorVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            dictionary["data-usage"] = $"cms:sitecore:{sitecoreVersion}:{connectorVersion}:javascript";*/

            return dictionary;
        }

        private void AddPlayerScriptUrl(PlayerMarkupResult playerMarkupResult, MediaGenerateMarkupArgs args)
        {
            if (args.AccountItem != null && args.PlayerItem != null)
            {
                var publisherId = args.AccountItem[FieldIDs.Account.AccountId];
                var playerId = args.PlayerItem["ID"];

                if (!publisherId.IsNullOrEmpty() && !playerId.IsNullOrEmpty())
                {
                    var key = string.Format("{0}{1}Url", publisherId, playerId);

                    if (!playerMarkupResult.BottomScripts.ContainsKey(key))
                    {
                        var scriptUrl = ScriptUrl
                            .Replace(AccountToken, publisherId)
                            .Replace(PlayerToken, playerId);
                        var scriptTag = string.Format(ScriptTagTemplate, scriptUrl);
                        ////playerMarkupResult.BottomScripts.Add(key, scriptTag);
                        //var str = "PlayerEventsListener.prototype.playerScripts.push({key:\"" + key + "\", value:\"" +
                                  //scriptUrl + "\"});";
                        //playerMarkupResult.BottomScripts.Add(key, string.Format("{2}brightcoveListener.playerScripts.push({{key:\"{0}\", value:\"{1}\"}});{2}", key, scriptUrl, System.Environment.NewLine));

                        playerMarkupResult.BottomScripts.Add(key, string.Format("{2}brightcoveListener.playerScripts['{0}']='{1}';{2}", key, scriptUrl, System.Environment.NewLine));
                    }
                }
            }
        }

        /// <summary>
        /// Generate a player markup.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public PlayerMarkupResult GenerateHtml(MediaGenerateMarkupArgs args)
        {
            PlayerMarkupResult playerMarkupResult = new PlayerMarkupResult();
            var scriptUrl = this.ScriptUrl;
            //playerMarkupResult.ScriptUrls.Add(this.AnalyticsScriptUrl);
            StringBuilder stringBuilder = new StringBuilder("<video controls", 1024);

            foreach (KeyValuePair<string, string> keyValuePair in this.GetPlayerParameters(args, ref scriptUrl))
                stringBuilder.Append(" " + keyValuePair.Key + "='" + keyValuePair.Value + "'");
            stringBuilder.Append(" data-embed='default'");
            stringBuilder.Append("></video>");

            if (!args.MediaItem.TemplateID.Equals(TemplateIDs.Video) && args.PlayerItem[FieldIDs.Player.ShowPlaylist] == "1")
            {
                stringBuilder.Append("<ol class='vjs-playlist'></ol>");
            }

            playerMarkupResult.Html = stringBuilder.ToString();
            AddPlayerScriptUrl(playerMarkupResult, args);

            args.Properties.MediaId = args.MediaItem["ID"];
            args.Result = playerMarkupResult;
            AddAnalytics(args);

            return playerMarkupResult;
        }

        public void AddAnalytics(MediaGenerateMarkupArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.MediaItem, "args.PlayerProperties");
            Assert.ArgumentNotNull(args.AccountItem, "args.PlayerProperties");

            if (!this.CheckState(args))
            {
                return;
            }

            Field eventsField = args.AccountItem.Fields[Sitecore.MediaFramework.FieldIDs.AccountSettings.PlaybackEventsRules];

            if (eventsField != null)
            {
                var ruleList = RuleFactory.GetRules<PlaybackRuleContext>(eventsField);

                var context = new PlaybackRuleContext { Item = args.MediaItem };

                ruleList.Run(context);

                args.PlaybackEvents = context.PlaybackEvents;

                if (args.PlaybackEvents.Count > 0)
                {
                    string mediaId = args.Properties.MediaId;

                    if (!string.IsNullOrEmpty(mediaId))
                    {
                        string json = JsonConvert.SerializeObject(args.PlaybackEvents);

                        string script = string.Format("PlayerEventsListener.prototype.playbackEvents['{0}'] = {1};", mediaId, json);

                        args.Result.BottomScripts.Add("mf_pe_" + args.MediaItem.ID, script);
                    }
                }
            }
        }

        protected virtual bool CheckState(MediaGenerateMarkupArgs args)
        {
            return args.MarkupType == MarkupType.Html && Sitecore.Configuration.Settings.GetBoolSetting("Xdb.Enabled", false) && !Context.PageMode.IsExperienceEditorEditing;
        }
    }
}
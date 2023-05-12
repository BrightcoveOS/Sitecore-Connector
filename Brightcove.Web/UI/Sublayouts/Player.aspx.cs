using System;
using System.Web.UI;
using Brightcove.Constants;
using Brightcove.Core.EmbedGenerator.Models;
using Brightcove.Web.EmbedGenerator;
using Brightcove.Web.Utilities;
using Sitecore.Data;
using Sitecore.Diagnostics;

namespace Brightcove.Web.UI.Sublayouts
{
    public partial class Player : Page
    {
        Database database = Sitecore.Data.Database.GetDatabase("master");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                return;
            }

            try
            {
                SitecoreEmbedGenerator generator = new SitecoreEmbedGenerator();
                EmbedModel model = new EmbedModel();

                var itemId = this.Request.QueryString["itemId"];
                var videoId = this.Request.QueryString["videoId"];
                var playerId = this.Request.QueryString["playerId"];
                var playerAssetId = this.Request.QueryString["playerAssetId"];
                var accountId = this.Request.QueryString["accountId"];
                var isPlaylist = this.Request.QueryString["isPlaylist"];
                var height = this.Request.QueryString["height"];
                var width = this.Request.QueryString["width"];

                if (!string.IsNullOrWhiteSpace(itemId))
                {
                    model = GenerateModelByItemId(itemId, playerId);
                }
                else if (!string.IsNullOrWhiteSpace(videoId))
                {
                    model = GenerateModelByVideoId(videoId, playerAssetId, accountId, isPlaylist, height, width);
                }

                EmbedMarkup result = generator.Generate(model);

                this.PlayerContainer.InnerHtml = result.Markup;
                //this.PlayerContainer.Attributes["data-mf-params"] = properties.ToString();
            }
            catch(Exception ex)
            {
                this.PlayerContainer.InnerHtml = "An error has occured loading the video";
                Sitecore.Diagnostics.Log.Error("Failed to load brightcove video", ex, this);
            }
        }

        private EmbedModel GenerateModelByVideoId(string videoId, string playerAssetId, string accountId, string isPlaylistString, string heightString, string widthString)
        {
            Assert.ArgumentNotNullOrEmpty(videoId, "videoId");
            Assert.ArgumentNotNullOrEmpty(accountId, "accountId");

            EmbedModel model = new EmbedModel();
            int height = 0;
            int width = 0;
            bool isPlaylist = false;

            if (string.IsNullOrWhiteSpace(playerAssetId))
            {
                playerAssetId = "default";
            }

            model.MediaId = videoId;
            model.AccountId = accountId;
            model.PlayerId = playerAssetId;
            model.EmbedType = EmbedType.Iframe;
            model.MediaSizing = MediaSizing.Responsive;
            
            if(int.TryParse(heightString, out height))
            {
                model.Height = height;
            }

            if(int.TryParse(widthString, out width))
            {
                model.Width = width;
            }
            
            if(bool.TryParse(isPlaylistString, out isPlaylist) && isPlaylist)
            {
                model.MediaType = MediaType.Playlist;
            }

            return model;
        }

        private EmbedModel GenerateModelByItemId(string itemId, string playerId)
        {
            Assert.ArgumentNotNullOrEmpty(itemId, "itemId");

            var item = database.GetItem(new ID(itemId));

            if(item == null)
            {
                throw new Exception($"The specified item does not exist: '{itemId}'");
            }

            var account = MediaItemUtil.GetAccountForMedia(item);
            bool isPlaylist = item.TemplateID == Templates.Playlist.Id;

            EmbedModel model = new EmbedModel();

            model.MediaId = item["ID"];
            model.AccountId = account["AccountId"];
            model.EmbedType = EmbedType.Iframe;
            model.MediaSizing = MediaSizing.Fixed;
            model.PlayerId = "default";

            if (!string.IsNullOrWhiteSpace(playerId))
            {
                model.PlayerId = database.GetItem(new ID(playerId))?["ID"];

                if(string.IsNullOrWhiteSpace(playerId))
                {
                    model.PlayerId = "default";
                }
            }

            if (isPlaylist)
            {
                model.MediaType = MediaType.Playlist;
            }

            return model;
        }
    }
}
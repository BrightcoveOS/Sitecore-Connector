namespace Sitecore.MediaFramework.UI.Sublayouts
{
  using System;
    using System.Linq;
    using System.Web.UI;
    using Brightcove.Core.EmbedGenerator.Models;
    using Brightcove.MediaFramework.Brightcove;
    using Brightcove.Web.EmbedGenerator;
    using Brightcove.Web.Utilities;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Shell.Framework.Commands;

    public partial class Player : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                return;
            }

            var itemId = this.Request.QueryString["itemId"];

            if(itemId == null)
            {
                return;
            }

            var item = Sitecore.Context.ContentDatabase.GetItem(new ID(itemId));
            var account = MediaItemUtil.GetAccountForMedia(item);
            bool isPlaylist = item.TemplateID == Brightcove.MediaFramework.Brightcove.TemplateIDs.Playlist;

            EmbedModel model = new EmbedModel();

            model.MediaId = item["ID"];
            model.AccountId = account["AccountId"];
            model.EmbedType = EmbedType.Iframe;
            model.MediaSizing = MediaSizing.Fixed;

            var playerId = this.Request.QueryString["playerId"];

            if (playerId != null)
            {
                model.PlayerId = Sitecore.Context.ContentDatabase.GetItem(new ID(playerId))["ID"];
            }
            else
            {
                model.PlayerId = "default";
            }

            if (isPlaylist)
            {
                model.MediaType = MediaType.Playlist;
            }

            SitecoreEmbedGenerator generator = new SitecoreEmbedGenerator();
            EmbedMarkup result = generator.Generate(model);

            this.PlayerContainer.InnerHtml = result.Markup;
            //this.PlayerContainer.Attributes["data-mf-params"] = properties.ToString();
        }
    }
}
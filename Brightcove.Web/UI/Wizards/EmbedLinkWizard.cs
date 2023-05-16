using System;
using Brightcove.Web.Utilities;
using Sitecore;
using Sitecore.Data;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;

namespace Brightcove.Web.UI.Wizards
{
    public class EmbedLinkWizard : BaseEmbedWizard
    {
        protected Edit LinkInput;
        /// <summary>
        /// On Load
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Sitecore.Context.ClientPage.IsEvent && string.IsNullOrEmpty(LinkInput.Value))
            {
                this.LinkInput.Value = WebUtil.GetQueryString("link", string.Empty);
            }
        }

        /// <summary>
        /// Insert Media
        /// </summary>
        protected override void InsertMedia()
        {
            // Check validation.
            if (!this.IsValid())
            {
                return;
            }

            //TODO:var item = this.GetItem();
            //IPlayerMarkupGenerator generator = MediaFrameworkContext.GetPlayerMarkupGenerator(item);

            /*var playerProperties = new PlayerProperties
              {
                ItemId = this.SourceItemID,
                //TODO:Template = item.TemplateID,
                //MediaId = generator.GetMediaId(item),
                PlayerId = new ID(this.PlayersList.Value),
                Width = MainUtil.GetInt(this.WidthInput.Value, MediaFrameworkContext.DefaultPlayerSize.Width),
                Height = MainUtil.GetInt(this.HeightInput.Value, MediaFrameworkContext.DefaultPlayerSize.Height)
              };

            var args = new MediaGenerateMarkupArgs
            {
              MarkupType = MarkupType.Link,
              Properties = playerProperties,
              LinkTitle = this.LinkInput.Value
            };

            MediaGenerateMarkupPipeline.Run(args);*/

            string mediaId = MediaItemUtil.GetMediaId(this.SourceItemID);
            string playerAssetId = MediaItemUtil.GetMediaId(new ID(this.PlayersList.Value));
            string accountId = MediaItemUtil.GetAccountForMedia(this.SourceItemID)["AccountId"];
            string title = this.LinkInput.Value;

            bool isPlaylist = IsPlaylist(SourceItemID);
            int width = int.Parse(this.WidthInput.Value);
            int height = int.Parse(this.HeightInput.Value);


            string url = $"/layouts/Brightcove/Sublayouts/Player.aspx?videoId={mediaId}&playerAssetId={playerAssetId}&accountId={accountId}&width={width}&height={height}&isPlaylist={isPlaylist}";
            string link = $"<a href='{url}' class='brightcove-media-link'>{title}</a>";

            switch (this.Mode)
            {
                case "webedit":
                    SheerResponse.SetDialogValue(link);//args.Result.Html);
                    this.EndWizard();
                    break;

                default:
                    SheerResponse.Eval($"scClose({StringUtil.EscapeJavascriptString(link)})");//SheerResponse.Eval("scClose(" + StringUtil.EscapeJavascriptString(args.Result.Html) + ")");
                    break;
            }
        }

        /// <summary>
        /// Checks if a form filled valid.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected override bool IsValid()
        {
            if (base.IsValid())
            {
                if (string.IsNullOrEmpty(this.LinkInput.Value) || string.IsNullOrWhiteSpace(this.LinkInput.Value))
                {
                    SheerResponse.Alert("Link title is empty");
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
using System;
using System.Linq;
using System.Web.UI;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.WebControls;

namespace Brightcove.Web.UI.Sublayouts
{                     
    public partial class EmbedMediaPlayer : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /*
            //if (Page.IsPostBack)
            //{
            //  return;
            //}

            RegisterDefaultResources(this.Page);

            var sub = this.Parent as Sublayout;
            if (sub != null && Sitecore.Data.ID.IsID(sub.DataSource))
            {
                var properties = new PlayerProperties(StringUtil.GetNameValues(sub.Parameters, '=', '&'))
                {
                    ItemId = new ID(sub.DataSource)
                };

                var args = new MediaGenerateMarkupArgs
                {
                    MarkupType = MarkupType.Html,
                    Properties = properties,
                    MediaItem = Sitecore.Context.Database.GetItem(properties.ItemId),
                    PlayerItem = Sitecore.Context.Database.GetItem(properties.PlayerId)
                };

                args.Properties.Template = args.MediaItem.Template.ID;
                args.AccountItem = Sitecore.Context.Database.GetItem(string.Join("/", args.MediaItem.Paths.Path.Split('/').Take(5)));

                var generator = new BrightcovePlayerMarkupGenerator();
                args.Result = generator.Generate(args);

                //MediaGenerateMarkupPipeline.Run(args);

                //if (!args.Aborted)
                //{*/
            this.PlayerContainer.InnerHtml = "An error has occured";

                //RegisterResources(this.Page, args.Result);

                //this.PlayerContainer.Attributes["data-mf-params"] = properties.ToString();
                /*}
                else
                {
                  this.PlayerContainer.Attributes.Remove("data-mf-params");
                  this.PlayerContainer.InnerHtml = PlayerManager.GetEmptyValue();
                }*/
            /*}
            else
            {
                this.PlayerContainer.InnerHtml = "<div class='mf-default-view'><p>No media is selected</p></div>";
            }*/
    
        }

        /*
        private void RegisterDefaultResources(Page page)
        {
            Assert.ArgumentNotNull(page, "page");

            //ScriptManager.RegisterClientScriptBlock(page, typeof(Page), "MF.css", "<link rel='stylesheet' type='text/css' href='/sitecore modules/Web/MediaFramework/CSS/MF.css'>", false);
            //ScriptManager.RegisterClientScriptInclude(page, typeof(Page), "PlayerEventsListener.js", "/sitecore modules/Web/MediaFramework/js/Analytics/PlayerEventsListener.js");

            page.ClientScript.RegisterClientScriptBlock(typeof(Page), "brightcove.css", "<link rel='stylesheet' type='text/css' href='/sitecore modules/Web/Brightcove/css/brightcove.css'>");
            page.ClientScript.RegisterClientScriptInclude(typeof(Page), "jquery.js", "/sitecore/shell/Controls/Lib/jQuery/jquery.js");
            page.ClientScript.RegisterClientScriptInclude(typeof(Page), "PlayerEventsListener.js", "/sitecore modules/Web/Brightcove/js/Analytics/PlayerEventsListener.js");
            page.ClientScript.RegisterClientScriptInclude(typeof(Page), "brightcove.js", "/sitecore modules/Web/Brightcove/js/Analytics/brightcove.js");
        }

        private void RegisterResources(Page page, PlayerMarkupResult result)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(result, "result");

            foreach (string url in result.CssUrls.Distinct())
            {
                //ScriptManager.RegisterClientScriptBlock(page, typeof(Page), url, "<link rel='stylesheet' type='text/css' href='" + url + "'>", false);
                page.ClientScript.RegisterClientScriptBlock(typeof(Page), url, "<link rel='stylesheet' type='text/css' href='" + url + "'>");
            }

            foreach (string url in result.ScriptUrls.Distinct())
            {
                //ScriptManager.RegisterClientScriptInclude(page, typeof(Page), url, url);
                page.ClientScript.RegisterClientScriptInclude(typeof(Page), url, url);
            }

            foreach (var pair in result.BottomScripts)
            {
                if (!page.ClientScript.IsStartupScriptRegistered(typeof(Page), pair.Key))
                {
                    //ScriptManager.RegisterStartupScript(page, typeof(Page), pair.Key, pair.Value, true);
                    page.ClientScript.RegisterStartupScript(typeof(Page), pair.Key, pair.Value, true);
                }
            }
        }
        */
    }
}
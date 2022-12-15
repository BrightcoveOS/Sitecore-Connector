using System;
using System.Globalization;
using System.Linq;
using Brightcove.Constants;
using Brightcove.Core.EmbedGenerator.Models;
using Brightcove.MediaFramework.Brightcove;
using Brightcove.Web.EmbedGenerator;
using Brightcove.Web.Models;
using Brightcove.Web.Utilities;
using Sitecore;
// using Brightcove.Web.Model;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.MediaFramework;
//using Sitecore.MediaFramework.Utils;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;

namespace Brightcove.Web.Commands
{
    [Serializable]
    public class MediaPreview : Command
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");

            Item item = this.GetItem(context);

            if (item != null)
            {
                bool isPlaylist = item.TemplateID == Templates.Playlist.Id;
                Item accountItem = MediaItemUtil.GetAccountForMedia(item);
                Item defaultPlayerItem;

                if (isPlaylist)
                {
                   defaultPlayerItem = (((ReferenceField)accountItem?.Fields["DefaultPlaylistPlayer"])?.TargetItem);
                }
                else
                {
                   defaultPlayerItem = (((ReferenceField)accountItem?.Fields["DefaultVideoPlayer"])?.TargetItem);
                }

                UrlString url = new UrlString("/layouts/Brightcove/Sublayouts/Player.aspx");

                url["sc_content"] = "master";
                url["itemId"] = item.ID.ToString();
                
                if(defaultPlayerItem != null)
                {
                    url["playerId"] = defaultPlayerItem.ID.ToString();
                }

                SheerResponse.ShowModalDialog(url.ToString(), "960", "540", string.Empty, false);
            }
        }

        public override CommandState QueryState(CommandContext context)
        {
            Item item = this.GetItem(context);
            if (item != null && MediaItemUtil.IsMediaElement(item.Template))
            {
                return CommandState.Enabled;
            }
            return CommandState.Hidden;
        }

        protected virtual Item GetItem(CommandContext context)
        {
            if ((context.Items.Length > 0) && (context.Items[0] != null))
            {
                return context.Items[0];
            }
      
            string id = context.Parameters["id"];
            if (!string.IsNullOrEmpty(id))
            {
                return (Context.ContentDatabase ?? Context.Database).GetItem(id);
            }

            return null;
        }
    }
}
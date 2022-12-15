using System;
using Sitecore.Data;
using Sitecore.Links;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using System.Linq;
using Brightcove.Web.Utilities;

namespace Brightcove.Web.Commands
{
    [Serializable]
    public class OpenUploader : Command
    {
        public override void Execute(CommandContext context)
        {
            var app = Database.GetDatabase("core").GetItem(new ID("{E7C6251E-C323-4562-94CB-85FCF2BCB933}"));

            var url = new UrlString(LinkManager.GetItemUrl(app));
            var selectedItem = context.Items.FirstOrDefault();
            if (selectedItem != null)
            {
                url.Parameters.Add("itemId", selectedItem.ID.Guid.ToString());
                url.Parameters.Add("database", selectedItem.Database.Name);
                url.Parameters.Add("type", "norm");
            }

            try
            {
                Windows.RunApplication(app, app.Appearance.Icon, app.DisplayName, url.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.Error("Opening Uploader failed.", this, ex);
            }
        }
        /*public override CommandState QueryState(CommandContext context)
        {
          return MediaFrameworkContext.IsExportAllowed() ? CommandState.Enabled : CommandState.Hidden;
        }*/
    }
}
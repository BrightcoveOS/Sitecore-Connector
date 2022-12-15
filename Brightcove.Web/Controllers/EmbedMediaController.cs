using System.Linq;
using System.Web.Mvc;
using Brightcove.Constants;
using Brightcove.Core;
using Brightcove.Core.EmbedGenerator;
using Brightcove.Core.EmbedGenerator.Models;
using Brightcove.MediaFramework.Brightcove;
using Brightcove.Web.EmbedGenerator;
using Brightcove.Web.Models;
using Brightcove.Web.Utilities;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;

namespace Brightcove.Web.Controllers
{
    public class EmbedMediaController : SitecoreController
    {
        private string renderVideoViewPath = "~/sitecore modules/Web/Brightcove/Views/EmbedMedia.cshtml";

        public ActionResult RenderMedia()
        {
            Rendering rendering = RenderingContext.Current.Rendering;
            EmbedRenderingParameters parameters = new EmbedRenderingParameters(rendering.Parameters.ToDictionary(p => p.Key, p => p.Value));

            if (ID.IsID(rendering.DataSource))
            {
                GetLegacyRenderingParameters(rendering.DataSource, parameters);
            }

            var generator = new SitecoreEmbedGenerator();
            var result = generator.Generate(parameters);

            return this.View(this.renderVideoViewPath, result);
        }


        //We try not to use Sitecore items directly as datasources anymore but for legacy reasons we still try to support that.
        //Instead everything should come from rendering parameters which are not dependent on any Sitecore items existing.
        public void GetLegacyRenderingParameters(string datasourceId, EmbedRenderingParameters parameters)
        {
            Item datasource = Sitecore.Context.Database.GetItem(new ID(datasourceId));
            parameters.MediaId = datasource["ID"];
            parameters.AccountId = MediaItemUtil.GetAccountForMedia(datasource)["AccountId"];
            parameters.IsPlaylist = datasource.TemplateID == Templates.Playlist.Id;

            if (!string.IsNullOrWhiteSpace(parameters.Parameters["playerId"]))
            {
                Item player = Sitecore.Context.Database.GetItem(new ID(parameters.Parameters["playerId"]));
                parameters.PlayerId = player["ID"];
            }
            else
            {
                parameters.PlayerId = "default";
            }
        }
    }
}
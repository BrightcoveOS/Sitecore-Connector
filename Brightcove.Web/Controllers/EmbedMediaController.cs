using System.Linq;
using System.Web.Mvc;
using Brightcove.Core;
using Brightcove.Core.EmbedGenerator;
using Brightcove.Core.EmbedGenerator.Models;
using Brightcove.Web.EmbedGenerator;
using Brightcove.Web.Models;
using Sitecore.Configuration;
using Sitecore.Data;
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

            /*if (!ID.IsID(rendering.DataSource))
            {
                return this.View(this.renderVideoViewPath, new MediaGenerateMarkupArgs());
            }

            PlayerProperties properties = new PlayerProperties(rendering.Parameters.ToDictionary(p => p.Key, p => p.Value))
            {
                ItemId = new ID(rendering.DataSource)
            };*/

            //MediaGenerateMarkupPipeline.Run(args);

            EmbedRenderingParameters parameters = new EmbedRenderingParameters(rendering.Parameters.ToDictionary(p => p.Key, p => p.Value));

            var generator = new SitecoreEmbedGenerator();
            var result = generator.Generate(parameters);

            return this.View(this.renderVideoViewPath, result);
        }
    }
}
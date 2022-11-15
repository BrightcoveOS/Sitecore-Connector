using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.MediaFramework.Brightcove;
using Brightcove.Web.Models;
using Newtonsoft.Json;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Names;
using Sitecore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sitecore.Diagnostics;

namespace Brightcove.Web.Controllers
{
    public class ExperienceController : SitecoreController
    {
        public ActionResult Render()
        {
            var renderingContext = Sitecore.Mvc.Presentation.RenderingContext.CurrentOrNull;
            ExperienceModel model = new ExperienceModel();

            if (renderingContext != null)
            {
                model.Url = renderingContext.Rendering.Parameters["Url"];               
            }

            return PartialView("/sitecore modules/Web/Brightcove/Views/ExperienceRendering.cshtml", model);
        }
    }
}
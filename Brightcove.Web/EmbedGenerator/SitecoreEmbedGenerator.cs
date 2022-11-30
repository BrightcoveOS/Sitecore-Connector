using Brightcove.Core;
using Brightcove.Core.EmbedGenerator;
using Brightcove.Core.EmbedGenerator.Models;
using Brightcove.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Brightcove.Web.EmbedGenerator
{
    public class SitecoreEmbedGenerator : BrightcoveEmbedGenerator
    {

        public SitecoreEmbedGenerator() : base()
        {
            //1) The RTE does not like the <video-js> tag and will try to incorrectly HTML encode it so we use standard <video> instead.
            //2) Using <video> instead also breaks some styling so we have to change the class to "video-js" (instead of vjs-fluid) and tweak the responsive styling.
            //3) Note that the controls attribute must be set to 'true' or the RTE will strip it away and break the embed
            jsTemplate = "<div style='width: {4}px;'><video data-account='{0}' data-player='{1}' data-embed='default' controls='true' data-video-id='{2}' data-playlist-id='{3}' data-application-id='' width='{4}' height='{5}' class='video-js'></video>{6}</div>";
            jsResponsiveTemplate = "<div style='max-width: {4}px;'><style>div.video-js:not(.vjs-audio-only-mode) {{padding-top: {5}%; width: 100%}}</style><video data-account='{0}' data-player='{1}' data-embed='default' controls='true' data-video-id='{2}' data-playlist-id='{3}' data-application-id='' class='video-js'></video>{6}</div>";

            //Update this to point to loadPlayer.js
            jsScriptTemplate = "<script src='https://players.brightcove.net/{0}/{1}_default/index.min.js'></script>";
        }

        public EmbedMarkup Generate(EmbedRenderingParameters parameters)
        {
            return Generate(parameters.CreateEmbedModel());
        }
    }
}
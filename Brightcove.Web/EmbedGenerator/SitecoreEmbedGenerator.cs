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
            //1) We wrap the iframe in a paragraph tag so that the RTE handles it better
            //2) We don't support using a resposive iframe in the RTE
            iframeTemplate = "<p><iframe class='brightcove-media-container brightcove-media-container-iframe' src='{0}' allowfullscreen='' allow='encrypted-media' width='{1}' height='{2}'></iframe></p>";

            //1) The RTE does not like the <video-js> tag and will try to incorrectly HTML encode it so we use standard <video> instead.
            //2) Using <video> instead also breaks some styling so we have to change the class to "video-js" (instead of vjs-fluid) and tweak the responsive styling.
            //3) Note that the controls attribute must be set to 'true' or the RTE will strip it away and break the embed
            //4) We also tweak the responsive styling to use full-width even if the video has not been fully loaded yet (for partial loads in the RTE or XE)
            jsTemplate = "<div class='brightcove-media-container brightcove-media-container-js' style='width: {4}px;'><video data-account='{0}' data-player='{1}' data-embed='default' controls='true' data-video-id='{2}' data-playlist-id='{3}' data-application-id='' width='{4}' height='{5}' class='video-js' {7} {8} {9}></video>{6}</div>";
            jsResponsiveTemplate = "<div class='brightcove-media-container brightcove-media-container-js' style='max-width: {4}px;'><style>div.video-js:not(.vjs-audio-only-mode) {{padding-top: {5}%; width: 100%}} video.video-js {{width: 100%}}</style><video data-account='{0}' data-player='{1}' data-embed='default' controls='true' data-video-id='{2}' data-playlist-id='{3}' data-application-id='' class='video-js' {7} {8} {9}></video>{6}</div>";

            /*
             * We cant let videos embedded in rich text fields load while in the content/experience editor because it modifies the value of the rich text field breaking the embed.
             * Note this only happens if you open the rich text editor while in the experience editor.
             * So Instead we load this script first and check if we are in the content/experience editor before loading anything else.
             */
            jsScriptTemplate = "<script src='/sitecore%20modules/Web/Brightcove/js/loadPlayer.js?account={0}&player={1}'></script>";
        }

        public EmbedMarkup Generate(EmbedRenderingParameters parameters)
        {
            return Generate(parameters.CreateEmbedModel());
        }
    }
}
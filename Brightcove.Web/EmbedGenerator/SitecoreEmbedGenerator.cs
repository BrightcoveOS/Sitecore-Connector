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
            iframeTemplate = "<p><iframe class='brightcove-media-container brightcove-media-container-iframe' src='{0}' allowfullscreen='' allow='encrypted-media' width='{1}' height='{2}' title='{3}'></iframe></p>";

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

        /*
         * We overrided the GenerateIframe method to add support for an optional title attribute to help with accessibility.
         * This is not really part of a standard Brightcove embed and technically Brightcove videos don't have such an "alt-text" field to map to.
         */
        protected override EmbedMarkup GenerateIframe(EmbedModel model)
        {
            EmbedMarkup result = new EmbedMarkup();
            string mediaParameter = "videoId";

            switch (model.MediaType)
            {
                case MediaType.Video:
                    mediaParameter = "videoId";
                    break;
                case MediaType.Playlist:
                    mediaParameter = "playlistId";
                    break;
                default:
                    throw new Exception("Invalid media type for iframe embed");
            }

            string iframeUrl = string.Format(iframeBaseUrl, model.AccountId, model.PlayerId, model.MediaId, mediaParameter);

            if (model.Autoplay)
            {
                //Note that the autoplay parameter is NOT a boolean
                iframeUrl += "&autoplay=muted";
            }

            if (model.Muted)
            {
                //Looks like the video is muted even if set to false...
                iframeUrl += "&muted=true";
            }

            if (!string.IsNullOrWhiteSpace(model.Language))
            {
                iframeUrl += $"&language={model.Language}";
            }

            switch (model.MediaSizing)
            {
                case MediaSizing.Responsive:
                    string aspectRatio = ((double)model.Height / model.Width * 100.0).ToString("F2");
                    result.Markup = string.Format(iframeResponsiveTemplate, iframeUrl, model.Width, aspectRatio, model.Title);
                    break;
                case MediaSizing.Fixed:
                    result.Markup = string.Format(iframeTemplate, iframeUrl, model.Width, model.Height, model.Title);
                    break;
                default:
                    throw new Exception("Invalid media sizing for iframe embed");
            }

            result.Model = model;

            return result;
        }

        public EmbedMarkup Generate(EmbedRenderingParameters parameters)
        {
            return Generate(parameters.CreateEmbedModel());
        }
    }
}
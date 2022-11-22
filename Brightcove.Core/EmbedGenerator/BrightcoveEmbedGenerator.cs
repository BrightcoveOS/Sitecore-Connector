using Brightcove.Core.EmbedGenerator.Models;
using System;

namespace Brightcove.Core
{
    public class BrightcoveEmbedGenerator
    {
        readonly static string iframeTemplate = "<iframe src='https://players.brightcove.net/{0}/{1}_default/index.html?{3}={2}' allowfullscreen='' allow='encrypted-media' width='{4}' height='{5}'></iframe>";
        readonly static string iframeResponsiveTemplate = "<div style='position: relative; display: block; max-width: {4}px;'><div style='padding-top: {5}%;'><iframe src='https://players.brightcove.net/{0}/{1}_default/index.html?{3}={2}' allowfullscreen='' allow='encrypted-media' style='position: absolute; top: 0px; right: 0px; bottom: 0px; left: 0px; width: 100%; height: 100%;'></iframe></div></div>";

        readonly static string jsTemplate = "<div style='width: {4}px;'><video-js data-account='{0}' data-player='{1}' data-embed='default' controls='' data-video-id='{2}' data-playlist-id='{3}' data-application-id='' width='{4}' height='{5}' class='vjs-fluid'></video-js>{6}</div>";
        readonly static string jsResponsiveTemplate = "<div style='max-width: {4}px;'><style>video-js.video-js.vjs-fluid:not(.vjs-audio-only-mode) {{padding-top: {5}%;}}</style><video-js data-account='{0}' data-player='{1}' data-embed='default' controls='' data-video-id='{2}' data-playlist-id='{3}' data-application-id='' class='vjs-fluid'></video-js>{6}</div>";
        readonly static string jsScriptTemplate = "<script src='https://players.brightcove.net/{0}/{1}_default/index.min.js'></script>";

        public EmbedMarkup Generate(EmbedModel model)
        {
            EmbedMarkup result = new EmbedMarkup();

            switch (model.EmbedType)
            {
                case EmbedType.Iframe:
                    result = GenerateIframe(model);
                    break;
                case EmbedType.JavaScript:
                    result = GenerateJavaScript(model);
                    break;
                default:
                    throw new Exception("Invalid embed type");
            }

            return result;
        }

        protected EmbedMarkup GenerateIframe(EmbedModel model)
        {
            EmbedMarkup result = new EmbedMarkup();
            string mediaParameter = "videoId";

            switch(model.MediaType)
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

            switch(model.MediaSizing)
            {
                case MediaSizing.Responsive:
                    string aspectRatio = ((double)model.Height / model.Width * 100.0).ToString("F2");
                    result.Markup = string.Format(iframeResponsiveTemplate, model.AccountId, model.PlayerId, model.MediaId, mediaParameter, model.Width, aspectRatio);
                    break;
                case MediaSizing.Fixed:
                    result.Markup = string.Format(iframeTemplate, model.AccountId, model.PlayerId, model.MediaId, mediaParameter, model.Width, model.Height);
                    break;
                default:
                    throw new Exception("Invalid media sizing for iframe embed");
            }

            result.Model = model;

            return result;
        }

        protected EmbedMarkup GenerateJavaScript(EmbedModel model)
        {
            EmbedMarkup result = new EmbedMarkup();
            string videoId = "";
            string playlistId = "";
            string playlistMarkup = "";

            switch (model.MediaType)
            {
                case MediaType.Video:
                    videoId = model.MediaId;
                    break;
                case MediaType.Playlist:
                    playlistId = model.MediaId;
                    playlistMarkup = "<div class='vjs-playlist'></div>";
                    break;
                default:
                    throw new Exception("Invalid media type for javascript embed");
            }

            switch (model.MediaSizing)
            {
                case MediaSizing.Responsive:
                    string aspectRatio = ((double)model.Height / model.Width * 100.0).ToString("F2");
                    result.Markup = string.Format(jsResponsiveTemplate, model.AccountId, model.PlayerId, videoId, playlistId, model.Width, aspectRatio, playlistMarkup);
                    break;
                case MediaSizing.Fixed:
                    result.Markup = string.Format(jsTemplate, model.AccountId, model.PlayerId, videoId, playlistId, model.Width, model.Height, playlistMarkup);
                    break;
                default:
                    throw new Exception("Invalid media sizing for javascript embed");
            }

            result.ScriptTag = string.Format(jsScriptTemplate, model.AccountId, model.PlayerId);
            result.Model = model;

            return result;
        }
    }
}

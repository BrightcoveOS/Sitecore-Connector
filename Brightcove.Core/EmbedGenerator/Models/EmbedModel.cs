using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.Core.EmbedGenerator.Models
{
    public class EmbedModel
    {
        public string AccountId { get; set; } = "";

        public string PlayerId { get; set; } = "default";

        public string MediaId { get; set; } = "";

        public int Width { get; set; } = 960;

        public int Height { get; set; } = 540;

        public MediaType MediaType { get; set; } = MediaType.Video;

        public MediaSizing MediaSizing { get; set; } = MediaSizing.Responsive;

        public EmbedType EmbedType { get; set; } = EmbedType.Iframe;

        public bool Autoplay { get; set; } = false;

        public bool Muted { get; set; } = false;

        public string Language { get; set; } = "";

        public string Title { get; set; } = "";

        public EmbedModel()
        {

        }

        public EmbedModel(string accountId, string playerId, string mediaId, MediaType mediaType)
        {
            AccountId = accountId;
            PlayerId = playerId;
            MediaId = mediaId;
            MediaType = mediaType;
        }
    }

    public enum MediaType
    {
        Video,
        Playlist,
        Experience
    }

    public enum MediaSizing
    {
        Responsive,
        Fixed
    }

    public enum EmbedType
    {
        Iframe,
        JavaScript
    }
}

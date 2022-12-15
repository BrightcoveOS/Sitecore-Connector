using Sitecore.Data;

namespace Brightcove.Constants
{
    public static class Templates
    {
        public static class MediaElement
        {
            public static readonly ID Id = new ID("{C4EA24F3-C8BB-44CA-A224-DEF748ADF582}");
            public static readonly ID EventsId = new ID("{C923961C-5664-43D2-85D2-D194143197DF}");
        }

        public static class Video
        {
            public static readonly ID Id = new ID("{6A5C6835-6E11-4602-A11D-B626E9255397}");
        }

        public static class Playlist
        {
            public static readonly ID Id = new ID("{0E24292F-D7A5-4BA2-BCA0-CD5F14A89634}");
        }

        public static class PlaybackEvent
        {
            public static readonly ID PageEventId = new ID("{7CCB928C-7EB0-465A-A755-D4BD7256BD58}");
            public static readonly ID ParameterId = new ID("{E1E68B6B-5C30-41D3-B6C9-7089B5CE1B34}");
        }
    }
}
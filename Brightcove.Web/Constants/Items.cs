using Sitecore.Data;

namespace Brightcove.Constants
{
    public static class Items
    {
        public static readonly ID MediaFrameworkRoot = new ID("{1ADBC873-DFBF-4C09-853E-AC71E6D59739}");

        public static class PageEvents
        {
            public static readonly ID PlaybackStarted = new ID("{76B6B0EA-F4BC-41AC-8BBA-CD4BBBF8E438}");

            public static readonly ID PlaybackCompleted = new ID("{035B4467-A848-4D57-A465-401B035FDE98}");

            public static readonly ID PlaybackChanged = new ID("{634C87A9-4E01-4C69-9522-215865012DC5}");

            public static readonly ID PlaybackError = new ID("{5F51C65D-796C-43BE-B9C4-5855159AF18B}");
        }
    }
}
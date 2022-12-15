using System.Linq;
using Brightcove.Constants;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Brightcove.Web.Utilities
{
    public class MediaItemUtil
    {
        public static string GetMediaId(ID mediaId)
        {
            return Sitecore.Context.ContentDatabase.GetItem(mediaId)["ID"];
        }

        public static Item GetAccountForMedia(Item media)
        {
            return media.Database.GetItem(string.Join("/", media.Paths.Path.Split('/').Take(5)));
        }

        public static Item GetAccountForMedia(ID mediaId)
        {
            return GetAccountForMedia(Sitecore.Context.ContentDatabase.GetItem(mediaId));
        }

        public static bool IsMediaElement(TemplateItem template)
        {
            if (template.ID == Templates.MediaElement.Id)
            {
                return true;
            }

            return template.BaseTemplates.Any(IsMediaElement);
        }
    }
}
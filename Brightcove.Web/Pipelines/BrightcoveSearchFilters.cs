using Brightcove.Constants;
using Sitecore.Buckets.Pipelines.UI.DialogSearchFilters;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Diagnostics;


namespace Brightcove.Web.Pipelines.DialogSearchFilters
{
    public class BrightcoveSearchFilters : DialogSearchFiltersProcessor
    {
        public override void Process(DialogSearchFiltersArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.SearchFilters, "args.SearchFilters");

            string filter = this.GetFilter();

            if (!args.SearchFilters.ContainsKey("MediaFramework.EmbedMedia"))
            {
                args.SearchFilters.Add("MediaFramework.EmbedMedia", filter);
            }

            if (!args.SearchFilters.ContainsKey("MediaFramework.EmbedLink"))
            {
                args.SearchFilters.Add("MediaFramework.EmbedLink", filter);
            }
        }

        protected virtual string GetFilter()
        {
            return this.GetLocationFilter();// + "&" + this.GetTemplatesFilter();
        }

        protected virtual string GetLocationFilter()
        {
            return string.Format("+location:Brightcove|{0}", IdHelper.NormalizeGuid(Items.MediaFrameworkRoot));
        }

        //Apparently cant filter by multiple templates or a base template
        /*protected virtual string GetTemplatesFilter()
        {
          return string.Format("+template:Videos|{0}&+template:Playlists|{1}",IdHelper.NormalizeGuid(Brightcove.MediaFramework.Brightcove.TemplateIDs.Video), IdHelper.NormalizeGuid(Brightcove.MediaFramework.Brightcove.TemplateIDs.Playlist));
        }*/
    }
}
using System;
using System.Collections.Generic;
using Sitecore.Data.Items;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.ContentSearch;
using Sitecore.Data.Fields;
using System.Linq;
using Sitecore.Diagnostics;

namespace Brightcove.Web.Search.ComputedIndexFields
{
    public class FolderComputedIndexField : IComputedIndexField
    {
        public string FieldName { get; set; }
        public string ReturnType { get; set; }

        public object ComputeFieldValue(IIndexable indexable)
        {
            try
            {
                Item video = (Item)(indexable as SitecoreIndexableItem);
                ReferenceField folder = video.Fields["BrightcoveFolder"];
                return folder.TargetItem["Name"];
            }
            catch(Exception ex)
            {
                Log.Error("Failed to compute folder index field", ex, this);
            }

            return "";
        }
    }
}

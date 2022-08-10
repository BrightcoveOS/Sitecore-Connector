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
    public class LabelsComputedIndexField : IComputedIndexField
    {
        public string FieldName { get; set; }
        public string ReturnType { get; set; }

        public object ComputeFieldValue(IIndexable indexable)
        {
            try
            {
                Item video = (Item)(indexable as SitecoreIndexableItem);
                MultilistField labels = video.Fields["labels"];

                if (labels != null)
                {
                    return string.Join(" ", labels.GetItems()?.Select(i => i["label"]) ?? new string[0]);
                }
            }
            catch(Exception ex)
            {
                Log.Error("Failed to compute labels index field", ex, this);
            }

            return new List<string>();
        }
    }
}

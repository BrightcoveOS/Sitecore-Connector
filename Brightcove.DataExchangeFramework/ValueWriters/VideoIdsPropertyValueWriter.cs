using Sitecore.Data.Fields;
using Sitecore.DataExchange.DataAccess;
using System;
using System.Reflection;
using System.Linq;

namespace Brightcove.DataExchangeFramework.ValueWriters
{
    public class VideoIdsPropertyValueWriter : ChainedPropertyValueWriter
    {
        public VideoIdsPropertyValueWriter(string propertyName) : base(propertyName)
        {
        }

        public override bool Write(object target, object value, DataAccessContext context)
        {
            try
            {
                if (value != null)
                {
                    string[] itemIds = ((string)value).Split('|');
                    var videoIds = itemIds.Select(id => Sitecore.Context.ContentDatabase.GetItem(id))
                                        .Where(v => v != null)
                                        .Select(v => v["ID"])
                                        .Where(id => !string.IsNullOrWhiteSpace(id))
                                        .ToList();

                    value = videoIds;
                }
            }
            catch
            {
                return false;
            }

            return base.Write(target, value, context);
        }
    }
}
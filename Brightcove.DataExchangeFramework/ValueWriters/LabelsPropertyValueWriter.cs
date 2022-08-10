using Sitecore.Data.Fields;
using Sitecore.DataExchange.DataAccess;
using System;
using System.Reflection;
using System.Linq;

namespace Brightcove.DataExchangeFramework.ValueWriters
{
    public class LabelsPropertyValueWriter : ChainedPropertyValueWriter
    {
        public LabelsPropertyValueWriter(string propertyName) : base(propertyName)
        {
        }

        public override bool Write(object target, object value, DataAccessContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (value != null)
            {
                string[] itemIds = ((string)value).Split('|');
                var paths = itemIds.Select(id => Sitecore.Context.ContentDatabase.GetItem(id))
                                    .Where(label => label != null)
                                    .Select(labelItem => labelItem["label"])
                                    .Where(path => !string.IsNullOrWhiteSpace(path))
                                    .ToList();

                value = paths;
            }

            return base.Write(target, value, context);
        }
    }
}
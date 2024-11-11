using Sitecore.Data.Fields;
using Sitecore.DataExchange.DataAccess;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Brightcove.DataExchangeFramework.ValueWriters
{
    public class LabelsPropertyValueWriter : ChainedPropertyValueWriter
    {
        public LabelsPropertyValueWriter(string propertyName) : base(propertyName)
        {
        }

        public override bool Write(object target, object labelIds, DataAccessContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            List<string> labelItemIds = null;

            try
            {
                if (labelIds != null)
                {
                    string[] itemIds = ((string)labelIds).Split('|');
                    labelItemIds = itemIds.Select(id => Sitecore.Context.ContentDatabase.GetItem(id))
                                        .Where(label => label != null)
                                        .Select(labelItem => labelItem["label"])
                                        .Where(path => !string.IsNullOrWhiteSpace(path))
                                        .ToList();
                }
            }
            catch
            {
                return false;
            }

            return base.Write(target, labelItemIds, context);
        }
    }
}
using Sitecore.DataExchange.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brightcove.DataExchangeFramework.ValueWriters
{
    public class CsvStringPropertyValueWriter : ChainedPropertyValueWriter
    {
        public CsvStringPropertyValueWriter(string propertyName) 
            : base(propertyName)
        {
            PropertyName = !string.IsNullOrWhiteSpace(propertyName) ? propertyName : throw new ArgumentOutOfRangeException(nameof(propertyName), (object)propertyName, "Property name must be specified.");
            ReflectionUtil = Sitecore.DataExchange.DataAccess.Reflection.ReflectionUtil.Instance;
        }

        public new string PropertyName { get; private set; }

        public override bool Write(object target, object value, DataAccessContext context)
        {
            ICollection<string> values;

            try
            {
                values = ((string)value).Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (values.Count == 0)
                    return true;
            }
            catch
            {
                return false;
            }

            return base.Write(target, values, context);
        }
    }
}
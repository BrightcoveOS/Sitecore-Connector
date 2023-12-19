using Sitecore.DataExchange.DataAccess;
using System;
using System.Globalization;

namespace Brightcove.DataExchangeFramework.ValueWriters
{
    public class DateTimePropertyValueWriter : ChainedPropertyValueWriter
    {
        public DateTimePropertyValueWriter(string propertyName) : base(propertyName)
        {

        }

        public override bool Write(object target, object value, DataAccessContext context)
        {
            DateTime? returnValue = null;

            try
            {
                if ((value is string) && !string.IsNullOrWhiteSpace((string)value))
                {
                    string sourceValue = (string)value;
                    DateTime tmp;

                    if (DateTime.TryParseExact(sourceValue, new string[2] { "yyyyMMddTHHmmss", "yyyyMMddTHHmmss\\Z" }, (IFormatProvider)CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out tmp))
                    {
                        returnValue = tmp;
                    }
                }
            }
            catch
            {
                returnValue = null;
            }

            return base.Write(target, returnValue, context);
        }
    }
}
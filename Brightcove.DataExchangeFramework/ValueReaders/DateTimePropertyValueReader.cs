using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.DataAccess.Readers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Web;

namespace Brightcove.DataExchangeFramework.ValueReaders
{
    public class DateTimePropertyValueReader : ChainedPropertyValueReader
    {
        public DateTimePropertyValueReader(string propertyName) : base(propertyName)
        {
        }

        public override ReadResult Read(object source, DataAccessContext context)
        {
            try
            {
                var result = base.Read(source, context);

                bool wasValueRead = result.WasValueRead;
                object obj = result.ReadValue;

                if (obj == null || !(obj is DateTime))
                {
                    obj = "";
                    wasValueRead = true;
                }
                else
                {
                    obj = ((DateTime)obj).ToString("yyyyMMddTHHmmss\\Z");
                }

                return new ReadResult(DateTime.UtcNow)
                {
                    WasValueRead = wasValueRead,
                    ReadValue = obj
                };
            }
            catch
            {
            }

            return new ReadResult(DateTime.UtcNow)
            {
                WasValueRead = false,
                ReadValue = null
            };
        }
    }
}

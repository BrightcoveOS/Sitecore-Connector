using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.DataAccess.Readers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Brightcove.DataExchangeFramework.ValueReaders
{
    public class CsvStringPropertyValueReader : ChainedPropertyValueReader
    {
        public CsvStringPropertyValueReader(string propertyName) : base(propertyName)
        {
        }

        public override ReadResult Read(object source, DataAccessContext context)
        {
            try
            {
                var result = base.Read(source, context);

                bool wasValueRead = result.WasValueRead;
                object obj = result.ReadValue;

                if (wasValueRead && obj != null)
                {
                    try
                    {
                        var csvString = obj as ICollection<string>;
                        obj = string.Join(",", csvString);
                    }
                    catch
                    {
                        wasValueRead = false;
                        obj = null;
                    }
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

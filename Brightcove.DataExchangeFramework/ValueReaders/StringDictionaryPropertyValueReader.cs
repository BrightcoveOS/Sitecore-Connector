using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.DataAccess.Readers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Web;

namespace Brightcove.DataExchangeFramework.ValueReaders
{
    public class StringDictionaryPropertyValueReader : ChainedPropertyValueReader
    {
        public StringDictionaryPropertyValueReader(string propertyName) : base(propertyName)
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
                        var stringDictionary = obj as IDictionary<string, string>;
                        obj = string.Join("&", stringDictionary.Select(p => $"{p.Key}={HttpUtility.UrlPathEncode(p.Value)}").ToArray());
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

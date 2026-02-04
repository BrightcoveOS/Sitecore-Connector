using Sitecore.DataExchange.DataAccess;
using System;
using System.Collections.Generic;
using System.Web;

namespace Brightcove.DataExchangeFramework.ValueWriters
{
    public class StringDictionaryPropertyValueWriter : ChainedPropertyValueWriter
    {
        public StringDictionaryPropertyValueWriter(string propertyName) 
            : base(propertyName)
        {
            PropertyName = !string.IsNullOrWhiteSpace(propertyName) ? propertyName : throw new ArgumentOutOfRangeException(nameof(propertyName), (object)propertyName, "Property name must be specified.");
            ReflectionUtil = Sitecore.DataExchange.DataAccess.Reflection.ReflectionUtil.Instance;
        }

        public new string PropertyName { get; private set; }

        public override bool Write(object target, object value, DataAccessContext context)
        {
            Dictionary<string, string> stringDictionary = new Dictionary<string, string>();

            try
            {
                string[] keyValuePairs = ((string)value).Split('&');

                foreach (string keyValuePair in keyValuePairs)
                {
                    string[] splitKeyValuePair = keyValuePair.Split('=');

                    if (splitKeyValuePair.Length == 2)
                    {
                        stringDictionary.Add(splitKeyValuePair[0], HttpUtility.UrlDecode(splitKeyValuePair[1]));
                    }
                }
            }
            catch
            {
                return false;
            }

            return base.Write(target, stringDictionary, context);
        }
    }
}
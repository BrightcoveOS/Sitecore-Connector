using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.DataAccess.Readers;
using System;

namespace Brightcove.DataExchangeFramework.ValueReaders
{
    public class ChainedPropertyValueReader : IValueReader
    {
        public ChainedPropertyValueReader(string propertyName)
        {
            PropertyName = !string.IsNullOrWhiteSpace(propertyName) ? propertyName : throw new ArgumentOutOfRangeException(nameof(propertyName), propertyName, "Property name must be specified.");
            ReflectionUtil = Sitecore.DataExchange.DataAccess.Reflection.ReflectionUtil.Instance;
        }

        public string PropertyName { get; private set; }

        public IReflectionUtil ReflectionUtil { get; set; }

        public virtual ReadResult Read(object source, DataAccessContext context)
        {
            try
            {
                bool wasValueRead = false;
                object obj = source;

                foreach (string property in PropertyName.Split('.'))
                {
                    var reader = new PropertyValueReader(property);
                    var result = reader.Read(obj, context);

                    wasValueRead = result.WasValueRead;
                    obj = result.ReadValue;

                    if (!wasValueRead || obj == null)
                        break;
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

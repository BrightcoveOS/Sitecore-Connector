using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.DataAccess.Readers;
using System;

namespace Brightcove.DataExchangeFramework.ValueReaders
{
    public class NullableEnumValueReader : EnumValueReader
    {
        public NullableEnumValueReader(Type enumType) 
            : base(enumType)
        {
        }

        private object ToEnumMember(object source)
        {
            if (source != null)
            {
                if (source is string)
                {
                    if (Enum.IsDefined(EnumType, source))
                        return Enum.Parse(EnumType, source.ToString());

                    if (int.TryParse(source.ToString(), out int result))
                        return ToEnumMemberAsInt(result);
                }

                if (source is int i2)
                    return ToEnumMemberAsInt(i2);

                if (EnumType.IsAssignableFrom(source.GetType()))
                    return source;
            }
            return null;
        }

        private object ToEnumMemberAsInt(int i)
        {
            object obj = Enum.ToObject(EnumType, i);
            return Enum.IsDefined(EnumType, obj) ? obj : null;
        }

        public override ReadResult Read(object source, DataAccessContext context)
        {
            try
            {
                object enumMember = ToEnumMember(source);

                return new ReadResult(DateTime.UtcNow)
                {
                    WasValueRead = true,
                    ReadValue = enumMember
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

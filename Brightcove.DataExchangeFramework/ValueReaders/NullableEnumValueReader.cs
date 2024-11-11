using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.DataAccess.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.DataExchangeFramework.ValueReaders
{
    public class NullableEnumValueReader : EnumValueReader
    {
        public NullableEnumValueReader(Type enumType) : base(enumType)
        {

        }

        private object ToEnumMember(object source)
        {
            if (source != null)
            {
                if (source is string)
                {
                    if (Enum.IsDefined(this.EnumType, source))
                        return Enum.Parse(this.EnumType, source.ToString());
                    int result = 0;
                    if (int.TryParse(source.ToString(), out result))
                        return this.ToEnumMemberAsInt(result);
                }
                if (source is int i2)
                    return this.ToEnumMemberAsInt(i2);
                if (this.EnumType.IsAssignableFrom(source.GetType()))
                    return source;
            }
            return (object)null;
        }

        private object ToEnumMemberAsInt(int i)
        {
            object obj = Enum.ToObject(this.EnumType, i);
            return Enum.IsDefined(this.EnumType, obj) ? obj : (object)null;
        }

        public override ReadResult Read(object source, DataAccessContext context)
        {
            try
            {
                object enumMember = this.ToEnumMember(source);

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

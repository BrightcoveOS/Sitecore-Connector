using Brightcove.DataExchangeFramework.ValueReaders;
using Sitecore.DataExchange;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;
using Brightcove.DataExchangeFramework.Helpers;

namespace Brightcove.DataExchangeFramework.Converters
{
    [SupportedIds(new string[] { "{28602B8B-760C-472A-895E-732F727A42A5}" })]
    public class NullableEnumValueReaderConverter : BaseItemModelConverter<IValueReader>
    {
        public NullableEnumValueReaderConverter(IItemModelRepository repository)
          : base(repository)
        {
        }

        protected override ConvertResult<IValueReader> ConvertSupportedItem(
          ItemModel source)
        {
            Type typeFromTypeName = GetTypeFromTypeName(source, FieldName.EnumType);
            return typeFromTypeName == null ? NegativeResult(source, "No type was resolved for the item.") : PositiveResult(new NullableEnumValueReader(typeFromTypeName));
        }
    }
}
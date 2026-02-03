using Brightcove.DataExchangeFramework.ValueReaders;
using Brightcove.DataExchangeFramework.ValueWriters;
using Sitecore.DataExchange;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters.DataAccess.ValueAccessors;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using Brightcove.DataExchangeFramework.Helpers;

namespace Brightcove.DataExchangeFramework
{
    [SupportedIds(new string[] { "{630C0748-9A38-4233-AB61-38983FD009D6}" })]
    public class ChainedPropertyValueAccessorConverter : ValueAccessorConverter
    {
        public ChainedPropertyValueAccessorConverter(IItemModelRepository repository): 
            base(repository)
        {
        }

        protected override ConvertResult<IValueAccessor> ConvertSupportedItem(
          ItemModel source)
        {
            ConvertResult<IValueAccessor> convertResult = base.ConvertSupportedItem(source);
            if (!convertResult.WasConverted)
                return convertResult;
            string stringValue = GetStringValue(source, FieldName.PropertyName);
            if (string.IsNullOrWhiteSpace(stringValue))
                return NegativeResult(source, "The property name field must have a value specified.", $"field: {FieldName.PropertyName}");
            IValueAccessor convertedValue = convertResult.ConvertedValue;
            if (convertedValue == null)
                return NegativeResult(source, "A null value accessor was returned by the converter.");
            if (convertedValue.ValueReader == null)
            {
                ChainedPropertyValueReader propertyValueReader = new ChainedPropertyValueReader(stringValue);
                convertedValue.ValueReader = propertyValueReader;
            }
            if (convertedValue.ValueWriter == null)
            {
                ChainedPropertyValueWriter propertyValueWriter = new ChainedPropertyValueWriter(stringValue);
                convertedValue.ValueWriter = propertyValueWriter;
            }
            return PositiveResult(convertedValue);
        }
    }
}
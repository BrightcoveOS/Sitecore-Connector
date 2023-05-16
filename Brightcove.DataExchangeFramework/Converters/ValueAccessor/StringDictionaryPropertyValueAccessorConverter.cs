using Brightcove.DataExchangeFramework.ValueReaders;
using Brightcove.DataExchangeFramework.ValueWriters;
using Sitecore.DataExchange;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters.DataAccess.ValueAccessors;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.DataAccess.Readers;
using Sitecore.DataExchange.DataAccess.Writers;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework
{
    [SupportedIds(new string[] { "{A8F0F3F3-2547-4852-AA0C-CBE9E73BA4B8}" })]
    public class StringDictionaryPropertyValueAccessorConverter : ValueAccessorConverter
    {
        public const string FieldNamePropertyName = "PropertyName";

        public StringDictionaryPropertyValueAccessorConverter(IItemModelRepository repository)
          : base(repository)
        {
        }

        protected override ConvertResult<IValueAccessor> ConvertSupportedItem(
          ItemModel source)
        {
            ConvertResult<IValueAccessor> convertResult = base.ConvertSupportedItem(source);
            if (!convertResult.WasConverted)
                return convertResult;
            string stringValue = this.GetStringValue(source, "PropertyName");
            if (string.IsNullOrWhiteSpace(stringValue))
                return this.NegativeResult(source, "The property name field must have a value specified.", "field: PropertyName");
            IValueAccessor convertedValue = convertResult.ConvertedValue;
            if (convertedValue == null)
                return this.NegativeResult(source, "A null value accessor was returned by the converter.");
            if (convertedValue.ValueReader == null)
            {
                StringDictionaryPropertyValueReader propertyValueReader = new StringDictionaryPropertyValueReader(stringValue);
                convertedValue.ValueReader = (IValueReader)propertyValueReader;
            }
            if (convertedValue.ValueWriter == null)
            {
                StringDictionaryPropertyValueWriter propertyValueWriter = new StringDictionaryPropertyValueWriter(stringValue);
                convertedValue.ValueWriter = (IValueWriter)propertyValueWriter;
            }
            return this.PositiveResult(convertedValue);
        }
    }
}
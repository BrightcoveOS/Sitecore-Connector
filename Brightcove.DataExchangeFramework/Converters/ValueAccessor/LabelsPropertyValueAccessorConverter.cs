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

namespace Brightcove.DataExchangeFramework.Converters
{
    [SupportedIds(new string[] { "{A0737430-A34E-4034-8F98-E15FBCC6948C}" })]
    public class LabelsPropertyValueAccessorConverter : ValueAccessorConverter
    {
        public const string FieldNamePropertyName = "PropertyName";

        public LabelsPropertyValueAccessorConverter(IItemModelRepository repository)
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
                LabelsPropertyValueReader propertyValueReader = new LabelsPropertyValueReader(stringValue);
                convertedValue.ValueReader = (IValueReader)propertyValueReader;
            }
            if (convertedValue.ValueWriter == null)
            {
                LabelsPropertyValueWriter propertyValueWriter = new LabelsPropertyValueWriter(stringValue);
                convertedValue.ValueWriter = (IValueWriter)propertyValueWriter;
            }
            return this.PositiveResult(convertedValue);
        }
    }
}
using Brightcove.DataExchangeFramework.Helpers;
using Brightcove.DataExchangeFramework.ValueReaders;
using Brightcove.DataExchangeFramework.ValueWriters;
using Sitecore.DataExchange;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters.DataAccess.ValueAccessors;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework
{
    [SupportedIds(new string[] { "{5719DF16-ED63-4668-B2E0-CF6531577244}" })]
    public class VideoIdsPropertyValueAccessorConverter : ValueAccessorConverter
    {
        public VideoIdsPropertyValueAccessorConverter(IItemModelRepository repository)
          : base(repository)
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
                VideoIdsPropertyValueReader propertyValueReader = new VideoIdsPropertyValueReader(stringValue);
                convertedValue.ValueReader = propertyValueReader;
            }
            
            if (convertedValue.ValueWriter == null)
            {
                VideoIdsPropertyValueWriter propertyValueWriter = new VideoIdsPropertyValueWriter(stringValue);
                convertedValue.ValueWriter = propertyValueWriter;
            }
           
            return PositiveResult(convertedValue);
        }
    }
}
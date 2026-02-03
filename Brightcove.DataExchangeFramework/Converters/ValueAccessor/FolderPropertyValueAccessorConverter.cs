using System;
using Brightcove.DataExchangeFramework.Helpers;
using Brightcove.DataExchangeFramework.ValueReaders;
using Sitecore.DataExchange;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters.DataAccess.ValueAccessors;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.DataAccess.Writers;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework.Converters
{
    [SupportedIds("{BEEC5E96-D7D6-4EB8-8472-E118C131340B}")]
    public class FolderPropertyValueAccessorConverter : ValueAccessorConverter
    {
        public FolderPropertyValueAccessorConverter(IItemModelRepository repository) : 
            base(repository)
        {
        }

        protected override IValueReader GetValueReader(ItemModel source)
        {
            string modelPropertyName = GetStringValue(source, FieldName.PropertyName);
            return new FolderPropertyValueReader(modelPropertyName);
        }
        protected override IValueWriter GetValueWriter(ItemModel source)
        {
            string modelPropertyName = GetStringValue(source, FieldName.PropertyName);
            return new PropertyValueWriter(modelPropertyName);
        }

        protected override ConvertResult<IValueAccessor> ConvertSupportedItem(ItemModel source)
        {
            try
            {
                ValueAccessor obj = new ValueAccessor
                {
                    ValueReader = GetValueReader(source),
                    ValueWriter = GetValueWriter(source)
                };
                return PositiveResult(obj);
            }
            catch (Exception ex)
            {
                Context.Logger.Error(ex.Message);
                throw ex;
            }

        }
    }
}
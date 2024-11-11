using Sitecore.DataExchange.DataAccess;
using System;
using System.Reflection;

namespace Brightcove.DataExchangeFramework.ValueWriters
{
    public class ChainedPropertyValueWriter : IValueWriter
    {
        public ChainedPropertyValueWriter(string propertyName)
        {
            this.PropertyName = !string.IsNullOrWhiteSpace(propertyName) ? propertyName : throw new ArgumentOutOfRangeException(nameof(propertyName), (object)propertyName, "Property name must be specified.");
            this.ReflectionUtil = (IReflectionUtil)Sitecore.DataExchange.DataAccess.Reflection.ReflectionUtil.Instance;
        }

        public string PropertyName { get; private set; }

        public IReflectionUtil ReflectionUtil { get; set; }

        public virtual bool Write(object target, object value, DataAccessContext context)
        {
            try
            {
                PropertyInfo propertyInfo = null;
                string[] properties = PropertyName.Split('.');

                for (int i = 0; i < properties.Length; i++)
                {
                    propertyInfo = this.ReflectionUtil.GetProperty(properties[i], target);

                    if (propertyInfo == (PropertyInfo)null || !propertyInfo.CanWrite)
                    {
                        return false;
                    }
                    //We may need to construct a property if it is null so we can access the next property (except if this is the last property)
                    else if (i < properties.Length - 1)
                    {
                        if (propertyInfo.GetValue(target) == null)
                        {
                            //If the value is null anyway dont bother constructing anything and just return immediately
                            if (value == null)
                                return true;

                            object newProperty = Activator.CreateInstance(propertyInfo.PropertyType);
                            propertyInfo.SetValue(target, newProperty);
                            target = newProperty;
                        }
                        else
                        {
                            target = propertyInfo.GetValue(target);
                        }
                    }
                }

                propertyInfo.SetValue(target, value);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
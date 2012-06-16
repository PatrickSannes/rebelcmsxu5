using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace Umbraco.Framework
{
    /// <summary>
    /// A custom type converter for the <see cref="HiveIdValue"/> type
    /// </summary>
    public class HiveIdTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            Mandate.ParameterNotNull(sourceType, "sourceType");

            return ((sourceType == typeof(Guid)) ||
                    (sourceType == typeof(int)) ||
                    (sourceType == typeof(string)) ||
                    (typeof(HiveId).IsAssignableFrom(sourceType) ||
                     base.CanConvertFrom(context, sourceType)));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            Mandate.ParameterNotNull(destinationType, "destinationType");

            return ((destinationType == typeof(InstanceDescriptor)) ||
                    (destinationType == typeof(Guid)) ||
                    (destinationType == typeof(int)) ||
                    (destinationType == typeof(string)) ||
                    ((destinationType == typeof(HiveId)) || base.CanConvertTo(context, destinationType)));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            // culture parameter can sometimes be null so don't mandate that
            Mandate.ParameterNotNull(value, "value");

            if(value is HiveId)
            {
                return value;
            }
            if (value is Guid)
            {
                return new HiveId((Guid)value);
            }
            if (value is int)
            {
                return new HiveId((int)value);
            }
            if (value is string)
            {
                var result = HiveId.TryParse((string) value);
                if (result.Success)
                    return result.Result;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            // culture parameter can sometimes be null so don't mandate that
            Mandate.ParameterNotNull(value, "value");
            Mandate.ParameterNotNull(destinationType, "destinationType");

            var castValue = (HiveId)value;
            if(destinationType == typeof(HiveId))
            {
                return castValue;
            }
            if (destinationType == typeof(Guid))
            {
                return Guid.Parse(castValue.Value.ToString());
            }
            if (destinationType == typeof(int))
            {
                return Int32.Parse(castValue.Value.ToString());
            }
            return destinationType == typeof(string) ? castValue.ToString() : base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
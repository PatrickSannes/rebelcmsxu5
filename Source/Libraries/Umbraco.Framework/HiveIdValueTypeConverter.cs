using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace Umbraco.Framework
{
    /// <summary>
    /// A custom type converter for the <see cref="HiveIdValue"/> type
    /// </summary>
    public class HiveIdValueTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            Mandate.ParameterNotNull(sourceType, "sourceType");

            return ((sourceType == typeof(Guid)) ||
                    (sourceType == typeof(int)) ||
                    (sourceType == typeof(string)) ||
                    (typeof(HiveIdValue).IsAssignableFrom(sourceType) ||
                     base.CanConvertFrom(context, sourceType)));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            Mandate.ParameterNotNull(destinationType, "destinationType");

            return ((destinationType == typeof(InstanceDescriptor)) ||
                    (destinationType == typeof(Guid)) ||
                    (destinationType == typeof(int)) ||
                    (destinationType == typeof(string)) ||
                    ((destinationType == typeof(HiveIdValue)) || base.CanConvertTo(context, destinationType)));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            // culture parameter can sometimes be null so don't mandate that
            Mandate.ParameterNotNull(value, "value");

            if (value.GetType() == typeof(Guid))
            {
                return new HiveIdValue((Guid)value);
            }
            if (value.GetType() == typeof(int))
            {
                return new HiveIdValue((int)value);
            }
            if (value.GetType() == typeof(string))
            {
                return new HiveIdValue((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            // culture parameter can sometimes be null so don't mandate that
            Mandate.ParameterNotNull(value, "value");
            Mandate.ParameterNotNull(destinationType, "destinationType");

            var castValue = (HiveIdValue)value;
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
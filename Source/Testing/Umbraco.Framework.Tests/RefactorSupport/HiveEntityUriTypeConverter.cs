using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using Umbraco.Framework;

namespace Umbraco.Tests.Framework.RefactorSupport
{
    /// <summary>
    /// A custom type converter for the HiveEntityUri object
    /// </summary>
    public class HiveEntityUriTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            Mandate.ParameterNotNull(sourceType, "sourceType");

            return ((sourceType == typeof(Guid)) ||
                    (sourceType == typeof(int)) ||
                    (sourceType == typeof(string)) ||
                    (typeof(HiveEntityUri).IsAssignableFrom(sourceType) ||
                     base.CanConvertFrom(context, sourceType)));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            Mandate.ParameterNotNull(destinationType, "destinationType");

            return ((destinationType == typeof(InstanceDescriptor)) ||
                    (destinationType == typeof(Guid)) ||
                    (destinationType == typeof(int)) ||
                    (destinationType == typeof(string)) ||
                     ((destinationType == typeof(HiveEntityUri)) || base.CanConvertTo(context, destinationType)));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            //Mandate.ParameterNotNull(culture, "culture");
            Mandate.ParameterNotNull(value, "value");

            if (value.GetType() == typeof(Guid))
            {
                return new HiveEntityUri((Guid)value);
            }
            if (value.GetType() == typeof(int))
            {
                return new HiveEntityUri((int)value);
            }
            if (value.GetType() == typeof(string))
            {
                return HiveEntityUri.Parse((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            //Mandate.ParameterNotNull(culture, "culture");
            Mandate.ParameterNotNull(value, "value");
            Mandate.ParameterNotNull(destinationType, "destinationType");

            var uri = value as HiveEntityUri;
            if (uri != null)
            {
                if (destinationType == typeof(Guid))
                {
                    return uri.AsGuid;
                }
                if (destinationType == typeof(int))
                {
                    return uri.AsInt;
                }
                if (destinationType == typeof(string))
                {
                    //return uri.GetAllStringParts();
                    return uri.ToString();
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
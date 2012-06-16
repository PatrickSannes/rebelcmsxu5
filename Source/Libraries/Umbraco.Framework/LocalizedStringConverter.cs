using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace Umbraco.Framework
{
    public class LocalizedStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            Mandate.ParameterNotNull(sourceType, "sourceType");
            return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            Mandate.ParameterNotNull(destinationType, "destinationType");
            return (destinationType == typeof(LocalizedString)) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null) return null;
            if (culture == null) culture = Thread.CurrentThread.CurrentCulture;

            Mandate.ParameterCondition(value.GetType() == typeof (string), "value");

            return new LocalizedString((string) value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null) return null;
            if (culture == null) culture = Thread.CurrentThread.CurrentCulture; 

            Mandate.ParameterNotNull(destinationType, "destinationType");

            return ((LocalizedString) value).GetValue(culture);
        }
    }
}
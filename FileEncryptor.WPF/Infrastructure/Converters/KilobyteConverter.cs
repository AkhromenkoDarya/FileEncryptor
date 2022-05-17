using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace FileEncryptor.WPF.Infrastructure.Converters
{
    [ValueConversion(typeof(long), typeof(double))]
    [MarkupExtensionReturnType(typeof(KilobyteConverter))]
    internal class KilobyteConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, 
            CultureInfo culture)
        {
            if (!(value is long))
            {
                return null;
            }

            var valueInBytes = System.Convert.ToInt64(value);
            var result = System.Convert.ToDouble((double)valueInBytes / 1024);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, 
            CultureInfo culture)
        {
            if (!(value is double))
            {
                return null;
            }

            var valueInBytes = System.Convert.ToDouble(value);
            var result = System.Convert.ToInt64(valueInBytes * 1024);
            return result;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}

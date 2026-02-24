using System;
using System.Globalization;
using System.Windows.Data;
using DigitalTwin.Services.Localization;

namespace DigitalTwin.Converters;

public class LocalizationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string key)
        {
            return LocalizationService.Instance.GetString(key);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

using System;
using System.Globalization;
using System.Windows.Data;
using DigitalTwin.Services.Localization;

namespace DigitalTwin.Converters;

public class TrackingButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isTracking)
        {
            var key = isTracking ? "Dashboard_StopTracking" : "Dashboard_StartTracking";
            return LocalizationService.Instance.GetString(key);
        }
        return LocalizationService.Instance.GetString("Dashboard_StartTracking");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

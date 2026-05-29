using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LiveFeedback.Shared.Enums;

namespace LiveFeedback.Converters;

public class UriStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is UriStatus uriStatus)
        {
            return uriStatus switch
            {
                UriStatus.Reachable => new SolidColorBrush(Colors.Green),
                UriStatus.Valid => new SolidColorBrush(Colors.DodgerBlue),
                _ => new SolidColorBrush(Colors.Red)
            };
        }
        return new SolidColorBrush(Colors.Red);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
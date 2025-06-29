using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace LiveFeedback.Converters;

public class EqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo _)
    {
        return value?.Equals(parameter) == true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo _)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
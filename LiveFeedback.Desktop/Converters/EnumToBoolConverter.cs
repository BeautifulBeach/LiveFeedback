using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace LiveFeedback.Converters;

public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo _)
        => value?.Equals(parameter) == true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo _)
    {
        if (value == null)
            return AvaloniaProperty.UnsetValue;

        return (bool)value ? parameter! : AvaloniaProperty.UnsetValue;
    }
}
using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace LiveFeedback.Converters;

public class BoolToGridLengthConverter : IValueConverter
{
    public double TrueLength { get; set; } = 500;

    public double FalseLength { get; set; } = 0;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isRunning = false;
        if(value is bool b)
        {
            isRunning = b;
        }
        return new GridLength(isRunning ? TrueLength : FalseLength, GridUnitType.Pixel);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GridLength length)
        {
            return length.Value > 0;
        }
        return false;
    }
}
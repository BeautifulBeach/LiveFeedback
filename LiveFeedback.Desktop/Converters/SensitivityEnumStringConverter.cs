using System;
using System.Globalization;
using Avalonia.Data.Converters;
using LiveFeedback.Models;

namespace LiveFeedback.Converters;

public class SensitivityEnumStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Sensitivity v)
        {
            return v switch
            {
                Sensitivity.High => "hoch",
                Sensitivity.Medium => "mittel",
                Sensitivity.Low => "niedrig",
                _ => throw new NotImplementedException()
            };
        }

        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            return s switch
            {
                "hoch" => Sensitivity.High,
                "mittel" => Sensitivity.Medium,
                "niedrig" => Sensitivity.Low,
                _ => Sensitivity.Medium
            };
        }

        return Sensitivity.High;
    }
}
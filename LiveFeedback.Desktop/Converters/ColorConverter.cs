using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LiveFeedback.Converters;

public class ColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ushort v)
        {
            return new SolidColorBrush(Colors.Transparent);
        }

        double normalized = v / 100.0;

        // Color stops
        Color[] stops = new[]
        {
            Color.FromArgb(255, 0, 50, 255),
            Color.FromArgb(255, 0, 200, 180),
            Color.FromArgb(200, 0, 200, 0),
            Color.FromArgb(255, 255, 210, 0),
            Color.FromArgb(220, 220, 20, 30)
        };

        // Relative positions of the stopps (normalized)
        double[] positions = [0.0, 0.25, 0.50, 0.75, 1.0];

        // Find the right segment index
        int segment = 0;
        for (int i = 0; i < positions.Length - 1; i++)
        {
            if (normalized >= positions[i] && normalized <= positions[i + 1])
            {
                segment = i;
                break;
            }
        }

        // Calculate the local interpolation value t in the segment
        double t = (normalized - positions[segment]) / (positions[segment + 1] - positions[segment]);

        // Linear interpolation (Lerp) for each color channel
        byte r = (byte)(stops[segment].R + (stops[segment + 1].R - stops[segment].R) * t);
        byte g = (byte)(stops[segment].G + (stops[segment + 1].G - stops[segment].G) * t);
        byte b = (byte)(stops[segment].B + (stops[segment + 1].B - stops[segment].B) * t);

        Color color = Color.FromArgb(255, r, g, b);
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
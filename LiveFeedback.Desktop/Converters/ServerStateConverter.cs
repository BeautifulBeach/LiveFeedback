using System;
using System.Globalization;
using Avalonia.Data.Converters;
using LiveFeedback.Models;

namespace LiveFeedback.Converters;

public class ServerStateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ServerState serverState)
        {
            return serverState switch
            {
                ServerState.Starting => "Starten…",
                ServerState.Running => "Stoppen",
                ServerState.Stopping => "Stoppen…",
                ServerState.Stopped => "Starten",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string serverState)
        {
            return serverState switch
            {
                "Starten…" => ServerState.Starting,
                "Stoppen" => ServerState.Running,
                "Beenden…" => ServerState.Stopping,
                "Starten" => ServerState.Stopped,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return ServerState.Stopped;
    }
}
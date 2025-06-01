using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using LiveFeedback.Models;
using LiveFeedback.ViewModels;
using LiveFeedback.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LiveFeedback.Services;

public class OverlayWindowService(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private List<OverlayWindow> _overlayWindowsOpened = [];

    public async Task ShowWindowOnAllScreensAsync(OverlayPosition overlayPosition = OverlayPosition.BottomRight)
    {
        var desktopLifetime =
            Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        if (desktopLifetime == null)
            return;

        Window? mainWindow = desktopLifetime.MainWindow;
        if (mainWindow == null)
            return;

        IReadOnlyList<Screen> screens = mainWindow.Screens.All;

        foreach (Screen screen in screens)
        {
            var window = _serviceProvider.GetRequiredService<OverlayWindow>();
            window.DataContext = _serviceProvider.GetRequiredService<OverlayWindowViewModel>();
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Position = CalculateWindowPosition(overlayPosition, screen, window);
            window.Show();
            _overlayWindowsOpened.Add(window);
            await Task.CompletedTask;
        }
    }

    public async Task HideWindowOnAllScreensAsync()
    {
        foreach (OverlayWindow window in _overlayWindowsOpened)
        {
            window.Close();
        }
        await Task.CompletedTask;
    }

    public static PixelPoint CalculateWindowPosition(OverlayPosition overlayPosition, Screen screen, Window window)
    {
        PixelSize windowSize = PixelSize.FromSize(window.ClientSize, screen.Scaling);
        PixelRect fullBounds = screen.Bounds;

        // Default position: bottom right
        int newX;
        int newY;

        switch (overlayPosition)
        {
            case OverlayPosition.BottomRight:
                newX = fullBounds.X + fullBounds.Width - windowSize.Width;
                newY = fullBounds.Y + fullBounds.Height - windowSize.Height;
                break;
            case OverlayPosition.BottomLeft:
                newX = fullBounds.X;
                newY = fullBounds.Y + fullBounds.Height - windowSize.Height;
                break;
            case OverlayPosition.TopLeft:
                newX = fullBounds.X;
                newY = fullBounds.Y;
                break;
            case OverlayPosition.TopRight:
                newX = fullBounds.X + fullBounds.Width - windowSize.Width;
                newY = fullBounds.Y;
                break;
            default:
                newX = fullBounds.X + fullBounds.Width - windowSize.Width;
                newY = fullBounds.Y + fullBounds.Height - windowSize.Height;
                break;
        }

        return new PixelPoint(newX, newY);
    }
}

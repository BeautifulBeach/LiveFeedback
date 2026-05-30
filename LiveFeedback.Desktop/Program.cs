using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.Shared;
using LiveFeedback.ViewModels;
using LiveFeedback.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveFeedback;

internal static class Program
{
    public static IServiceProvider Services { get; private set; } = null!;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
        DesktopProgramConfig initialConfig = await LocalConfigService.GetInitialConfig();
        Services = ConfigureServices(initialConfig);
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }

    private static ServiceProvider ConfigureServices(DesktopProgramConfig initialConfig)
    {
        GlobalConfig globalConfig = new();
        ServiceCollection services = [];
        services.AddSingleton(initialConfig);
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        services.AddSingleton(globalConfig);
        services.AddSingleton<LocalConfigService>();
        services.AddSingleton<AppState>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<OverlayWindow>();
        services.AddSingleton<OverlayWindowViewModel>();
        services.AddSingleton<OverlayWindowService>();
        services.AddSingleton<SignalRService>();
        services.AddSingleton<ServerService>();
        services.AddSingleton<PositionSelectorViewModel>();
        services.AddSingleton<SettingsWindowViewModel>();
        services.AddTransient<SettingsWindow>();
        services.AddTransient<IClipboard>(_ =>
        {
            var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var topLevel = TopLevel.GetTopLevel(lifetime?.MainWindow);
            return topLevel?.Clipboard ?? throw new InvalidOperationException("Clipboard is not available yet");
        });

        return services.BuildServiceProvider();
    }
}
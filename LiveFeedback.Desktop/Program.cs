using System;
using System.Threading.Tasks;
using Avalonia;
using LiveFeedback.Services;
using LiveFeedback.Shared;
using LiveFeedback.ViewModels;
using LiveFeedback.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveFeedback;

internal static class Program
{
    public static readonly IServiceProvider Services = ConfigureServices();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
        await Services.GetRequiredService<LocalConfigService>().Setup();

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

    private static ServiceProvider ConfigureServices()
    {
        GlobalConfig globalConfig = new();
        ServiceCollection services = [];
        services.AddSingleton(globalConfig);
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
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
        return services.BuildServiceProvider();
    }
}
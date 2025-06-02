using System;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using LiveFeedback.Services;
using LiveFeedback.ViewModels;
using LiveFeedback.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveFeedback;

internal sealed class Program
{
    public static IServiceProvider Services { get; private set; } = null!;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
        ConfigureServices();
        await Services.GetRequiredService<LocalConfig>().Initialize();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }

    private static void ConfigureServices()
    {
        Shared.GlobalConfig globalConfig = new();
        ServiceCollection serviceCollection = [];
        serviceCollection.AddSingleton(globalConfig);
        serviceCollection.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        serviceCollection.AddSingleton<MainWindow>();
        serviceCollection.AddSingleton<LocalConfig>();
        serviceCollection.AddSingleton<MainWindowViewModel>();
        serviceCollection.AddTransient<OverlayWindow>();
        serviceCollection.AddSingleton<OverlayWindowViewModel>();
        serviceCollection.AddSingleton<OverlayWindowService>();
        serviceCollection.AddSingleton<SignalRService>();
        serviceCollection.AddSingleton<ServerService>();
        serviceCollection.AddSingleton<AppState>();
        serviceCollection.AddSingleton<PositionSelectorViewModel>();

        Services = serviceCollection.BuildServiceProvider();
    }
}

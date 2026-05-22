using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LiveFeedback.Services;
using LiveFeedback.Shared;
using LiveFeedback.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveFeedback;

public class App : Application
{
    private readonly ILogger<App> _logger = Program.Services.GetService<ILogger<App>>()!;
    private readonly ServerService _serverService = Program.Services.GetService<ServerService>()!;
    private readonly OverlayWindowService _overlayWindowService = Program.Services.GetService<OverlayWindowService>()!;
    private readonly GlobalConfig _globalConfig = Program.Services.GetService<GlobalConfig>()!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            desktop.ShutdownRequested += ShutDownLiveFeedback;

            desktop.MainWindow = Program.Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
        _logger.LogInformation("Application started successfully");
    }

    private void ShutDownLiveFeedback(object? sender, ShutdownRequestedEventArgs args)
    {
        _logger.LogInformation("Shutting down LiveFeedback ({Mode} mode)",
            _globalConfig.Mode.ToString().ToLower());
        Task.WhenAll(_overlayWindowService.HideWindowOnAllScreensAsync(), _serverService.StopServerAsync());
    }
}
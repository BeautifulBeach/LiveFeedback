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
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            desktop.ShutdownRequested += ShutDownLiveFeedback;

            desktop.MainWindow = Program.Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
        _logger.LogInformation("Application started successfully");
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        DataAnnotationsValidationPlugin[] dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (DataAnnotationsValidationPlugin plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private void ShutDownLiveFeedback(object? sender, ShutdownRequestedEventArgs args)
    {
        _logger.LogInformation("Shutting down LiveFeedback ({Mode} mode)",
            _globalConfig.Mode.ToString().ToLower());
        Task.WhenAll(_overlayWindowService.HideWindowOnAllScreensAsync(), _serverService.StopServerAsync());
    }
}
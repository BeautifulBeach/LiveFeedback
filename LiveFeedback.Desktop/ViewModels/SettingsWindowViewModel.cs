using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.Shared.Enums;
using ReactiveUI;

namespace LiveFeedback.ViewModels;

public class SettingsWindowViewModel : ReactiveObject
{
    public SettingsWindowViewModel(AppState appState, LocalConfigService localConfigService)
    {
        AppState = appState;
        _isModeExpanded = localConfigService.GetMode() == Mode.Distributed;
        ExternalServersConfigured = localConfigService.GetExternalServer() != null;
        ExternalServers = new ObservableCollection<ServerConfig>(localConfigService.GetExternalServers());

        this.WhenAnyValue(x => x.ExternalServers)
            .Subscribe(newExternalServers =>
            {
                Console.WriteLine($"New external servers: {JsonSerializer.Serialize(newExternalServers)}");
            });
    }

    public AppState AppState { get; }

    private bool _isModeExpanded;

    public bool IsModeExpanded
    {
        get => _isModeExpanded;
        set => this.RaiseAndSetIfChanged(ref _isModeExpanded, value);
    }

    public bool ExternalServersConfigured { get; set; }

    public ObservableCollection<ServerConfig> ExternalServers { get; set; }
}
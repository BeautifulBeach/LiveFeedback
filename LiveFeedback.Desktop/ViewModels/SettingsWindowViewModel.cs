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
        ExternalServersAvailable = localConfigService.GetExternalServers().Count > 0;
        ExternalServers = new ObservableCollection<ServerConfig>(localConfigService.GetExternalServers());

        this.WhenAnyValue(x => x.ExternalServers)
            .Subscribe(newExternalServers =>
            {
                Console.WriteLine($"New external server data!");
            });
    }

    public AppState AppState { get; }

    public bool ExternalServersAvailable { get; set; }

    public ObservableCollection<ServerConfig> ExternalServers { get; set; }
}
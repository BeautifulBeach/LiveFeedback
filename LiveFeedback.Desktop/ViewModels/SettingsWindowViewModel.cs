using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveFeedback.Models;
using LiveFeedback.Services;

namespace LiveFeedback.ViewModels;

public partial class SettingsWindowViewModel(AppState appState, LocalConfigService localConfigService) : ObservableObject
{
    public AppState AppState { get; } = appState;

    public bool ExternalServersAvailable { get; set; } = localConfigService.GetExternalServers().Count > 0;

    public ObservableCollection<ServerConfig> ExternalServers { get; set; } = new(localConfigService.GetExternalServers());
    
}
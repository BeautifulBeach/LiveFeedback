using LiveFeedback.Models;
using LiveFeedback.Services;
using ReactiveUI;

namespace LiveFeedback.ViewModels;

public class SettingsWindowViewModel(AppState appState, LocalConfig localConfig) : ReactiveObject
{
    public AppState AppState { get; set; }= appState;
    public LocalConfig LocalConfig { get; set; } = localConfig;
}
using CommunityToolkit.Mvvm.ComponentModel;
using LiveFeedback.Services;

namespace LiveFeedback.ViewModels;

public partial class OverlayWindowViewModel : ObservableObject
{
    [ObservableProperty] public partial AppState AppState { get; set; }

    public OverlayWindowViewModel(AppState appState)
    {
        AppState = appState;
    }
}
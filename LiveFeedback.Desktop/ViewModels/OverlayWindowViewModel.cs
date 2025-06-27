using System;
using LiveFeedback.Services;
using ReactiveUI;

namespace LiveFeedback.ViewModels;

public class OverlayWindowViewModel : ReactiveObject
{
    public AppState AppState { get; set; }

    public OverlayWindowViewModel(AppState appState)
    {
        AppState = appState;
        AppState.WhenAnyValue(x => x.MinimalUserCount)
            .Subscribe(newUserCount => { EnoughParticipants = newUserCount >= AppState.MinimalUserCount; });
    }
    
    public bool EnoughParticipants { get; set; }
}
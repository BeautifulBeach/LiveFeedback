using LiveFeedback.Services;
using LiveFeedback.Shared.Models;
using ReactiveUI;
using System;
using System.Diagnostics;
using LiveFeedback.Core;
using LiveFeedback.Models;

namespace LiveFeedback.ViewModels;

public class OverlayWindowViewModel : ReactiveObject
{
    private AppState _appState;

    private ComprehensibilityInformation _currentComprehensibility;

    public ComprehensibilityInformation CurrentComprehensibility
    {
        get => _currentComprehensibility;
        set => this.RaiseAndSetIfChanged(ref _currentComprehensibility, value);
    }

    public OverlayWindowViewModel(AppState appState)
    {
        _appState = appState;
        _appState.WhenAnyValue(x => x.CurrentComprehensibility)
            .Subscribe((ComprehensibilityInformation v) =>
            {
                EnoughParticipants = v.UsersInvolved >= _appState.MinimalUserCount;
                CurrentComprehensibility = Calculator.CalculateComprehensibilityWithSensitivity(v, appState);
            });

        _appState.WhenAnyValue(x => x.Sensitivity)
            .Subscribe(newSensitivity =>
            {
                CurrentComprehensibility = Calculator.CalculateComprehensibilityWithSensitivity(_appState.CurrentComprehensibility, appState);
            });

        _appState.WhenAnyValue(x => x.MinimalUserCount)
            .Subscribe(newUserCount =>
            {
                EnoughParticipants = newUserCount >= _appState.MinimalUserCount;
            });

        _currentComprehensibility = new ComprehensibilityInformation()
        {
            UsersInvolved = 0,
            IndividualRatings = [],
            OverallRating = Shared.Constants.DefaultRating
        };
    }
    
    private bool _enoughParticipants;

    public bool EnoughParticipants
    {
        get => _enoughParticipants;
        set => this.RaiseAndSetIfChanged(ref _enoughParticipants, value);
    }
}
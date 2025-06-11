using System;
using LiveFeedback.Core;
using LiveFeedback.Services;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Models;
using ReactiveUI;

namespace LiveFeedback.ViewModels;

public class OverlayWindowViewModel : ReactiveObject
{
    private readonly AppState _appState;

    private ComprehensibilityInformation _currentComprehensibility;

    private bool _enoughParticipants;

    public OverlayWindowViewModel(AppState appState)
    {
        _appState = appState;
        _appState.WhenAnyValue(x => x.CurrentComprehensibility)
            .Subscribe(v =>
            {
                EnoughParticipants = v.UsersInvolved >= _appState.MinimalUserCount;
                CurrentComprehensibility = Calculator.CalculateComprehensibilityWithSensitivity(v, appState);
            });

        _appState.WhenAnyValue(x => x.Sensitivity)
            .Subscribe(newSensitivity =>
            {
                CurrentComprehensibility =
                    Calculator.CalculateComprehensibilityWithSensitivity(_appState.CurrentComprehensibility,
                        appState);
            });

        _appState.WhenAnyValue(x => x.MinimalUserCount)
            .Subscribe(newUserCount => { EnoughParticipants = newUserCount >= _appState.MinimalUserCount; });

        _currentComprehensibility = new ComprehensibilityInformation
        {
            UsersInvolved = 0,
            IndividualRatings = [],
            OverallRating = Constants.DefaultRating
        };
    }

    public ComprehensibilityInformation CurrentComprehensibility
    {
        get => _currentComprehensibility;
        set => this.RaiseAndSetIfChanged(ref _currentComprehensibility, value);
    }

    public bool EnoughParticipants
    {
        get => _enoughParticipants;
        set => this.RaiseAndSetIfChanged(ref _enoughParticipants, value);
    }
}
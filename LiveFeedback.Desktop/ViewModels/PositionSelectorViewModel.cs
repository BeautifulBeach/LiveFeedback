using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveFeedback.Models;
using LiveFeedback.Services;

namespace LiveFeedback.ViewModels;

public partial class PositionSelectorViewModel : ObservableObject
{
    [ObservableProperty] private partial AppState AppState { get; set; }

    private readonly LocalConfigService _localConfigService;

    [ObservableProperty] public partial string BlBtnColor { get; set; }

    [ObservableProperty]
    public partial string BrBtnColor { get; set; }

    [ObservableProperty]
    public partial string TlBtnColor { get; set; }

    [ObservableProperty]
    public partial string TrBtnColor { get; set; }

    public PositionSelectorViewModel(AppState appState, LocalConfigService localConfigService)
    {
        AppState = appState;
        _localConfigService = localConfigService;
        TlBtnColor = BtnColor("tl");
        TrBtnColor = BtnColor("tr");
        BrBtnColor = BtnColor("br");
        BlBtnColor = BtnColor("bl");
    }

    public void SelectCornerCommand(string corner)
    {
        DeactivateAnyButton();
        switch (corner)
        {
            case "tl":
                AppState.OverlayPosition = OverlayPosition.TopLeft;
                TlBtnColor = "DodgerBlue";
                break;
            case "tr":
                AppState.OverlayPosition = OverlayPosition.TopRight;
                TrBtnColor = "DodgerBlue";
                break;
            case "bl":
                AppState.OverlayPosition = OverlayPosition.BottomLeft;
                BlBtnColor = "DodgerBlue";
                break;
            case "br":
                AppState.OverlayPosition = OverlayPosition.BottomRight;
                BrBtnColor = "DodgerBlue";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _localConfigService.SaveOverlayPosition(AppState.OverlayPosition);
    }

    private void DeactivateAnyButton()
    {
        TlBtnColor = "Transparent";
        TrBtnColor = "Transparent";
        BlBtnColor = "Transparent";
        BrBtnColor = "Transparent";
    }

    private string BtnColor(string btn)
    {
        return AppState.OverlayPosition switch
        {
            OverlayPosition.TopLeft => btn == "tl" ? "DodgerBlue" : "Transparent",
            OverlayPosition.TopRight => btn == "tr" ? "DodgerBlue" : "Transparent",
            OverlayPosition.BottomRight => btn == "br" ? "DodgerBlue" : "Transparent",
            OverlayPosition.BottomLeft => btn == "bl" ? "DodgerBlue" : "Transparent",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}